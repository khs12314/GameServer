


using NetWorkModule;

namespace SnakeServer
{
    
    internal class Program
    {
        static Dictionary<int, User_Session> userlist = new Dictionary<int, User_Session>();
        public static GameServer game_server = new GameServer();
        static void Main(string[] args)
        {
            CPacketBufferManger.initialize(100000);//임시
            userlist = new Dictionary<int, User_Session>();
            CNetworkService service = new CNetworkService();
            service.session_created_callback += on_session_created;

            service.initialize();
            service.listen("127.0.0.1", 8000, 5000);
                Console.WriteLine("Listening....");
            while (true)
            {

            }

        }
        static void on_session_created(CUserToken token)
        {
            User_Session user = new User_Session(token);
            lock (userlist)
            {
                userlist.Add(user.Uid, user);
            }
        }
        //public static void remove_user(User_Session user)
        //{
        //    Internal_Disconnect packet = new Internal_Disconnect();
        //    game_server.enqueue_packet(packet.Write(), user);
        //}
    }
}
