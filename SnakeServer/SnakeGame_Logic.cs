using Microsoft.VisualBasic;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace SnakeServer
{
    //인게임 로직 처리
    public class SnakeGame_Logic : IJobqueue
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
        JobQueue jobqueue;
        List<int> deadUser;
        public SnakeGame_Logic()
        {
            jobqueue = new JobQueue();
            game_users = new Dictionary<int, GameUser_Session>();
            Room_ID = System.Threading.Interlocked.Increment(ref Room_ID_Generator);
            Console.WriteLine($"게임 룸 생성{Room_ID}, {Room_ID_Generator}");
            _lock = new object();
            deadUser = new List<int>();
            map = new int[MapSizeY + 1, MapSizeX + 1];
            Is_Running = false;
            UserCount = 0;
        }
        public void Push(Action job)
        {
            jobqueue.Push(job);
        }
        public void Start()
        {
            GenerateApple();
            ScheduleUpdate();
        }
        void ScheduleUpdate()
        {
            Task.Delay(200).ContinueWith(t =>
            {
                Push(Update);
            });
        }
        void Update()
        {
            foreach (var p in game_users.Values)
            {
                if (!p.Is_Alive) continue;
                ProcessUserTurn(p);
            }

            foreach (var p in game_users.Keys)
            {
                if (!game_users[p].Is_Alive)
                {
                    Console.WriteLine($"죽은 유저 아이디 : {p}");
                    deadUser.Add(p);
                }
            }
            if (deadUser.Count > 0)
            {
                foreach (var p in deadUser)
                {
                    Console.WriteLine($"삭제 유저 아이디 : {p}");
                    game_users.Remove(p);
                }
            }

            ScheduleUpdate();

        }
        void ProcessUserTurn(GameUser_Session session)
        {
            // 1. [계산] 다음 좌표 구하기
            var nextPos = GetNextPosition(session);
            Console.WriteLine("계속 진행되는지 확인");
            // 2. [판정] 갈 수 있는 곳인가? (벽, 충돌)
            if (CanGo(nextPos, session) == false)
            {
                ProcessDeath(session);
                return;
            }

            // 3. [판정] 사과가 있는가?
            bool isEat = TryEatApple(nextPos);

            // 4. [행동] 실제 몸통 이동 (여기서 Insert/Remove 수행)
            MoveBody(session, nextPos, isEat);

            // 5. [전송] 브로드캐스팅
            BroadcastMove(session, nextPos, isEat);
        }

        private void ProcessDeath(GameUser_Session session)
        {

            if (!session.Is_Alive)
            {
                return;
            }
            session.Is_Alive = false;
            S_Move deathPacket = new S_Move();
            deathPacket.playerId = session.GameUser_Session_Info.userID;
            deathPacket.posX = -1000;
            deathPacket.posY = -1000;
            deathPacket.IsEat = false;
            Broadcast(deathPacket.Write());

        }

        // --- 아래는 각 기능을 담당하는 '일꾼(Worker)' 메서드들 ---

        // 기능 1: 좌표 계산만 함 (상태 변경 X)
        GameUser_Session.Pos GetNextPosition(GameUser_Session session)
        {
            var info = session.GameUser_Session_Info;
            int x = (int)info.userPositionX;
            int y = (int)info.userPositionY;

            switch (info.dir)
            {
                case 0: y += 1; break;
                case 1: y -= 1; break;
                case 2: x -= 1; break;
                case 3: x += 1; break;
            }
            return new GameUser_Session.Pos { x = x, y = y };
        }

        // 기능 2: 충돌 검사만 함 (상태 변경 X)
        bool CanGo(GameUser_Session.Pos pos, GameUser_Session session)
        {
            // 맵 범위 체크
            if (pos.x < 0 || pos.x >= MapSizeX || pos.y < 0 || pos.y >= MapSizeY) return false;

            // 몸통 충돌 체크 로직...
            foreach (var bodyPart in session.BodyList)
            {
                if (bodyPart.x == pos.x && bodyPart.y == pos.y)
                    return false; // 내 몸에 박음
            }
            foreach (var otherUser in game_users.Values)
            {
                if (otherUser.GameUser_Session_Info.userID == session.GameUser_Session_Info.userID)
                    continue; // 나는 위에서 검사했으니 패스

                foreach (var bodyPart in otherUser.BodyList)
                {
                    if (bodyPart.x == pos.x && bodyPart.y == pos.y)
                        return false; // 남의 몸 밟음 (사망)
                }
            }
            return true;
        }

        // 기능 3: 사과 먹었는지 확인 및 사과 재생성 (사과 관련 상태만 변경)
        bool TryEatApple(GameUser_Session.Pos pos)
        {
            if (pos.x == appleX && pos.y == appleY)
            {
                GenerateApple();
                return true;
            }
            return false;
        }

        // 기능 4: 뱀의 몸통 리스트 관리 (이동 관련 상태만 변경)
        void MoveBody(GameUser_Session session, GameUser_Session.Pos nextPos, bool isEat)
        {
            // 머리 추가
            session.BodyList.Insert(0, nextPos);

            // 사과 안 먹었으면 꼬리 자르기
            if (isEat == false)
            {
                if (session.BodyList.Count > 0)
                    session.BodyList.RemoveAt(session.BodyList.Count - 1);
            }

            // 좌표 정보 갱신
            session.GameUser_Session_Info.userPositionX = nextPos.x;
            session.GameUser_Session_Info.userPositionY = nextPos.y;
        }

        // 기능 5: 패킷 전송만 담당
        void BroadcastMove(GameUser_Session session, GameUser_Session.Pos pos, bool isEat)
        {
            S_Move move = new S_Move();
            move.playerId = session.GameUser_Session_Info.userID;
            move.posX = pos.x;
            move.posY = pos.y;
            move.IsEat = isEat;
            Broadcast(move.Write());
        }
        //여기 아래는 주기적으로 행동하는게 아닌 세팅 혹은 받은 값 기반으로 바로 처리해야하는 부분
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
            if (UserCount == 1)
            {
                Is_Running = true;
            }
            if (Is_Running)
            {
                Task.Run(() => Start());
            }
        }
        public void ChangeDir(GameUser_Session session, C_Move packet)
        {
            lock (_lock)
            {
                session.GameUser_Session_Info.dir = packet.direction;
            }
        }
        public void LeavGame(GameUser_Session session, int playerID)
        {
            lock (this)
            {
                if (game_users.ContainsKey(playerID) == false)
                {
                    return;
                }
                S_PlayerLeave leave = new S_PlayerLeave();
                leave.userid = session.GameUser_Session_Info.userID;
                Broadcast(leave.Write());
            }

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
