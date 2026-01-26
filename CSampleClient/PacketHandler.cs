using CSampleClient;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Multi_player_Whack_a_Mole
{
    public class PacketHandler
    {
        static Random _rand = new Random();
        
        // -----------------------------------------------------------
        // 1. 로그인 성공 (S_LoginOk)
        // -----------------------------------------------------------
        public static void S_LoginOkHandler(IPeer peer, S_LoginOk packet)
        {
            CRemoteServerPeer session = peer as CRemoteServerPeer;
            if (session == null) return;

            // 내 ID 저장 (추후 타격 패킷 보낼 때 내 ID가 필요함)
            // session에 SetMyId 메소드나 public 필드가 있어야 함
            session.SetMyId(packet.ServerUserID);

            Console.WriteLine($"[Client] Login Success! Assigned ID: {packet.ServerUserID}");
            
            // [Action] 로그인 성공했으니 바로 게임 입장 요청 (C_EnterGame)
            C_EnterGame enterPacket = new C_EnterGame();
            enterPacket.userId = packet.ServerUserID;
            Console.WriteLine($"엔터 패킷 유저 아이디{enterPacket.userId}");
            session.token.send(enterPacket.Write());
        }
        public static void S_Move_Handler(IPeer peer, S_Move packet)
        {
            CRemoteServerPeer session = peer as CRemoteServerPeer;
            if (session == null) return;

            // [중요] "내가 움직인 결과"가 맞는지 확인!
            // 남이 움직인 패킷(S_Move)을 받고 내가 또 움직이면
            // 1명이 움직일 때 50명이 동시에 반응하는 '핵폭탄'이 터짐.
            if (packet.playerId != session.MyServerID)
                return;
            

            // -----------------------------------------------------------
            // [무한 동력 로직] 0 -> 1 -> 2 -> 3 -> 0 ...
            // -----------------------------------------------------------

            // 1. 다음 방향 계산 (모듈러 연산 활용)
            session.CurrentDirection = (session.CurrentDirection + 1) % 4;

            // 2. 패킷 생성
            //Thread.Sleep(200);
            C_Move movePacket = new C_Move();
            movePacket.direction = session.CurrentDirection;
            
            // 3. 약간의 딜레이 (선택 사항)
            // 너무 빠르면(0초 딜레이) 서버가 '핵'으로 간주하거나 처리가 밀릴 수 있음.
            // 테스트 목적이면 0.1초~0.5초 정도 쉬고 보내는 게 현실적임.

            // 4. 전송
            session.send(movePacket.Write());
        }
        public static void S_Login_DBHandler(IPeer session, IPacket packet)
        {
            S_Login_DB pkt = packet as S_Login_DB;
            CRemoteServerPeer dummy = session as CRemoteServerPeer;

            if (pkt.Sucess)
            {
                // 중요: 서버가 발급해준 ID를 저장한다.
                // (서버 로직상 EnterGame 할 때 이 ID를 다시 보내줘야 하니까)
                // dummy.SetServerID(pkt.UserID); <- 이런 식으로 세터 만들어도 됨
                Console.WriteLine($"[Dummy_{dummy.DummyIndex}] Login OK (UID: {pkt.UserID})");
                dummy.MyServerID = pkt.UserID;
                // [Step 3] 로그인 성공했으니 바로 100번 방 입장 요청
                C_EnterGame_Snake enterGame = new C_EnterGame_Snake();
                enterGame.userId = pkt.UserID; // 받은 ID 그대로 토스
                enterGame.Room_id = 100;       // 목표: 100번 방

                dummy.send(enterGame.Write());
            }
            else
            {
                Console.WriteLine($"[Dummy_{dummy.DummyIndex}] Login Failed");
            }
        }
        // -----------------------------------------------------------
        // 2. 두더지 스폰 (S_Spawn_Mole) -> 자동 타격 로직
        // -----------------------------------------------------------
        public static void S_Spawn_MoleHandler(IPeer peer, S_Spawn_Mole packet)
        {
            CRemoteServerPeer session = peer as CRemoteServerPeer;
            if (session == null) return;
            Console.WriteLine($"받은 스폰 인덱스{packet.moleIndex}");
            int moleIndex = packet.moleIndex;
            int myId = session.GetMyId(); // 저장해둔 내 ID 가져오기
            // 비동기로 반응 속도 시뮬레이션 후 타격 패킷 전송
            Task.Run(async () =>
            {
                // 50ms ~ 250ms 랜덤 지연 (사람 반응 속도 흉내)
                int delay = _rand.Next(50, 250);
                await Task.Delay(delay);

                // 타격 패킷 생성 및 전송
                C_HitMole hitPacket = new C_HitMole();
                hitPacket.moleIndex = moleIndex;
                hitPacket.UserID = myId;
                Console.WriteLine($"푸시 아이디{myId}");
                session.token.send(hitPacket.Write());
                // 로그가 너무 많으면 주석 처리
                // Console.WriteLine($"[Client] Auto-Hit Mole[{moleIndex}] (Delay: {delay}ms)");
            });
        }

        // -----------------------------------------------------------
        // 3. 점수 업데이트 (S_ScoreUpdate)
        // -----------------------------------------------------------
        public static void S_ScoreUpdateHandler(IPeer peer, S_ScoreUpdate packet)
        {
            // 단순히 점수 로그 출력 추가예정
            Console.WriteLine($"[Client] Current Score: {packet.score}");
        }

        // -----------------------------------------------------------
        // 4. 게임 시작 (S_GameStart)
        // -----------------------------------------------------------
        public static void S_GameStartHandler(IPeer peer, S_GameStart packet)
        {
            Console.WriteLine($"[Client] Game Started! Duration: {packet.duration}s");
            CRemoteServerPeer session = peer as CRemoteServerPeer;
            if (session == null) return;
            session.duration = packet.duration;
        }

        // -----------------------------------------------------------
        // 5. 로딩 시작 (S_StartLoading) -> 로딩 완료 응답 필수
        // -----------------------------------------------------------
        public static void S_StartLoadingHandler(IPeer peer, S_StartLoading packet)
        {
            CRemoteServerPeer session = peer as CRemoteServerPeer;
            if (session == null) return;

            Console.WriteLine($"[Client] Loading Start... (PlayerIndex: {packet.PlayerIndex})");

            // [중요] 서버가 로딩하라고 했으니, 바로 "로딩 끝났습니다"라고 답장을 보내야 게임이 시작됨
            // 실제 게임이라면 리소스 로드 후 보내겠지만, 더미는 바로 보냄
            C_LoadingComplete loadEndPacket = new C_LoadingComplete();
            session.token.send(loadEndPacket.Write());
        }

        // -----------------------------------------------------------
        // 6. 플레이어 정보 (S_PlayerInfo)
        // -----------------------------------------------------------
        public static void S_PlayerInfoHandler(IPeer peer, S_PlayerInfo packet)
        {
            Console.WriteLine($"[Client] Player Info Received: Index {packet.playerIndex}");
            CRemoteServerPeer session = peer as CRemoteServerPeer;
            if (session == null) return;
            session.myIndex = packet.playerIndex;
        }

        // -----------------------------------------------------------
        // 7. 방 제거 알림 (S_RoomRemoved)
        // -----------------------------------------------------------
        public static void S_RoomRemovedHandler(IPeer peer, S_RoomRemoved packet)
        {
            Console.WriteLine("[Client] Room Removed (Game Over or Disconnected).");
            // 필요 시 클라이언트 종료 혹은 재접속 로직 추가 예정
        }
    }
}

