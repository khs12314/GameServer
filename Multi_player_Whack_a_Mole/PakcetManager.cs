using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_player_Whack_a_Mole
{
    class PacketManager
    {
        #region Singleton
        static PacketManager _instance = new PacketManager();
        public static PacketManager Instance { get { return _instance; } }
        #endregion

        public PacketManager()
        {
            // 나중에는 여기서 Register() 등으로 자동화할 수 있지만, 
            // 지금은 직관적인 switch문으로 갑니다.
        }

        // [핵심] 네트워크 모듈에서 호출하는 함수
        // session: 패킷을 보낸 유저 (GameUser)
        // packet: 도착한 데이터 뭉치
        public void OnRecvPacket(IPeer session, CPacket packet)
        {
            // 1. 프로토콜 ID 추출 (short)
            
            short protocolId = packet.protocol_id;
            Console.WriteLine("패킷 매니저 실행");
            Console.WriteLine($"{protocolId}");
            // 2. ID에 따라 알맞은 패킷 생성 및 핸들러 호출
            switch ((PacketID)protocolId)
            {
                case PacketID.PlayerInfoReq:
                    {
                       
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(packet);
                        // PacketHandler.PlayerInfoReqHandler(session, p);
                    }
                    break;
                // --- [기존 패킷] ---
                case PacketID.C_Login:
                    {
                        Console.WriteLine("로그인");
                        C_Login p = new C_Login();
                        p.Read(packet);
                        PacketHandler.C_LoginHandler(session, p);
                    }
                    break;

                // --- [신규 추가: 두더지 게임] ---
                case PacketID.C_EnterGame:
                    {
                        Console.WriteLine("엔터게임");
                        C_EnterGame p = new C_EnterGame();
                        p.Read(packet);
                        PacketHandler.C_EnterGameHandler(session, p);
                    }
                    break;

                case PacketID.C_LoadingComplete:
                    {
                        Console.WriteLine("로딩 컴플릿");
                        C_LoadingComplete p = new C_LoadingComplete();
                        p.Read(packet);
                        PacketHandler.C_LoadingCompleteHandler(session, p);
                    }
                    break;

                case PacketID.C_HitMole:
                    {
                        Console.WriteLine("힛몰");
                        C_HitMole p = new C_HitMole();
                        p.Read(packet);
                        PacketHandler.C_HitMoleHandler(session, p);
                    }
                    break;


                   
                // 정의되지 않은 패킷
                default:
                    {
                        Console.WriteLine($"Unknown Protocol ID: {protocolId}");
                    }
                    break;
            }
        }
    }
}
