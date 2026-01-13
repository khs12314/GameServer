using NetWorkModule;

namespace Multi_player_Whack_a_Mole
{
    internal class Program
    {
        static List<GameUser> userlist;
        public static GameServer game_main = new GameServer();
        static void Main(string[] args)
        {
            CPacketBufferManger.initialize(100000);
            userlist = new List<GameUser>();

            CNetworkService service = new CNetworkService();
            //콜백 메소드 설정.
            service.session_created_callback += on_session_created;
            //초기화
            service.initialize();
            service.listen("127.0.0.1", 8000, 5000);

            
            while (true)
            {
                
                
            }
        }

        /// <summary>
        /// 클라이언트가 접속 완료했을 때 호출
        /// n개의 워커 스레드에서 호출될 수 있으므로 공유 자원 접근시 동기화 처리
        /// </summary>
        /// <param name="token"></param>
        static void on_session_created(CUserToken token)
        {
            GameUser user = new GameUser(token);
            lock (userlist)
            {
                userlist.Add(user);
            }
        }
        public static void remove_user(GameUser user)
        {
            Internal_Disconnect packet = new Internal_Disconnect();
            
            game_main.enqueue_packet(packet.Write(), user);
        }
    }
}
