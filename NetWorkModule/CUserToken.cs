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

        //session객체, 어플리케이션 단에서 구현하여 사용
        IPeer peer;

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
            this.message_resolver.on_receive(buffer, offset, transfered, on_message);
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
            this.sending_queue.Clear();

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
            CPacket clone = new CPacket();
            msg.copy_to(clone);
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
                CPacket msg = this.sending_queue.Peek();

                //헤더에 패킷 사이즈를 기록
                msg.record_size();

                //이번에 보낼 패킷 사이즈만큼 버퍼 크기를 설정하고
                this.send_event_args.SetBuffer(this.send_event_args.Offset, msg.position);

                //패킷 내용을 SocketAsyncEventArgs 버퍼에 복사한다.
                Array.Copy(msg.buffer, 0, this.send_event_args.Buffer, this.send_event_args.Offset, msg.position);

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
            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
            {
                Console.WriteLine(string.Format("send error {0}, transferred {1}", e.SocketError, e.BytesTransferred));
                return;
            }

            lock (this.cs_sending_queue)
            {
                //count가 0이하일 경우는 없겠지만

                if (this.sending_queue.Count <= 0)
                {
                    throw new Exception("Sending queue count is less than zero!");//방어적 코드, 있을 수 없는 상황, send 시 큐 할당은 필수이기 때문
                }

                //todo: 재전송 로직 다시 검토
                //패킷 하나를 다 못보낸 경우는??
                int size = this.sending_queue.Peek().position;
                if (e.BytesTransferred != size)
                {
                    string error = string.Format("Need to send more! transferred {0}, need to send {1}", e.BytesTransferred, size);
                    Console.WriteLine(error);
                    return;
                }

                //System.Threading.Interlocked.Increment(ref sent_count);   
                lock (cs_count)
                {
                    ++sent_count;//전위 연산하는 이유 분석
                    //if(sent_count % 20000 == 0)  //2만으로 나눈 나머지인 이유 분석
                    {
                        Console.WriteLine(string.Format("process send : {0}, transferred{1}, sent count{2}",
                            e.SocketError, e.BytesTransferred, sent_count));
                    }
                }
                //Console.WriteLine(string.Format("process send : {0}, transferred {1}, sent count {2}",
                //	e.SocketError, e.BytesTransferred, sent_count));

                // 전송 완료된 패킷을 큐에서 제거한다.
                //CPacket packet = this.sending_queue.Dequeue();
                //CPacket.destroy(packet);
                this.sending_queue.Dequeue();

                //아직 전송하지 않은 대기중인 패킷이 있다면 다시한번 전송을 요청한다.
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
