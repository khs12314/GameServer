


using NetWorkModule;

namespace SnakeServer
{

    internal class Program
    {
        public static Dictionary<int, IPeer> userlist = new Dictionary<int, IPeer>();
        public static GameServer game_server = new GameServer();
        static void Main(string[] args)
        {
            CPacketBufferManger.initialize(10000);//임시
            userlist = new Dictionary<int, IPeer>();
            CNetworkService service = new CNetworkService();
            service.session_created_callback += on_session_created;

            service.initialize();
            service.listen("0.0.0.0", 8000, 5000);
            Console.WriteLine("Listening....");
            while (true)
            {

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
