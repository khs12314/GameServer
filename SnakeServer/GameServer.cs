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

        public RoomManager room_manager { get; private set; }
        //여기는 로비에서 다뤄지는 매칭 대기 유저들 로그인하면 db에 저장된 값 기반으로 키를 설정해줌
        Dictionary<int ,User_Session> matching_waiting_users;
        
        public GameServer()
        {
            this._lock = new object();
            this.JobQueue = new Queue<CPacket>();
            this.room_manager = new RoomManager();
            this.matching_waiting_users = new Dictionary<int, User_Session>();
            this.locgic_thread = new Thread(GameLoop);
            this.locgic_thread.Start();
        }
        void GameLoop()
        {

        }
        void Matching_Request(User_Session user)
        {

        }
        void Enqueue_Job(CPacket packet)
        {
            lock (_lock)
            {
                JobQueue.Enqueue(packet);
            }
        }
        void Process_Receive(CPacket packet)
        {

        }
        void User_Disconnect()
        {

        }




    }
}
