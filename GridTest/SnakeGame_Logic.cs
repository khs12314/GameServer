using GridTest;
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
        public int MapSizeY = 100;
        public int MapSizeX = 100;
        object _lock;
        bool Is_Running;
        int UserCount;
        int appleX = -1;
        int appleY = -1;
        GridManager grid;
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
            grid = new GridManager(MapSizeX, 10);
        }
        public void Push(Action job)
        {
            jobqueue.Push(job);
        }
        public void Start()
        {

            //GenerateApple();
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
            if (Is_Running)
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
                        //Console.WriteLine($"죽은 유저 아이디 : {p}");
                        deadUser.Add(p);
                    }
                }
                if (deadUser.Count > 0)
                {
                    foreach (var p in deadUser)
                    {
                        //Console.WriteLine($"삭제 유저 아이디 : {p}");
                    }
                }
                lock (_lock)
                {
                    deadUser.Clear();
                }
                ScheduleUpdate();
            }
        }
        void ProcessUserTurn(GameUser_Session session)
        {
            // 1. [계산] 다음 좌표 구하기
            if (!session.Is_Alive) return;
            var nextPos = GetNextPosition(session);
            //Console.WriteLine("계속 진행되는지 확인");
            // 2. [판정] 갈 수 있는 곳인가? (벽, 충돌)
            if (CanGo(nextPos, session) == false)
            {
                //ProcessDeath(session);
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
            S_Die s_Die = new S_Die();
            s_Die.userid = session.GameUser_Session_Info.userID;
            Broadcast(s_Die.Write(), grid.GetNeighbors(session));
            //S_Move deathPacket = new S_Move();
            //deathPacket.playerId = session.GameUser_Session_Info.userID;
            //deathPacket.posX = -1000;
            //deathPacket.posY = -1000;
            //deathPacket.IsEat = false;
            //Broadcast(deathPacket.Write());

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
                case -1: break;
            }
            return new GameUser_Session.Pos { x = x, y = y };
        }

        // 기능 2: 충돌 검사만 함 (상태 변경 X)
        bool CanGo(GameUser_Session.Pos pos, GameUser_Session session)
        {
            // 맵 범위 체크
            if (pos.x < 0 || pos.x >= MapSizeX || pos.y < 0 || pos.y >= MapSizeY)
            {
                Interlocked.Decrement(ref UserCount);
                return false;
            }

            // 몸통 충돌 체크 로직...
            foreach (var bodyPart in session.BodyList)
            {
                if (bodyPart.x == pos.x && bodyPart.y == pos.y)
                {
                    Interlocked.Decrement(ref UserCount);
                    return false;
                }
            }
            foreach (var otherUser in game_users.Values)
            {
                if (otherUser.GameUser_Session_Info.userID == session.GameUser_Session_Info.userID)
                    continue; // 나는 위에서 검사했으니 패스

                foreach (var bodyPart in otherUser.BodyList)
                {
                    if (bodyPart.x == pos.x && bodyPart.y == pos.y)
                    {
                        Interlocked.Decrement(ref UserCount);
                        return false;
                    }
                }
            }
            return true;
        }

        // 기능 3: 사과 먹었는지 확인 및 사과 재생성 (사과 관련 상태만 변경)
        bool TryEatApple(GameUser_Session.Pos pos)
        {
            if (pos.x == appleX && pos.y == appleY)
            {
                //GenerateApple();
                return true;
            }
            return false;
        }

        // 기능 4: 뱀의 몸통 리스트 관리 (이동 관련 상태만 변경)
        void MoveBody(GameUser_Session session, GameUser_Session.Pos nextPos, bool isEat)
        {
            // 머리 추가
            session.BodyList.AddFirst(nextPos);

            // 사과 안 먹었으면 꼬리 자르기
            if (isEat == false)
            {
                if (session.BodyList.Count > 0)
                    session.BodyList.RemoveLast();
            }

            // 좌표 정보 갱신
            grid.Move(session, nextPos);
            session.GameUser_Session_Info.userPositionX = nextPos.x;
            session.GameUser_Session_Info.userPositionY = nextPos.y;
            //Console.WriteLine($"x: {session.GameUser_Session_Info.userPositionX}, y:{session.GameUser_Session_Info.userPositionY}");
        }

        // 기능 5: 패킷 전송만 담당
        void BroadcastMove(GameUser_Session session, GameUser_Session.Pos pos, bool isEat)
        {
            S_Move move = new S_Move();
            move.playerId = session.GameUser_Session_Info.userID;
            move.posX = pos.x;
            move.posY = pos.y;
            move.IsEat = isEat;
            Broadcast(move.Write(), grid.GetNeighbors(session));
        }
        //여기 아래는 주기적으로 행동하는게 아닌 세팅 혹은 받은 값 기반으로 바로 처리해야하는 부분
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
            if (user == null)
            {
                Console.WriteLine("[BUG] User is NULL inside Enter_Game!");
                return;
            }


            MapLoad(MapSizeY, MapSizeX);
            GameUser_Session gameuser = new GameUser_Session(user, userid, user);
            Interlocked.Increment(ref UserCount);

            gameuser.GameUser_Session_Info.userPositionX = rnd.Next(1, MapSizeX);
            gameuser.GameUser_Session_Info.userPositionY = rnd.Next(1, MapSizeY);
            grid.Add(gameuser);
            lock (game_users)
            {
                if (!game_users.ContainsKey(user.Uid))
                {
                    game_users.Add(gameuser.GameUser_Session_Info.userID, gameuser);
                    gameuser.Enter_Room(this);

                }
            }
            //Console.WriteLine($"유저 카운트{UserCount}");
            S_EnterGame_Snake s_EnterGame = new S_EnterGame_Snake();
            s_EnterGame.userid = userid;
            s_EnterGame.mapSizeX = MapSizeX;
            s_EnterGame.mapSizeY = MapSizeY;
            gameuser.Send(s_EnterGame.Write());

            S_Spawn packet = new S_Spawn();
            {
                packet.userid = gameuser.GameUser_Session_Info.userID;
                packet.SpawnX = gameuser.GameUser_Session_Info.userPositionX;
                packet.SpawnY = gameuser.GameUser_Session_Info.userPositionY;
                Broadcast(packet.Write(), grid.GetNeighbors(gameuser));

            }

            S_Spawn s_EnterGame_Snake = new S_Spawn();
            foreach (var item in game_users.Values)
            {
                if (item.GameUser_Session_Info.userID == gameuser.GameUser_Session_Info.userID)
                    continue;
                s_EnterGame_Snake.SpawnX = item.GameUser_Session_Info.userPositionX;
                s_EnterGame_Snake.SpawnY = item.GameUser_Session_Info.userPositionY;
                s_EnterGame_Snake.userid = item.GameUser_Session_Info.userID;
                gameuser.Send(s_EnterGame_Snake.Write());
            }

            
            
            if (!Is_Running)
            {
                //Console.WriteLine($"{UserCount}, {Is_Running}");
                Is_Running = true;
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
            Console.WriteLine("리브 게임");
            lock (this)
            {
                if (game_users.ContainsKey(playerID) == false)
                {
                    Console.WriteLine("검색 안됨");
                    return;
                }
                else if (game_users.Count == 1)
                {
                    game_users.Clear();
                    Is_Running = false;

                }
                else
                {
                    game_users.Remove(playerID);
                }
                
                S_PlayerLeave leave = new S_PlayerLeave();
                leave.userid = session.GameUser_Session_Info.userID;
                Broadcast(leave.Write(), grid.GetNeighbors(session));
            }

        }
        public void Broadcast(CPacket msg, List<GameUser_Session> AoiList)
        {
            // [1] 명단 확보 (이 부분은 아주 훌륭함. ThreadLocal + Lock 최소화 유지)
            // ---------------------------------------------------------
            List<GameUser_Session> targets = new List<GameUser_Session>();
            if (targets == null)
            {
                return;
            }
            targets.Clear();
            targets = AoiList;
            //lock (_lock)
            //{
            //    foreach (var item in game_users)
            //    {
            //        GameUser_Session session = item.Value;
            //        if (session.UserToken.socket != null && session.UserToken.socket.Connected)
            //        {
            //            targets.Add(session);
            //        }
            //    }
            //}
            // ---------------------------------------------------------

            // [2] 전송 (Deep Copy 삭제 -> 참조 전달)
            // ---------------------------------------------------------
            
            for (int i = 0; i < targets.Count; i++)
            {
                GameUser_Session session = targets[i];
                try
                {
                    // [삭제된 것들]
                    // - CPacketBufferManger.pop() (할당 X)
                    // - SetOwner (공유 객체라 주인 설정 의미 없음)
                    // - Buffer.BlockCopy (복사 X)
                    // - move_position (커서 이동 X)

                    // [변경된 것]
                    // 그냥 원본(msg)을 던짐.
                    // 전제조건: session.Send 안에서 msg.Retain()을 하고 있어야 함!
                    session.Send(msg);
                    //Console.WriteLine($"브로드 캐스트 참조 카운트{msg._refCount}");
                }
                catch (Exception)
                {
                    // 에러 처리
                }
            }

            // [3] 뒷정리 (매우 중요)
            // ---------------------------------------------------------
            // 나는(Broadcast 함수) 100명한테 뿌리는 임무를 완수했음.
            // 내 손을 떠났으니 Release.
            // (아직 전송 중인 세션들이 Retain을 잡고 있어서 Ref Count > 0 상태임. 안 죽음)
            //Console.WriteLine($"브로드캐스트 부분 마지막 참조 카운트 반납 : {msg._refCount}");
            msg.Release();
        }
    }
}
