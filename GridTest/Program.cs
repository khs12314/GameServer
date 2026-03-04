


using NetWorkModule;

namespace SnakeServer
{

    internal class Program
    {
        public static Dictionary<int, IPeer> userlist = new Dictionary<int, IPeer>();
        public static GameServer game_server = new GameServer();
        static void Main(string[] args)
        {
            CPacketBufferManger.initialize(1000000);//임시
            ThreadPool.SetMinThreads(100, 100);
            userlist = new Dictionary<int, IPeer>();
            CNetworkService service = new CNetworkService();
            service.session_created_callback += on_session_created;

            service.initialize();
            service.listen("127.0.0.1", 8000, 5000);
            Console.WriteLine("Listening....");
            Thread timerThread = new Thread(GlobalTimerLoop);
            timerThread.IsBackground = true;
            timerThread.Name = "GlobalTimerThread";
            timerThread.Start();
            while (true)
            {
                Thread.Sleep(10);
            }

        }
        static void GlobalTimerLoop()
        {
            while (true)
            {
                // 1. 관리 중인 100개의 방을 가져온다.
                // (주의: Lock Contention을 피하기 위해 RoomManager 내부에서 
                // 방 목록을 배열이나 리스트 복사본으로 안전하게 넘겨주도록 설계해야 한다)
                var rooms = game_server.room_manager.GetAllRooms();

                // 2. 100개의 방에 시간(Tick)을 공급한다.
                foreach (var room in rooms)
                {
                    room.Update(); // 내부에 jobTimer.Flush()가 들어있음
                }

                // 3. CPU 100% 폭주 방지 (초당 약 100번 틱 발생)
                System.Threading.Thread.Sleep(10);
            }
        }
        static void on_session_created(CUserToken token)
        {
            Console.WriteLine("connect");
            User_Session user = new User_Session(token);
            lock (userlist)
            {
                if (!userlist.ContainsKey(user.Uid))
                {
                    userlist.Add(user.Uid, user);
                }
            }
        }
        public static void remove_user(IPeer user)
        {
            lock (userlist)
            {
                userlist.Remove(user.Get_MyID());
                game_server.User_Disconnect(user);
                if (user is GameUser_Session gameUser_Session)
                {
                    gameUser_Session.room.LeavGame(gameUser_Session, gameUser_Session.GameUser_Session_Info.userID);
                }

            }
        }
    }
}
