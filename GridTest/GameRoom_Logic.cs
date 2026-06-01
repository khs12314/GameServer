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
    public class GameRoom_Logic : IJobqueue
    {
        private readonly ThreadLocal<List<GameObject>> _threadLocalTargets
    = new ThreadLocal<List<GameObject>>(() => new List<GameObject>(200));
        //여기는 인게임 유저를 다루는 딕셔너리 로비의 딕셔너리 키와 같다. 로그인 후 제공받은 고유 키가 여기서도 동일하게 적용된다
        Dictionary<int, GameObject> WorldObjectList;
        HashSet<Player> DirtySet;
        Stack<ProJectile> proJectiles;
        public int Room_ID;
        static int Room_ID_Generator = 0;
        public int[,] map;
        public int MapSizeY = 50;
        public int MapSizeX = 50;
        object _lock;
        bool Is_Running;
        int UserCount;
        int appleX = -1;
        int appleY = -1;
        GridManager grid;
        public const float CellSize = 10.0f;

        // 컴파일러가 빌드할 때 1.0f / 10.0f를 미리 계산해서 0.1f로 박아버린다! (연산 비용 0)
        public const float InvCellSize = 1.0f / CellSize;
        GameMap gameMap;
        Random rnd = new Random();
        JobQueue jobqueue;
        JobTimer jobTimer;
        List<int> deadUser;
        const float PlayerRadius = 0.4f;
        private List<GameObject> _searchBuffer;
        private List<GameObject> _broadcastBuffer;
        public GameRoom_Logic()
        {

            jobqueue = new JobQueue();
            WorldObjectList = new Dictionary<int, GameObject>();
            Room_ID = System.Threading.Interlocked.Increment(ref Room_ID_Generator);
            Console.WriteLine($"게임 룸 생성{Room_ID}, {Room_ID_Generator}");
            _lock = new object();
            deadUser = new List<int>();
            jobTimer = new JobTimer();
            //map = new int[MapSizeY + 1, MapSizeX + 1];
            Is_Running = false;
            UserCount = 0;
            grid = new GridManager(MapSizeX, (int)CellSize);
            gameMap = new GameMap();
            MMO_MapLoad(MapSizeY, MapSizeX);
            _searchBuffer = new List<GameObject>(30);
            _broadcastBuffer = new List<GameObject>(50);
            proJectiles = new Stack<ProJectile>(100);
            DirtySet = new HashSet<Player>();
            SpawnMonster(10.5f, 0.5f, 10.5f, 1);
            SpawnMonster(11.5f, 0.5f, 11.5f, 1);
            SpawnMonster(10.5f, 0.5f, 11.5f, 1);
            Start();

        }
        public void Push(Action job)
        {
            jobqueue.Push(job);
        }

        public void PushAfter(int tickAfter, Action job)
        {
            jobTimer.PushAfter(tickAfter, () =>
            {
                Push(job);
            });
        }
        public Dictionary<int, GameObject> GetGameUsers()
        {
            return WorldObjectList;
        }
        public void SetObject(GameObject gameObject)
        {
            if (WorldObjectList.ContainsKey(gameObject.Id))
            {
                Console.WriteLine($"이미 존재하는 오브젝트 {gameObject.Id}");
                return;
            }
            WorldObjectList.Add(gameObject.Id, gameObject);

            grid.Add(gameObject);
        }
        public void Item()
        {
            return;
        }
        public List<GameObject> GetAOIGameUsers(GameObject gameObject, List<GameObject> _objectsearchBuffer)
        {
            grid.GetNeighbors(gameObject, _objectsearchBuffer);

            return _objectsearchBuffer;
        }
        //void ScheduleUpdate()
        //{

        //    Task.Delay(200).ContinueWith(t =>
        //    {
        //        Push(Update);
        //        Console.WriteLine("게임 실행중");
        //    });


        //}
        public void Start()
        {
            while (Is_Running)
            {
                Update();
                Thread.Sleep(1);
            }

        }
        public void Update()
        {
            jobTimer.Flush();
        }

        private void ProcessDeath(GameUser_Session session)
        {

            if (!session.Is_Alive)
            {
                return;
            }
            session.Is_Alive = false;
            S_Die s_Die = new S_Die();
            s_Die.userid = session.MyPlayer.Id;
            Broadcast(s_Die.Write(), session.MyPlayer);
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
            var info = session.MyPlayer;
            int x = (int)info.PosX;
            int y = (int)info.PosY;
            switch (info.RotY)
            {
                case 0: y += 1; break;
                case 1: y -= 1; break;
                case 2: x -= 1; break;
                case 3: x += 1; break;
                case -1: break;
            }
            return new GameUser_Session.Pos { x = x, z = y };
        }

        // 기능 2: 충돌 검사만 함 (상태 변경 X)

        // 기능 3: 사과 먹었는지 확인 및 사과 재생성 (사과 관련 상태만 변경)
        bool TryEatApple(GameUser_Session.Pos pos)
        {
            if (pos.x == appleX && pos.z == appleY)
            {
                //GenerateApple();
                return true;
            }
            return false;
        }

        // 기능 4: 뱀의 몸통 리스트 관리 (이동 관련 상태만 변경)
        //void MoveBody(GameUser_Session session, GameUser_Session.Pos nextPos, bool isEat)
        //{
        //    // 머리 추가
        //    session.BodyList.AddFirst(nextPos);



        //    // 사과 안 먹었으면 꼬리 자르기
        //    if (isEat == false)
        //    {
        //        if (session.BodyList.Count > 0)
        //            session.BodyList.RemoveLast();
        //    }

        //    // 좌표 정보 갱신
        //    grid.Move(session.MyPlayer, nextPos);
        //    session.MyPlayer.PosX = nextPos.x;
        //    session.MyPlayer.PosY = nextPos.z;
        //    //Console.WriteLine($"x: {session.GameUser_Session_Info.userPositionX}, y:{session.GameUser_Session_Info.userPositionY}");
        //}

        // 기능 5: 패킷 전송만 담당
        void BroadcastMove(GameUser_Session session, GameUser_Session.Pos pos, bool isEat)
        {
            S_Move move = new S_Move();
            move.playerId = session.MyPlayer.Id;
            move.posX = pos.x;
            move.posY = pos.z;
            move.IsEat = isEat;
            Broadcast(move.Write(), session.MyPlayer);
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
        public void MMO_MapLoad(int y, int x)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(baseDir, "Map");

            string mapName = "Map_00_01";
            map = new int[y, x];
            if (gameMap != null)
            {
                map = gameMap.LoadMap(folderPath, mapName);
                Console.WriteLine("generate map");
                Console.WriteLine($"{map.Length}");
            }
            else
            {
                Console.WriteLine("gameMap is Null");
                return;
            }
        }
        public void Sync_Pos(GameUser_Session session)
        {
            S_Sync syncpacket = new S_Sync();
            syncpacket.posX = session.MyPlayer.PosX;
            syncpacket.posY = session.MyPlayer.PosY;
            syncpacket.posZ = session.MyPlayer.PosZ;
            syncpacket.rotY = session.MyPlayer.RotY;
            syncpacket.playerid = session.MyPlayer.Id;
            Console.WriteLine($"sync pos : {syncpacket.posX}, {syncpacket.posZ}");
            Broadcast(syncpacket.Write(), session.MyPlayer);
        }
        public void MMO_Move(GameUser_Session session, MMO_C_Move packet)
        {
            if (session.MyPlayer.State == PlayerState.DIE)
            {
                return;
                //메소드를 만들어 보안 검사 구현 예정
            }
            var NextPos = new GameObject.Pos { x = packet.posX, z = packet.posZ };
            if (session.MyPlayer.State == PlayerState.ATTACK)
            {
                Console.WriteLine("공격 중 이동 불가");
                return;
            }
            if (!gameMap.CanGo(NextPos.x, NextPos.z))
            {
                Console.WriteLine("장애물");
                Console.WriteLine("aaa");
                Sync_Pos(session);
                return;
            }
            //Console.WriteLine("이동 가능");
            session.MyPlayer.ChangeStateAndBroadcast(PlayerState.MOVE, this);
            session.MyPlayer.PosX = packet.posX;
            session.MyPlayer.PosY = packet.posY;
            session.MyPlayer.PosZ = packet.posZ;
            session.MyPlayer.RotY = packet.rot;
            RedisManager.SavePlayerPosition(session.MyPlayer.Id, session.MyPlayer.PosX, session.MyPlayer.PosY, session.MyPlayer.PosZ);

            //Console.WriteLine($"주변 오브젝트{GetAOIGameUsers(session.MyPlayer, session.MyPlayer.AOIObjects).Count}");
            grid.Move(session.MyPlayer);
            MMO_S_Move movePacket = new MMO_S_Move();
            movePacket.posX = session.MyPlayer.PosX;
            movePacket.posY = session.MyPlayer.PosY;
            movePacket.posZ = session.MyPlayer.PosZ;
            movePacket.rot = session.MyPlayer.RotY;
            movePacket.playerid = session.MyPlayer.Id;
            movePacket.state = (byte)PlayerState.MOVE;
            Broadcast(movePacket.Write(), session.MyPlayer);
        }
        public void RedisSavePlayerInfo(GameObject gameObject)
        {
            if (gameObject is Player player)
            {
                RedisManager.SavePlayerPosition(player.Id, player.PosX, player.PosY, player.PosZ);
            }
        }
        public async void Enter_Game(User_Session user, int userid)
        {
            if (user == null)
            {
                Console.WriteLine("[BUG] User is NULL inside Enter_Game!");
                return;
            }

            //Console.WriteLine($"유저 카운트{UserCount}");


            GameUser_Session gameuser = new GameUser_Session(user.CUserToken, userid, user);
            Interlocked.Increment(ref UserCount);
            if (PacketHandler.USE_DB)
                await DBMananger.SavePlayerToDBAsync(userid);
            else
            {
                gameuser.MyPlayer.PosX = 2.5f;
                gameuser.MyPlayer.PosY = 0.5f;
                gameuser.MyPlayer.PosZ = 2.5f;
                gameuser.MyPlayer.Id = userid;
                gameuser.MyPlayer.Room = this;
                gameuser.MyPlayer.Hp = 100;
                gameuser.MyPlayer.MaxHp = 100;
            }

            grid.Add(gameuser.MyPlayer);
            lock (WorldObjectList)
            {
                if (!WorldObjectList.ContainsKey(gameuser.MyPlayer.Id))
                {
                    WorldObjectList.Add(gameuser.MyPlayer.Id, gameuser.MyPlayer);//entityID 등록
                    gameuser.Enter_Room(this);

                }
            }
            S_EnterGame_Snake s_EnterGame = new S_EnterGame_Snake();//임시
            s_EnterGame.userid = userid;
            s_EnterGame.mapSizeX = MapSizeX;
            s_EnterGame.mapSizeY = MapSizeY;
            gameuser.UserToken.send(s_EnterGame.Write());
            Spawn_User(gameuser);
            MMO_Spawn(gameuser);
            gameuser.MyPlayer.RedisSave();
            Console.WriteLine($"플레이어 스폰 : {gameuser.MyPlayer.PosX}, {gameuser.MyPlayer.PosZ}");

            //Console.WriteLine($"{UserCount}, {Is_Running}");



        }

        public void MMO_Spawn(GameUser_Session gameuser)
        {
            S_Spawn MMO_Spawn = new S_Spawn();
            foreach (var item in WorldObjectList.Values)
            {
                if (item is Player player)
                {
                    if (player.Id == gameuser.MyPlayer.Id)
                        continue;
                    MMO_Spawn.SpawnX = player.PosX;
                    MMO_Spawn.SpawnY = player.PosY;
                    MMO_Spawn.SpawnZ = player.PosZ;
                    MMO_Spawn.entityID = player.Id;
                    gameuser.UserToken.send(MMO_Spawn.Write());
                }
            }
            S_MonsterSpawn monsterSpawn = new S_MonsterSpawn();
            foreach (var item in WorldObjectList.Values)
            {
                if (item is Monster monster)
                {
                    monsterSpawn.SpawnX = monster.PosX;
                    monsterSpawn.SpawnY = monster.PosY;
                    monsterSpawn.SpawnZ = monster.PosZ;
                    monsterSpawn.entityid = monster.Id;//내부 고유 id
                    monsterSpawn.objectid = monster.MonsterID; // 오브젝트 ID로 고유 ID와는 별개로 몬스터 종류 구분
                    gameuser.UserToken.send(monsterSpawn.Write());
                    Console.WriteLine($"몬스터 정보 전송 id는 {item.Id}");
                }
            }
        }

        public void Spawn_User(GameUser_Session gameuser)
        {
            S_Spawn packet = new S_Spawn();
            {
                packet.entityID = gameuser.MyPlayer.Id;
                packet.SpawnX = gameuser.MyPlayer.PosX;
                packet.SpawnY = gameuser.MyPlayer.PosY;
                packet.SpawnZ = gameuser.MyPlayer.PosZ;
                Console.WriteLine($"패킷 : {packet.SpawnX}, {packet.SpawnZ}");
                Broadcast(packet.Write(), gameuser.MyPlayer);

            }
        }
        void Normal_Attack()
        {

        }
        public void MMO_Attack(GameUser_Session session, MMO_C_Attack packet)
        {
            if (session.MyPlayer.State == PlayerState.DIE)
            {
                return;
                //메소드를 만들어 보안 검사 구현 예정
                //예시로는 직업별로 가능한 공격인지 아닌지 판단 후 리턴
            }
            MMO_S_Attack attackPacket = new MMO_S_Attack();
            attackPacket.playerid = session.MyPlayer.Id;
            attackPacket.skillId = packet.skillId;
            attackPacket.state = (byte)PlayerState.ATTACK;
            session.MyPlayer.ChangeStateAndBroadcast(PlayerState.ATTACK, this);
            switch (packet.skillId)
            {
                case 1:

                    break;
                case 2:
                    //투사체 구현 예정
                    //Use_Skill(session, IsHitValid_Skill2);
                    break;
                case 3:
                    break;
                case 4:
                    SkillData? skill = DataManager.GetSkillData(packet.skillId);
                    if (skill == null)
                    {
                        Console.WriteLine("스킬 데이터 없음");
                        return;
                    }
                    Use_Skill(session, (targetMonster) =>
                    {

                        return MathUtils.IsHitValid(
                            session.MyPlayer.PosX, session.MyPlayer.PosZ, session.MyPlayer.RotY,
                            targetMonster.PosX, targetMonster.PosZ,
                            skill.Range, skill.Angle);
                    });
                    break;

                default:
                    Console.WriteLine("알 수 없는 스킬 ID");
                    return;
            }
            // 서버 권위 검증 실시
            Broadcast(attackPacket.Write(), session.MyPlayer);
        }

        private void Use_Skill(GameUser_Session session, Func<Monster, bool> func)
        {
            grid.GetNeighbors(session.MyPlayer, session.MyPlayer.AOIObjects);
            foreach (var item in session.MyPlayer.AOIObjects)
            {
                bool isHit = false;
                if (item is Monster monster)
                {

                    if (func(monster))
                    {
                        Console.WriteLine($"[Hit] Player {session.MyPlayer.Id} -> Target {item.Id}");
                        // TODO: 타겟 HP 차감 및 S_ChangeHp 브로드캐스트
                        ChangeHP(monster);
                    }

                }
            }
        }

        void ChangeHP(GameObject target)
        {

            target.Hp -= 10; // 예시로 10 데미지
            Console.WriteLine($"Target HP: {target.Hp}");
            if (target.Hp <= 0)
            {
                Console.WriteLine($"Target {target.Id} is dead.");
                //S_Die
                MMO_S_Die s_Die = new MMO_S_Die();
                s_Die.playerid = target.Id;
                s_Die.state = (byte)PlayerState.DIE;
                Broadcast(s_Die.Write());
                if (target is Player player)
                {

                }
                else if (target is Monster monster)
                {

                    jobTimer.PushAfter(5000, () =>
                    {
                        // 5초 후 몬스터 리스폰
                        ReSpawnMonster(monster);
                    });
                }
            }
            else
            {
                S_Damage s_Damage = new S_Damage();
                s_Damage.playerid = target.Id;
                s_Damage.damage = 10; // 예시로 10 데미지
                s_Damage.state = (byte)PlayerState.HIT;
                Broadcast(s_Damage.Write());
            }
        }
        void ChangeHP(GameObject target, int damage)
        {
            //여기서 데미지는 공격한 플레이어의 스탯 기준으로 계산된 값이 들어와야 함
            target.Hp -= damage; // 예시로 10 데미지
            if (target.Hp <= 0)
            {
                Console.WriteLine($"Target {target.Id} is dead.");
                //S_Die
                MMO_S_Die s_Die = new MMO_S_Die();
                s_Die.playerid = target.Id;
                s_Die.state = (byte)PlayerState.DIE;
                Broadcast(s_Die.Write());
                if (target is Player player)
                {
                    jobTimer.PushAfter(5000, () =>
                    {
                        // 5초 후 플레이어 리스폰
                        ReSpawnUser(player);
                    });
                }
                else if (target is Monster monster)
                {

                    jobTimer.PushAfter(5000, () =>
                    {
                        // 5초 후 몬스터 리스폰
                        ReSpawnMonster(monster);
                    });
                }
            }
            else
            {
                S_Damage s_Damage = new S_Damage();
                s_Damage.playerid = target.Id;
                s_Damage.damage = damage; // 예시로 10 데미지
                s_Damage.state = (byte)PlayerState.HIT;
                Broadcast(s_Damage.Write());
            }
        }
        public void Broadcast(CPacket msg)
        {
            foreach (var item in WorldObjectList)
            {
                if (item.Value is Player player)
                {
                    GameUser_Session session = player.Session;
                    if (session.UserToken.socket != null && session.UserToken.socket.Connected)
                    {
                        try
                        {
                            session.send(msg);
                        }
                        catch (Exception e)
                        {
                            // 에러 처리
                            Console.WriteLine($"{e.Message}");
                        }
                    }
                }
            }
            msg.Release();
        }
        public void Broadcast(CPacket msg, GameObject player)
        {
            // [1] 명단 확보 (이 부분은 아주 훌륭함. ThreadLocal + Lock 최소화 유지)
            // ---------------------------------------------------------
            grid.GetNeighbors(player, _broadcastBuffer);
            if (_broadcastBuffer == null)
            {
                Console.WriteLine("보낼 대상 없음");
                return;
            }

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
            for (int i = 0; i < _broadcastBuffer.Count; i++)
            {
                if (_broadcastBuffer[i] is Player players)
                {
                    //Console.WriteLine($"대상 플레이어 아이디: {players.Id}");
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
                        players.Session.send(msg);
                    }
                    catch (Exception)
                    {
                        // 에러 처리
                    }
                }
            }

            //Console.WriteLine($"{session.GameUser_Session_Info.userID}");
            // [3] 뒷정리 (매우 중요)
            // ---------------------------------------------------------
            // 나는(Broadcast 함수) 100명한테 뿌리는 임무를 완수했음.
            // 내 손을 떠났으니 Release.
            // (아직 전송 중인 세션들이 Retain을 잡고 있어서 Ref Count > 0 상태임. 안 죽음)
            msg.Release();
            _broadcastBuffer.Clear();
        }
        public void SpawnMonster(float x, float y, float z, int obectid)
        {
            Monster monster = new Monster(obectid);
            monster.Initialize(x, y, z, this, monster.Id);
            grid.Add(monster);
            lock (WorldObjectList)
            {
                if (!WorldObjectList.ContainsKey(monster.Id))
                {
                    WorldObjectList.Add(monster.Id, monster);
                }
            }
            monster.UpdateAI();
        }
        public void ReSpawnUser(Player player)
        {
            player.Hp = player.MaxHp;
            S_Spawn spawnPacket = new S_Spawn();
            spawnPacket.entityID = player.Id;
            spawnPacket.SpawnX = player.PosX;
            spawnPacket.SpawnY = player.PosY;
            spawnPacket.SpawnZ = player.PosZ;
            Broadcast(spawnPacket.Write());
        }
        public void ReSpawnMonster(Monster monster)
        {
            monster.Hp = monster.MaxHp;
            S_MonsterSpawn spawnMonsterPacket = new S_MonsterSpawn();
            spawnMonsterPacket.entityid = monster.Id;//고유 id 클라에서도 오브젝트 매니저에서 관리할 번호
            spawnMonsterPacket.SpawnX = monster.PosX;
            spawnMonsterPacket.SpawnY = monster.PosY;
            spawnMonsterPacket.SpawnZ = monster.PosZ;
            spawnMonsterPacket.objectid = 1; // 몬스터 타입 ID 프리펩에서 생성할 때 구분짓기용
            Broadcast(spawnMonsterPacket.Write());
        }
        public GameObject FindObjectById(int id)
        {
            if (WorldObjectList.TryGetValue(id, out GameObject obj))
                return obj;
            return null;
        }
        public void LeavGame(GameUser_Session session, int playerID)
        {
            Console.WriteLine("리브 게임");
            lock (this)
            {
                if (WorldObjectList.ContainsKey(playerID) == false)
                {
                    Console.WriteLine("검색 안됨");
                    return;
                }
                else if (WorldObjectList.Count == 1)
                {
                    WorldObjectList.Clear();
                    Is_Running = false;

                }
                else
                {
                    WorldObjectList.Remove(playerID);
                }
                Console.WriteLine("체인지 피어");
                session.Change_Peer();
                S_PlayerLeave leave = new S_PlayerLeave();
                leave.userid = session.MyPlayer.Id;
                Broadcast(leave.Write(), session.MyPlayer);
            }
        }
        public void ChangeDir(GameUser_Session session, C_Move packet)
        {
            lock (_lock)
            {
                //session.GameUser_Session_Info.dir = packet.direction;
            }
        }
        public ProJectile Get_Projectile()
        {
            if (proJectiles.Count > 0)
            {
                return proJectiles.Pop();
            }
            else
            {
                // 필요에 따라 새로운 프로젝타일 생성
                return new ProJectile();
            }
        }
        public void Return_Projectile(ProJectile proJectile)
        {
            proJectiles.Push(proJectile);
        }
        public void Add_Projectile(SkillData skillData, float dirx, float dirz, ProJectile proJectile, GameObject gameObject)
        {
            proJectile.Initialize(gameObject.Id,
                   gameObject.PosX, gameObject.PosY, gameObject.PosZ,
                    dirx, dirz, 3000, skillData.Id, skillData.Damage, gameObject, skillData.Speed);
            Console.WriteLine($"{gameObject.PosX}");
            S_UseSkill s_UseSkill = new S_UseSkill();
            s_UseSkill.playerid = gameObject.Id;
            s_UseSkill.posX = gameObject.PosX;
            s_UseSkill.posY = gameObject.PosY;
            s_UseSkill.posZ = gameObject.PosZ;
            s_UseSkill.rotY = gameObject.RotY;
            s_UseSkill.dirx = dirx;
            s_UseSkill.dirz = dirz;
            s_UseSkill.skillId = skillData.Id;
            Broadcast(s_UseSkill.Write(), gameObject);


        }

        public void Get_Item(GameUser_Session gameUser_Session, C_MMOGetItem c_MMOGetItem)
        {
            GameObject obj = FindObjectById(c_MMOGetItem.ItemID);
            Player player = gameUser_Session.MyPlayer;
            if (!(obj is Item item))
            {
                return;
            }
            float dx = player.PosX - item.PosX;
            float dz = player.PosZ - item.PosZ;
            float sqrDist = (dx * dx) + (dz * dz);
            if (sqrDist > 4.0f) // 예시로 1.0f 이상 떨어져 있으면 아이템 획득 불가
            {
                Console.WriteLine("아이템과의 거리가 멀어서 획득 불가");
                return;
            }

            bool isGetItem = player.Inventory.AddItem(item);
            if (isGetItem)
            {
                S_MMOGetItem s_MMOGetItem = new S_MMOGetItem();
                s_MMOGetItem.ItemID = c_MMOGetItem.ItemID;
                Broadcast(s_MMOGetItem.Write(), gameUser_Session.MyPlayer);
                S_MMODespawnItem s_MMODespawnItem = new S_MMODespawnItem();
                s_MMODespawnItem.entityID = c_MMOGetItem.ItemID;
                Broadcast(s_MMODespawnItem.Write(), gameUser_Session.MyPlayer);
                WorldObjectList.Remove(c_MMOGetItem.ItemID);
                DirtySet.Add(player);
            }
            
        }
    }
}
