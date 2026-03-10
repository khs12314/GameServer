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
        JobQueue jobqueue;
        AutoResetEvent loop;
        public RoomManager room_manager { get; private set; }
        //여기는 로비에서 다뤄지는 매칭 대기 유저들 로그인하면 db에 저장된 값 기반으로 키를 설정해줌
        Dictionary<int, IPeer> Loby_User;

        public GameServer()
        {
            this._lock = new object();
            this.loop = new AutoResetEvent(false);
            this.JobQueue = new Queue<CPacket>();
            this.room_manager = new RoomManager(100);
            this.Loby_User = new Dictionary<int, IPeer>();
            this.locgic_thread = new Thread(GameLoop);
            this.locgic_thread.Start();
            jobqueue = new JobQueue();
        }
        public void Push(Action job)
        {
            jobqueue.Push(job);
        }
        void GameLoop()
        {
            Console.WriteLine("게임 루프 시작");
            while (true)
            {
                //Console.WriteLine("게임 루프 도는 중");
                CPacket? packet = null;
                lock (_lock)
                {
                    if (JobQueue.Count > 0)
                    {
                        packet = JobQueue.Dequeue();
                        if (packet.GetRefCount() < 0)
                        {
                            Console.WriteLine("레프카운트");
                        }
                    }
                }
                if (packet != null)
                {
                    //Console.WriteLine("잡 수행을 위한 메소드 실행");
                    try
                    {
                        Process_Receive(packet);
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine($"게임서버 루프에서 예외 발생: {ex}");
                    }
                    finally
                    {

                    }
                }
                else
                {
                    loop.WaitOne();
                }



            }
        }
        public bool Enter_Game_Request(User_Session user, int userid, int Room_id)
        {
            //Console.WriteLine("엔터 리퀘스트 실행");
            return room_manager.Enter_Room(user, userid, Room_id);
        }
        void Matching_Request(User_Session user)
        {

        }
        public void Enqueue_Job(CPacket packet, CUserToken user_Session)
        {
            lock (_lock)
            {

                JobQueue.Enqueue(packet);
                if (packet.GetRefCount() < 0)
                {
                    Console.WriteLine("레프카운트");
                }
                loop.Set();
            }
        }
        void Process_Receive(CPacket packet)
        {
            if (packet.owner == null) return;
            //Console.WriteLine("패킷 들어옴");
            if (packet.GetRefCount() < 0)
            {
                Console.WriteLine("레프카운트");
            }
            short protocol_id = BitConverter.ToInt16(packet.buffer, 2);

            if (protocol_id == (short)SystemPacketID.InternalDisconnect)
            {
                packet.pop_protocol_id();
                IPeer session = packet.owner;
                this.User_Disconnect(session);
            }
            else
            {
                packet.owner.process_user_operation(packet);
            }
        }
        public void User_Disconnect(IPeer session)
        {
            lock (_lock)
            {
                Loby_User.Remove(session.Get_MyID());

            }
        }
        public void Set_User_Session(User_Session user, int Userid)
        {

            if (!Loby_User.ContainsKey(Userid))
            {
                Console.WriteLine($"로비에 등록 완료 및 로비 등록 완료 유저 아이디는 {Userid}");
                Loby_User.Add(Userid, user);
            }

        }



    }
}
