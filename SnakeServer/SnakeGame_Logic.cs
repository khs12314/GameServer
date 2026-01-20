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
        public int[,] map;
        public int MapSizeY = 25;
        public int MapSizeX = 25;
        object _lock;
        bool Is_Running;
        int UserCount;
        int appleX = -1;
        int appleY = -1;
        Random rnd = new Random();
        public SnakeGame_Logic()
        {
            game_users = new Dictionary<int, GameUser_Session>();
            Room_ID = System.Threading.Interlocked.Increment(ref Room_ID_Generator);
            Console.WriteLine($"게임 룸 생성{Room_ID}, {Room_ID_Generator}");
            _lock = new object();
            map = new int[MapSizeY+1, MapSizeX+1];
            Is_Running = false;
            UserCount = 0;
        }
        void GameStart_Loop()
        {
            GenerateApple();
            while (Is_Running)
            {
                // A. 자물쇠 걸기 (업데이트 중에 누가 들어오거나 나가면 안 됨)
                lock (_lock)
                {
                    // B. 모든 유저 순회 (이게 님이 걱정하던 그 부분)
                    // 그냥 foreach 돌리면 끝입니다. 아주 빠릅니다.
                    foreach (var session in game_users.Values)
                    {
                        HandleMove(session);
                    }
                }

                // C. 0.2초 휴식 (이게 틱 레이트가 됨)
                Thread.Sleep(200);
            }
        }
        void GenerateApple()
        {
            while (true)
            {
                int x = rnd.Next(0, MapSizeX); // 맵 크기(50)
                int y = rnd.Next(0, MapSizeY);

                // 유저랑 겹치는지 체크
                bool overlapped = false;
                foreach (var user in game_users.Values)
                {
                    if ((int)user.GameUser_Session_Info.userPositionX == x &&
                       (int)user.GameUser_Session_Info.userPositionY == y)
                    {
                        overlapped = true;
                        break;
                    }
                }

                if (!overlapped)
                {
                    appleX = x;
                    appleY = y;

                    // "새 사과 심어라" 방송
                    S_SpawnItem packet = new S_SpawnItem();
                    packet.ItemID = 0; // 사과 ID
                    packet.posX = x;
                    packet.posY = y;
                    Broadcast(packet.Write());
                    break;
                }
            }
        }
        public void MapLoad(int y, int x)
        {
            for (int i = 0; i <= MapSizeY; i++)
            {
                map[i, 0] = 1;  // 좌
                map[i, MapSizeY] = 1; // 우
                map[0, i] = 1;  // 하
                map[MapSizeY, i] = 1; // 상
            }
        }
        public void Enter_Game(User_Session user, int userid)
        {
            MapLoad(MapSizeY, MapSizeX);
            GameUser_Session gameuser = new GameUser_Session(user.CUserToken, userid);
            gameuser.GameUser_Session_Info.userPositionX = 1.0f;
            gameuser.GameUser_Session_Info.userPositionY = 1.0f;
           
            lock (game_users)
            {
                if (!game_users.ContainsKey(user.Uid))
                {
                    game_users.Add(gameuser.GameUser_Session_Info.userID, gameuser);
                    gameuser.Enter_Room(this);

                }
            }

            S_EnterGame_Snake s_EnterGame = new S_EnterGame_Snake();
            s_EnterGame.userid = userid;
            s_EnterGame.mapSizeX = MapSizeX;
            s_EnterGame.mapSizeY = MapSizeY;
            gameuser.UserToken.send(s_EnterGame.Write());

            S_Spawn packet = new S_Spawn();
            {
                packet.userid = gameuser.GameUser_Session_Info.userID;
                packet.SpawnX = gameuser.GameUser_Session_Info.userPositionX;
                packet.SpawnY = gameuser.GameUser_Session_Info.userPositionY;
                Broadcast(packet.Write());

            }

            S_Spawn s_EnterGame_Snake = new S_Spawn();
            foreach (var item in game_users.Values)
            {
                if (item.GameUser_Session_Info.userID == gameuser.GameUser_Session_Info.userID)
                    continue;
                s_EnterGame_Snake.SpawnX = item.GameUser_Session_Info.userPositionX;
                s_EnterGame_Snake.SpawnY = item.GameUser_Session_Info.userPositionY;
                s_EnterGame_Snake.userid = item.GameUser_Session_Info.userID;
                gameuser.UserToken.send(s_EnterGame_Snake.Write());
            }
            UserCount = Interlocked.Increment(ref UserCount);
            if(UserCount == 2)
            {
                Is_Running = true;  
            }
            if (Is_Running)
            {
                Task.Run(() => GameStart_Loop());
            }
        }
       public void ChangeDir(GameUser_Session session, C_Move packet)
        {
            lock (_lock)
            {
                session.GameUser_Session_Info.dir = packet.direction;
            }
        }
        public void HandleMove(GameUser_Session session)
        {
            float nextX = game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionX;
            float nextY = game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionY;
            
            switch (session.GameUser_Session_Info.dir)
            {
                case 0: nextY += 1.0f; break;
                case 1: nextY -= 1.0f; break;
                case 2: nextX -= 1.0f; break;
                case 3: nextX += 1.0f; break;
            }

            // 2. [검사] 맵 밖인가? (예: 0~50 범위)
            // (나중에는 여기에 map[,] 배열 체크를 넣으면 됨)
            if (nextX < 0 || nextX > MapSizeX || nextY < 0 || nextY > MapSizeY || map[(int)nextY, (int)nextX] == 1)
            {
                // 벽이다! 움직이지 마. (그냥 리턴)
                return;
            }
            foreach(var b in session.BodyList)
            {
                if(b.x == (int)nextX && b.y == (int)nextY)
                {
                    Console.WriteLine("자기 자신과 충돌");
                    return;
                }
            }

            bool AteApple = false;
            if((int)nextX == appleX && (int)nextY == appleY)
            {
                Console.WriteLine("사과 섭취");
                AteApple = true;
                GenerateApple();
            }
            // 3. 통과했으면 실제 좌표에 적용
            session.BodyList.Insert(0, new GameUser_Session.Pos { x = (int)nextX, y = (int)nextY });
            if (!AteApple)
            {
                if (session.BodyList.Count > 0)
                {
                    session.BodyList.RemoveAt(session.BodyList.Count - 1);
                }
            }
            S_Move move = new S_Move();
            move.playerId = session.GameUser_Session_Info.userID;
            move.posX = nextX;
            move.posY = nextY;
            move.IsEat = AteApple;
            Broadcast(move.Write());
            session.GameUser_Session_Info.userPositionX = nextX;
            session.GameUser_Session_Info.userPositionY = nextY;
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
