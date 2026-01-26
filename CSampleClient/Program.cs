using NetWorkModule;
using System.Net;

namespace CSampleClient
{
    using GameServer;
    using System.Diagnostics;
    using System.Net.WebSockets;

    internal class Program
    {
        static List<IPeer> game_servers = new List<IPeer>();
        static List<CUserToken> _dummies = new List<CUserToken>();
        public static int count = 0;
        // 목표 접속 수
        const int DUMMY_COUNT = 100;

        static void Main(string[] args)
        {
            // 1. 패킷 풀 초기화
            // 500명이 동시에 쏘려면 2000개로는 부족할 수 있습니다. 넉넉하게 잡습니다.
            CPacketBufferManger.initialize(100000);

            // 2. 네트워크 서비스 생성 (하나의 서비스로 모든 연결 처리)
            CNetworkService service = new CNetworkService();
            // (필요하다면 service.session_created_callback 설정)
            service.initialize();

            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);

            Console.WriteLine($"[Client] Spawning {DUMMY_COUNT} Zombies...");

            // 3. 500개 접속 시도 (Connection Loop)
            for (int i = 0; i < DUMMY_COUNT; i++)
            {
                CConnector connector = new CConnector(service);

                // 콜백 등록: 접속 성공 시 리스트에 추가
                connector.connected_callback += on_connected_gameserver;

                connector.connect(endpoint);

                // 너무 한 번에 접속하면 OS가 SYN Flood로 오인하거나 포트가 고갈됨.
                // 10ms 정도 텀을 줍니다.
                Thread.Sleep(10);


            }
            Console.ReadLine();
            foreach(var session in game_servers)
            {
                C_Login_DB loginPacket = new C_Login_DB();
                loginPacket.ID= $"Dummy_{session.Get_MyID()}"; // 약속된 형식
                loginPacket.PassWord = "1234";
                Console.WriteLine($"로그인{session.Get_MyID()}");
                session.send(loginPacket.Write());
            }
         
            while (true)
            {
                Thread.Sleep(10);
            }
        }

        // 접속 성공 시 호출되는 콜백
        static void on_connected_gameserver(CUserToken token)
        {
            Console.WriteLine("dasasdasd");
            lock (_dummies)
            {
                IPeer server = new CRemoteServerPeer(token, ++count);
                game_servers.Add(server);
            }
        }
        private static void TestClientMethod()
        {
            //CPacketBufferManger.initialize(2000);   //1
            ////CNetworkService객체는 메시지의 비동기 송,수신 처리를 수행한다.
            ////메시지 송, 수신은 서버, 클라이언트 모두 동일한 로직으로 처리될 수 있으므로
            ////CNetworkService 객체를 생성해 connector 겍체에 넘겨준다.
            //CNetworkService service = new CNetworkService();
            ////endpoint 정보를 갖고 있는 Connector 생성. 만들어둔 NetworkService 객체를 넣어준다.
            //CConnector connector = new CConnector(service);//2
            //// 접속 성공 시 호출될 콜백 메소드 지정.
            //connector.connected_callback += on_connected_gameserver;
            //IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            //connector.connect(endpoint);
            //Console.ReadLine();
            //CPacket msg = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_REQ);
            //game_servers[0].send(msg);
            //Thread.Sleep(1000);
            //msg = CPacket.create((short)PROTOCOL.LOADING_COMPLETED);
            //game_servers[0].send(msg);
            while (true)
            {

                //Console.WriteLine("> ");
                //string line = Console.ReadLine();//31
                //if (line == "q")
                //{
                //    break;
                //}

                //CPacket msg = CPacket.create((short)PROTOCOL.CHAT_MSG_REQ);
                //msg.push(line);
                //Console.WriteLine($"{game_servers.Count}");
                //game_servers[0].send(msg);

            }
                    ((CRemoteServerPeer)game_servers[0]).token.disconnect();
            Console.ReadKey();
        }

        /// <summary>
        /// 접속 성공시 호출될 콜백 메소드
        /// </summary>
        /// <param name="="server_token"></param>
        //static void on_connected_gameserver(CUserToken server_toekn)
        //{
        //    IPeer server = new CRemoteServerPeer(server_toekn); //1
        //    game_servers.Add(server);
        //    Console.WriteLine("Connected!");

        //}



    }
}
