using NetWorkModule;


namespace CSampleServer
{
    internal class Program
    {
        static List<CGameUser> userlist;
        static void Main(string[] args)
        {
            //동시처리 패킷 클래스의 인스턴스는 최대 2000개라는 의미
            CPacketBufferManger.initialize(2000);//1
            userlist = new List<CGameUser>();

            CNetworkService service = new CNetworkService();//2
            
            

            //콜백 메소드 설정.
            service.session_created_callback += on_session_created;

            //초기화
            //3
            service.initialize();
            service.listen("0.0.0.0", 8000, 100);

            Console.WriteLine("Started!");

            while (true)
            {
                Thread.Sleep(1000); //4
            }
            Console.ReadKey();


        }
        /// <summary>
        /// 클라이언트가 접속 완료하였을 때 호출됩니다.
        /// n 개의 워커 스레드에서 호출될 수 있으므로 공ㅇ ㅠ자원 접근 시 동기화 처리 해줘야합니다.
        /// </summary>
        /// <param name="token"></param>
        static void on_session_created(CUserToken token)//1)
        {
            CGameUser user = new CGameUser(token);
            lock (userlist)
            {
                userlist.Add(user);
            }
        }
        public static void remove_user(CGameUser user)//2
        {

            lock (userlist)
            {
                userlist.Remove(user);
            }

        }
    }
}
