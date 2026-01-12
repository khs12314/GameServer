using CSampleClient;
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
        public void OnRecvPacket(IPeer peer, CPacket packet)
        {
            // 1. 프로토콜 ID 추출 (short)
            short protocolId = packet.protocol_id;
            CRemoteServerPeer session = peer as CRemoteServerPeer;
            Console.WriteLine($"{protocolId}");
            // 2. ID에 따라 알맞은 패킷 생성 및 핸들러 호출
            switch ((PacketID)protocolId)
            {
               

                case PacketID.S_LoginOk:
                    S_LoginOk loginPacket = new S_LoginOk();
                    loginPacket.Read(packet);
                    PacketHandler.S_LoginOkHandler(peer, loginPacket);
                    //로그인 확인
                    break;
                case PacketID.S_Spawn_Mole:
                    S_Spawn_Mole spawnPacket = new S_Spawn_Mole();
                    spawnPacket.Read(packet);
                    //두더지 스폰
                    PacketHandler.S_Spawn_MoleHandler(peer, spawnPacket);
                    break;
                case PacketID.S_ScoreUpdate:
                    S_ScoreUpdate scorePacket = new S_ScoreUpdate();
                    scorePacket.Read(packet);
                    //점수 업데이트
                    PacketHandler.S_ScoreUpdateHandler(peer, scorePacket);
                    break;
                case PacketID.S_GameStart:
                    S_GameStart startPacket = new S_GameStart();
                    startPacket.Read(packet);
                    //게임 시작
                    PacketHandler.S_GameStartHandler(peer, startPacket);
                    break;
                case PacketID.S_StartLoading:
                    S_StartLoading loadPacket = new S_StartLoading();
                    loadPacket.Read(packet);
                    //로딩 시작
                    PacketHandler.S_StartLoadingHandler(peer, loadPacket);
                    break;
                case PacketID.S_PlayerInfo:
                    S_PlayerInfo infoPacket = new S_PlayerInfo();
                    infoPacket.Read(packet);
                    //플레이어 정보
                    PacketHandler.S_PlayerInfoHandler(peer, infoPacket);
                    break;
                case PacketID.S_RoomRemoved:
                    S_RoomRemoved removePacket = new S_RoomRemoved();
                    removePacket.Read(packet);
                    //방 제거
                    PacketHandler.S_RoomRemovedHandler(peer, removePacket);
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
