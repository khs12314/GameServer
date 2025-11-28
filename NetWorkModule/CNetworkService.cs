using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NetWorkModule;


namespace NetWorkModule
{
    public class CNetworkService
    {
        int connected_count;


        //클라이언트 접속을 받아들이기 위한 객체
        CListener cleint_listener;


        //메시지 수신용 풀.
        //메시지 송수신시 필요한객체
        ScoketAsyncEventArgsPool receive_event_args_pool;
        //메시지 송신용 풀
        ScoketAsyncEventArgsPool send_event_args_pool;

        //메시지 수신, 전송 시 닷넷 비동기 소켓에서 사용할 버퍼를 관리하는 객체
        BufferManger buffer_manger;



        // 클라이언트 접속이 이루어졌을 때 호출되는 델리 게이트
        public delegate void SessionHandler(CUserToken token);
        int max_connections;
        int buffer_size;
        readonly int pre_alloc_count = 2;


        //msdn 샘플코드 그대로 사용
        public SessionHandler session_created_callback { get; set; }




        public CNetworkService()
        {
            this.connected_count = 0;
            this.session_created_callback = null;
        }
        // Initializes the server by preallocating reusable buffers and 
        // context objects.  These objects do not need to be preallocated 
        // or reused, but it is done this way to illustrate how the API can 
        // easily be used to create reusable objects to increase server performance.

        public void initialize()
        {

            this.max_connections = 10000;
            this.buffer_size = 1024;
            //버퍼 전체의 크기 = 최대 동시 접속자 수 * 버퍼 하나의 크기 * 송수신 각각 하나씩 할당)
            this.buffer_manger = new BufferManger(this.max_connections * this.buffer_size * this.pre_alloc_count, this.buffer_size);

            this.receive_event_args_pool = new ScoketAsyncEventArgsPool(this.max_connections);
            this.send_event_args_pool = new ScoketAsyncEventArgsPool(this.max_connections);
            this.buffer_manger.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs arg;
            for (int i = 0; i < this.max_connections; i++)
            {
                // 동일한 소켓에 대고 send, receive를 하므로
                // user token은 세션별로 하나씩만 만들어 놓고 
                // receive, send EventArgs에서 동일한 token을 참조하도록 구성한다.
                CUserToken token = new CUserToken();
                //recvie pool
                {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(receive_completed);
                    arg.UserToken = token;

                    this.buffer_manger.SetBuffer(arg);//1 버퍼의 크기만큼 인덱스 값을 늘려 객체화 메모리 풀링
                    this.receive_event_args_pool.Push(arg);
                }

                //send pool
                {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(send_completed);
                    arg.UserToken = token;

                    this.buffer_manger.SetBuffer(arg);

                    this.send_event_args_pool.Push(arg);
                }
            }
        }
        public void listen(string host, int port, int backlog)
        {
            this.cleint_listener = new CListener();
            this.cleint_listener.callback_on_newclient += on_new_client;
            this.cleint_listener.start(host, port, backlog);
        }
        /// <summary>
        /// todo:검토중....
        /// 원격 서버에 접속 성공 했을 때 호출됩니다.
        /// </summary>
        /// <param name="socket"></param>
        public void on_connect_completed(Socket socket, CUserToken token)
        {
            //socketAsyncEventArgsPool에서 빼오지 않고 그때 그떄 할당해서 사용
            //풀은 서버에서 클라와 통신용으로만 쓰려고 만든 것
            //클라이언트 입장에서 서버와 통신을 할 때는 접속한 서버당 두개의 EventArgs만 있으면 되기 때문에 그냥 new해서 쓴다
            //서버간 연결에서도 마찬가지이다.
            //풀링처리를 하려면 c->s로 가는 별도의 풀을 만들어서 써야한다.

            SocketAsyncEventArgs receive_event_arg = new SocketAsyncEventArgs();
            receive_event_arg.Completed += new EventHandler<SocketAsyncEventArgs>(receive_completed);
            receive_event_arg.UserToken = token;
            receive_event_arg.SetBuffer(new byte[1024], 0, 1024);

            SocketAsyncEventArgs send_event_arg = new SocketAsyncEventArgs();
            send_event_arg.Completed += new EventHandler<SocketAsyncEventArgs>(send_completed);
            send_event_arg.UserToken = token;
            send_event_arg.SetBuffer(new byte[1024], 0, 1024);

            begin_receive(socket, receive_event_arg, send_event_arg);

        }
        ///<summary>
        ///새로운 클라이언트가 접속 성공 했을 때 호출됩니다.
        ///AcceptAsync의 콜백 메소드에서 호출되며 여러 스레드에서 동시에 호출될 수 있기 때문에 공유자원에 접근할 때는 주의해야 합니다.
        ///</summary>
        ///<param name="client_socket"></param>
        void on_new_client(Socket client_socket, object token)
        {
            //todo:
            // peer list 처리.
            Interlocked.Increment(ref this.connected_count);

            Console.WriteLine(string.Format("[{0}] A client connected. handle {1}, count {2}",
                Thread.CurrentThread.ManagedThreadId, client_socket.Handle,
                this.connected_count));

            //풀에서 하나 꺼내와 사용한다.
            SocketAsyncEventArgs receive_args = this.receive_event_args_pool.Pop();
            SocketAsyncEventArgs send_args = this.send_event_args_pool.Pop();

            CUserToken user_token = null;
            user_token = receive_args.UserToken as CUserToken;
            user_token.socket = client_socket;
            user_token.set_event_args(receive_args, send_args);
            if (this.session_created_callback != null)
            {
                this.session_created_callback(user_token);
            }

            begin_receive(client_socket, receive_args, send_args);
            //user_token.start_keepalive();
        }

        void begin_receive(Socket socket, SocketAsyncEventArgs receive_args, SocketAsyncEventArgs send_args)
        {
            //receive_args, send_args 아무곳에서나 꺼내와도 된다. 둘 다 동일한 CUserToken을 참조하고 있다.
            var token = receive_args.UserToken as CUserToken;
            token.set_event_args(receive_args, send_args);
            //생성된 클라이언트 소켓을 보관해 놓고 통신할 때 사용한다.
            token.socket = socket;

            bool pending = socket.ReceiveAsync(receive_args);
            if (!pending)
            {
                process_receive(receive_args);
            }

        }
        //This method is called whenever a receive or send operation is completed on a socket
        //
        //<param name = "e">SocketAsyncEventArg associated with the completed receive operation</param>    
        void receive_completed(object sender, SocketAsyncEventArgs e)
        {
            process_receive(e);
            return;
        }
        //This method is called whenever a receive or send operation is completed on a socket
        //
        //<param name = "e">SocketAsyncEventArg associated with the completed send operation</param>    
        void send_completed(object sender, SocketAsyncEventArgs e)
        {
            CUserToken token = e.UserToken as CUserToken;
            token.process_send(e);

        }
        // This method is invoked when an asynchronous receive operation completes. 
        // If the remote host closed the connection, then the socket is closed.  
        //
        void process_receive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            CUserToken token = e.UserToken as CUserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                token.on_receive(e.Buffer, e.Offset, e.BytesTransferred);

                bool pending = token.socket.ReceiveAsync(e);
                if (!pending)
                {
                    // Oh! stack overflow??
                    process_receive(e);
                }
            }
            else
            {
                Console.WriteLine(string.Format("error {0},  transferred {1}", e.SocketError, e.BytesTransferred));
                close_clientsocket(token);
            }
        }
        public void close_clientsocket(CUserToken token)
        {
            token.on_removed();

            //Free the SocketAsyncEventArg so they can be reused by another client
            //버퍼는 반환할 필요가 없다. SocketAsyncEventArg가 버퍼를 들고 있기 때문에
            //이것을 재사용 할 때 물고 있는 버퍼를 그대로 사용되기 때문이다.
            if (this.receive_event_args_pool != null)
            {
                this.receive_event_args_pool.Push(token.receive_event_args);
            }

            if (this.send_event_args_pool != null)
            {
                this.send_event_args_pool.Push(token.send_event_args);
            }
        }










    }
}
//buffer_manger = new BufferManger();

