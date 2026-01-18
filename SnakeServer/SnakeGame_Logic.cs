using Microsoft.VisualBasic;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SnakeServer
{
    //인게임 로직 처리
    public class SnakeGame_Logic
    {
        private readonly ThreadLocal<List<GameUser_Session>> _threadLocalTargets
    = new ThreadLocal<List<GameUser_Session>>(() => new List<GameUser_Session>(200));
        //여기는 인게임 유저를 다루는 딕셔너리 로비의 딕셔너리 키와 같다. 로그인 후 제공받은 고유 키가 여기서도 동일하게 적용된다
        Dictionary<int, GameUser_Session> game_users;
        public int Room_ID;
        static int Room_ID_Generator = 0;
        object _lock;
        public SnakeGame_Logic()
        {
            game_users = new Dictionary<int, GameUser_Session>();
            Room_ID = System.Threading.Interlocked.Increment(ref Room_ID_Generator);
            Console.WriteLine($"게임 룸 생성{Room_ID}, {Room_ID_Generator}");
            _lock = new object();
        }
        public void Enter_Game(User_Session user, int userid)
        {
            GameUser_Session gameuser = new GameUser_Session(user.CUserToken, userid);
            gameuser.GameUser_Session_Info.userPositionX = 0.0f;
            gameuser.GameUser_Session_Info.userPositionY = 0.0f;
            Console.WriteLine($"@@@@@@@@@@@@@@@@@엔터 게임 로직 부분 실행 접속한 유저 아이디{userid}");
            lock (game_users)
            {
                if (!game_users.ContainsKey(user.Uid))
                {
                    game_users.Add(gameuser.GameUser_Session_Info.userID, gameuser);
                    gameuser.Enter_Room(this);

                }
            }
            S_EnterGame_Snake packet = new S_EnterGame_Snake();
            {
                packet.userid = gameuser.GameUser_Session_Info.userID;
                packet.posX = gameuser.GameUser_Session_Info.userPositionX;
                packet.posY = gameuser.GameUser_Session_Info.userPositionY;
                Broadcast(packet.Write());

            }

            S_EnterGame_Snake s_EnterGame_Snake = new S_EnterGame_Snake();
            foreach (var item in game_users.Values)
            {
                if (item.GameUser_Session_Info.userID == gameuser.GameUser_Session_Info.userID)
                    continue;
                s_EnterGame_Snake.posX = item.GameUser_Session_Info.userPositionX;
                s_EnterGame_Snake.posY = item.GameUser_Session_Info.userPositionY;
                s_EnterGame_Snake.userid = item.GameUser_Session_Info.userID;
                gameuser.UserToken.send(s_EnterGame_Snake.Write());
            }
        }
        public void HandleMove(GameUser_Session session, C_Move packet)
        {
            Console.WriteLine($"{session.GameUser_Session_Info.userID}");
            switch (packet.direction)
            {
                case 0:
                    game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionY += 1.0f;

                    break;
                case 1:
                    game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionY -= 1.0f;
                    break;
                case 2:
                    game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionX -= 1.0f;
                    break;
                case 3:
                    game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionX += 1.0f;
                    break;
            }
            S_Move move = new S_Move();
            move.playerId = session.GameUser_Session_Info.userID;
            move.posX = game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionX;
            move.posY = game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionY;
            Broadcast(move.Write());
        }
        public void Broadcast(CPacket msg)
        {
            // 원본 데이터 정보 미리 확보
            byte[] sourceBuffer = msg.buffer;
            int copySize = msg.position;

            // [2] 현재 스레드의 전용 리스트 확보 & 초기화 (할당 없음)
            List<GameUser_Session> targets = _threadLocalTargets.Value;
            targets.Clear();

            // =========================================================
            // Phase 1. 명단 확보 (여기만 Lock! 순식간에 끝남)
            // =========================================================
            lock (_lock)
            {
                foreach (var item in game_users)
                {
                    GameUser_Session session = item.Value;
                    // 소켓이 살아있는 유저만 명단에 추가 (참조 복사라 매우 빠름)
                    if (session.UserToken.socket != null && session.UserToken.socket.Connected)
                    {
                        targets.Add(session);
                    }
                }
            }
            // Lock 해제! 이제 다른 스레드가 입장하든 퇴장하든 상관없음

            // =========================================================
            // Phase 2. 복사 및 전송 (Lock 없이 자유롭게 수행)
            // =========================================================
            // Foreach 대신 For문 사용 (GC 완전 방지)
            for (int i = 0; i < targets.Count; i++)
            {
                GameUser_Session session = targets[i];
                try
                {
                    

                    // -----------------------------------------------------------
                    // [수동 Deep Copy 로직]
                    // -----------------------------------------------------------
                    // 1. 풀에서 빈 패킷 꺼내기
                    CPacket newPacket = CPacketBufferManger.pop();

                    // 2. 주인 설정
                    newPacket.SetOwner(session);

                    // 3. [핵심] 데이터 복사 (원본 -> 새 패킷)
                    // Lock 밖에서 수행하므로 서버 전체 멈춤 현상 없음
                    Buffer.BlockCopy(sourceBuffer, 0, newPacket.buffer, 0, copySize);

                    // 4. 커서 동기화
                    newPacket.move_position(copySize);

                    // 5. 전송
                    session.UserToken.send(newPacket);
                }
                catch (Exception)
                {
                    // 로그 제외 (필요시 에러 카운트만 증가)
                }
            }

        }
    }
}
