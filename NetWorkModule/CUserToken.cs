using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

using System.Text;
using System.Threading.Tasks;

namespace NetWorkModule
{

    public class CUserToken
    {
        public Socket socket { get; set; }

        public SocketAsyncEventArgs receive_event_args { get; private set; }
        public SocketAsyncEventArgs send_event_args { get; private set; }
        //바이트를 패킷 형식으로 해석해주는 해석기
        CMessage_resolver message_resolver;
        List<ArraySegment<byte>> _pendingList;
        List<ArraySegment<byte>> _sendingList;
        List<CPacket> _sendList;
        //session객체, 어플리케이션 단에서 구현하여 사용
        IPeer peer;
        CMessage_resolver.CompltedMessageCallback _onMessageCallback;
        //전송할 패킷을 보관해놓는 큐.1-send로 처리하기 위한 큐이다.
        Queue<CPacket> sending_queue;
        //Sending_queue lock처리에 사용되는 객체
        private object cs_sending_queue;

        public CUserToken()
        {
            this.cs_sending_queue = new object();

            this.message_resolver = new CMessage_resolver();

            this.peer = null;
            this.sending_queue = new Queue<CPacket>();
            _pendingList = new List<ArraySegment<byte>>();
            _sendList = new List<CPacket>();
            this._onMessageCallback = new CMessage_resolver.CompltedMessageCallback(on_message);
        }
        public void set_peer(IPeer peer)
        {
            this.peer = peer;
        }
        public void set_event_args(SocketAsyncEventArgs receive_event_args, SocketAsyncEventArgs send_event_args)
        {
            this.receive_event_args = receive_event_args;
            this.send_event_args = send_event_args;
        }

        /// <summary>
        /// 소켓 버퍼로부터 데이터를 수신할 때마다 호출된다.
        /// 데이터가 남아 있을 때까지 계속 패킷을 만들어 callback을 호출해준다.
        /// 하나의 패킷을 완성하지 못했다면 버퍼에 보관해 놓운 뒤 다음수신을 기다린다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="transferred"></param>
        public void on_receive(byte[] buffer, int offset, int transfered)
        {
            this.message_resolver.on_receive(buffer, offset, transfered, this._onMessageCallback);
        }
        void on_message(Const<byte[]> buffer)
        {

            if (this.peer != null)
            {
                this.peer.on_message(buffer);
            }
        }
        public void on_removed()
        {
            // [수정] 연결이 끊길 때, 큐에 남은 패킷들을 전부 구조해서 반납해야 함
            lock (this.cs_sending_queue)
            {
                while (this.sending_queue.Count > 0)
                {
                    CPacket packet = this.sending_queue.Dequeue();
                    packet.Release(); // "집으로 돌아가라"
                }
            }

            if (this.peer != null)
            {
                this.peer.on_removed();
            }
        }




        /// <summary>
        /// 패킷을 전송한다    
        /// 큐가 비어있다면 큐에 추가한 뒤 바로 sendasync 메서드 호출
        /// 데이터가 들어있을 경우 새로 추가
        /// 
        /// 큐잉된 패킷의  전송 시점 : 현재 진행 중인 sendasync가 완료 되었을 때 검사해 나머지 패킷 전송
        /// </summary>
        /// <param name="msg"></param>

        //전송 로직
        //1)패킷을 만들어 전송 큐에 추가
        //2) 큐가 비어있다면 즉시 비동기 전송 메소드 호출
        //3) 전송 오퍼레이션이 완료된 후 다시 큐를 검사
        //4) 데이터가 존재할 경우 다시 비동기 전송 메소드 호출
        //5) 데이터가 없을 경우 더 이상 아무런 작업을 하지 않고 리턴
        public void send(CPacket msg)
        {



            lock (this.cs_sending_queue)
            {
                //큐가 비어있다면 큐에 추가, 바로 비동기 전송 메소드 호출
                if (this.sending_queue.Count <= 0)
                {
                    this.sending_queue.Enqueue(msg);

                    start_send();
                    return;
                }

                //큐에 무언가가 들어 있다면 아직 이전 전송이 완료 되지않은 상태이므로
                //큐에 추가만 하고 리턴
                //현재 수행중인 sendasync가 완료된 이후 큐를 검사해 데이터가 있으면 sendasync를 호출해 전송해 줄 것이다.
                this.sending_queue.Enqueue(msg);
            }
        }
        /// <summary>
        /// 비동기 전송을 시작한다.
        /// </summary>
        //비동기 전송 시나리오
        //1) 전송 요청 -> 큐에 데이터 추가
        //2) 큐에서 데이터를 하나 꺼내옵니다.(Peek 대신 Dequeue사용)
        //3) Dequeue를 사용해 큐는 Empty 상태가 된다.
        //4) SendAsync 호출 -> 비동기 메소드이기 때문에 다른 스레드에서 전송 작업 진행
        //5) 또 다른 데이터의 전송 요청
        //6) 큐가 비어있기 때문에 즉시 start_send 메소드를 호출해 그 안에서 sendAsync가 호출된다.

        void start_send()//1
        {
            lock (this.cs_sending_queue)
            {
                //전송이 아직 완료된 상태가 아니므로 데이터만 가져오고 큐에서 제거하진 않는다.
                CPacket msg = null;
               
                while (this.sending_queue.Count > 0)
                {
                    msg = this.sending_queue.Dequeue();
                    msg.record_size();
                    _sendList.Add(msg);
                    _pendingList.Add(new ArraySegment<byte>(msg.GetBuffer(), 0, msg.position));
                }
                
                //헤더에 패킷 사이즈를 기록

                //이번에 보낼 패킷 사이즈만큼 버퍼 크기를 설정하고

                send_event_args.BufferList = _pendingList;

                //비동기 전송 시작 2
                bool pending = this.socket.SendAsync(this.send_event_args);
                if (!pending)
                {
                    process_send(this.send_event_args);
                }
            }
        }



        static int sent_count = 0;
        static object cs_count = new object();
        /// <summary>
        /// 비동기 전송 완료시 호출되는 콜백 메소드
        ///</summary>   
        ///<param name="e"></param>"

        public void process_send(SocketAsyncEventArgs e)
        {
            int expectedSize = 0;
            foreach (var seg in _pendingList) // _pendingList는 아직 안 비웠으니 살아있음
                expectedSize += seg.Count;
            Console.WriteLine($"보낸 바이트{e.BytesTransferred}");
            // [수정 1] 에러 처리 시에도 Lock 필수!
            if (e.BytesTransferred != expectedSize)
            {
                lock (this.cs_sending_queue)
                {
                    disconnect();
                    // 에러가 났어도 큐에 있는 건 빼서 반납해야 함
                    foreach (var packet in _sendList)
                    {
                        packet.Release();
                    }
                    _pendingList.Clear();
                    _sendList.Clear();
                    e.BufferList = null;


                }
                return;
            }

            lock (this.cs_sending_queue)
            {

                if(_sendList.Count > 1)
                {
                    Console.WriteLine("모아쏘기");
                }
                foreach (var packet in _sendList)
                {
                    packet.Release();
                }
                _pendingList.Clear();
                _sendList.Clear();
                e.BufferList = null;




                // 다음 패킷 전송
                if (this.sending_queue.Count > 0)
                {
                    start_send();
                }
            }
        }

        //void send_directly(CPacket msg)
        //{
        //	msg.record_size();
        //	this.send_event_args.SetBuffer(this.send_event_args.Offset, msg.position);
        //	Array.Copy(msg.buffer, 0, this.send_event_args.Buffer, this.send_event_args.Offset, msg.position);
        //	bool pending = this.socket.SendAsync(this.send_event_args);
        //	if (!pending)
        //	{
        //		process_send(this.send_event_args);
        //	}
        //}

        public void disconnect()
        {
            //close the socket associated with th client
            try
            {
                peer.disconnect();
                this.socket.Shutdown(SocketShutdown.Send);
            }
            //throws if client process has already closed
            catch (Exception) { }
            this.socket.Close();

        }

        public void start_keepalive()
        {
            System.Threading.Timer Keepalive = new System.Threading.Timer((object e) =>
            {
                CPacket msg = CPacket.create(0);
                msg.push(0);
                send(msg);
            }, null, 0, 3000);
        }
    }

}