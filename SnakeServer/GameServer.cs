using Microsoft.VisualBasic;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    //게임 서버 로직
    public class GameServer
    {
        object _lock = new object();
        Queue<CPacket> JobQueue;
        //로직 스레드
        Thread locgic_thread;
        AutoResetEvent loop;
        public RoomManager room_manager { get; private set; }
        //여기는 로비에서 다뤄지는 매칭 대기 유저들 로그인하면 db에 저장된 값 기반으로 키를 설정해줌
        Dictionary<int ,User_Session> matching_waiting_users;
        
        public GameServer()
        {
            this._lock = new object();
            this.loop = new AutoResetEvent(false);
            this.JobQueue = new Queue<CPacket>();
            this.room_manager = new RoomManager();
            this.matching_waiting_users = new Dictionary<int, User_Session>();
            this.locgic_thread = new Thread(GameLoop);
            this.locgic_thread.Start();
        }
        void GameLoop()
        {
            Console.WriteLine("게임 루프 시작");
            while (true) {
                Console.WriteLine("게임 루프 도는 중");
                CPacket packet = null;
                lock (_lock)
                {
                    if (JobQueue.Count > 0)
                    {
                        packet = JobQueue.Dequeue();
                    }
                }
                if (packet != null)
                {
                    Console.WriteLine("잡 수행을 위한 메소드 실행");
                    try
                    {
                        Process_Receive(packet);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"게임서버 루프에서 예외 발생: {ex.Message}");
                    }
                    finally
                    {
                        CPacket.destroy(packet);
                    }
                }
                else
                {
                    loop.WaitOne();
                }



            }
        }
        void Matching_Request(User_Session user)
        {

        }
        public void Enqueue_Job(CPacket packet)
        {
            lock (_lock)
            {
                Console.WriteLine("잡 들어옴");
                JobQueue.Enqueue(packet);
                loop.Set();
            }
        }
        void Process_Receive(CPacket packet)
        {
            if (packet.owner == null) return;
            Console.WriteLine("패킷 들어옴");

            short protocol_id = BitConverter.ToInt16(packet.buffer, packet.position);
            if(protocol_id == (short)SystemPacketID.InternalDisconnect)
            {
                packet.pop_protocol_id();
                GameUser_Session user = (GameUser_Session)packet.owner;
                this.User_Disconnect();
            }
            else
            {
                packet.owner.process_user_operation(packet);
            }
        }
        void User_Disconnect()
        {

        }
        public void Set_User_Session(User_Session user, int Userid)
        {
            lock(_lock)
            {
                if(!matching_waiting_users.ContainsKey(Userid))
                {
                    matching_waiting_users.Add(Userid, user);
                }
            }
        }



    }
}
