


using NetWorkModule;
using System.Net;

namespace SnakeServer
{

    internal class Program
    {
        static Listener _listener = new Listener();
        //instance로 세션 매니저 접근, 원본 값 자체를 sessionmanager에서 관리하는걸로 변경 및 로그인 후 로비에 유저 전달
        //public static Dictionary<int, PacketSession> userlist = new Dictionary<int, PacketSession>();
        public static GameServer game_server = new GameServer();
        static void Main(string[] args)
        {
            CPacketBufferManger.initialize(10000);//임시
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            
            Console.WriteLine("Listening...");

            //FlushRoom();
            //JobTimer.Instance.Push(FlushRoom);

            // TODO
            while (true)
            {
                //JobTimer.Instance.Flush();
                Thread.Sleep(100);
            }

        }
        //static void on_session_created(CUserToken token)
        //{
        //    Console.WriteLine("connect");
        //    User_Session user = new User_Session();
        //    lock (userlist)
        //    {
        //        if (!userlist.ContainsKey(user.Uid))
        //        {
        //            userlist.Add(user.Uid, user);
        //        }
        //    }
        //}
        //public static void remove_user(PacketSession user)
        //{
        //    lock (userlist)
        //    {
        //        userlist.Remove(user.Get_MyID());
        //        game_server.User_Disconnect(user);
        //        if (user is GameUser_Session gameUser_Session)
        //        {
        //            gameUser_Session.room.LeavGame(gameUser_Session, gameUser_Session.GameUser_Session_Info.userID);
        //        }

        //    }
        //}
    }
}
