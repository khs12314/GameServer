using NetWorkModule;
using System.Net;

namespace CSampleClient
{
    using GameServer;
    using System.Net.WebSockets;

    internal class Program
    {
        static List<IPeer> game_servers = new List<IPeer>();
        static List<CUserToken> _dummies = new List<CUserToken>();

        // 목표 접속 수
        const int DUMMY_COUNT = 2;

        static void Main(string[] args)
        {
            // 1. 패킷 풀 초기화
            // 500명이 동시에 쏘려면 2000개로는 부족할 수 있습니다. 넉넉하게 잡습니다.
            CPacketBufferManger.initialize(10);

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

            Console.WriteLine("[Client] All Connected. Start Firing Packets...");
            Console.ReadLine();
            // 4. 패킷 난사 루프 (Firing Loop)
            while (true)
            {
                
                try
                {
                    // 리스트를 순회하며 패킷 전송
                    // 접속이나 끊김이 비동기로 일어나므로 리스트 접근 시 lock 필수
                    lock (_dummies)
                    {
                        foreach (var token in game_servers)
                        {
                            // [Zero-Alloc Packet 생성]
                            // 여기서 create를 쓰면 내부적으로 Pool에서 가져옴
                            //CPacket msg = CPacket.create((short)PROTOCOL.ENTER_GAME_ROOM_REQ);
                            C_Login c_Login = new C_Login();
                            
                            //Console.WriteLine("Send Pakcet ENTER_GAME_ROOM_REQ");
                            // 필요하다면 더미 데이터 푸시
                            // msg.push(100); 

                            token.send(c_Login.Write());
                            Console.WriteLine("패킷 전송");
                            
                        }
                    }
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }

                // 서버가 숨 쉴 틈은 줘야 함 (0.1초 대기)
                // 너무 빠르면 네트워크 대역폭 문제로 끊길 수 있음.
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
                IPeer server = new CRemoteServerPeer(token);
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
