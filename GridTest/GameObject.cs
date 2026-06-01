using GridTest;
using NetWorkModule;
using SnakeServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{

    public class Inventory
    {
        const int MaxSlots = 20;
        public Item[] Slots { get; private set; } = new Item[MaxSlots];
        public Dictionary<long, Item> Items { get; private set; } = new Dictionary<long, Item>();
        public bool AddItem(Item item)
        {
            if (item.SlotIndex != Item.INVALID_SLOT)
            {
                return false; // 이미 어딘가에 소속된 아이템
            }

            // 아이템 타입 검사: 장비(2)가 아니면 모두 중첩 가능하다고 가정
            bool isStackable = (item.ItemType != 2);

            // 1단계: 소모품(Stackable)인 경우, 합칠 수 있는 기존 슬롯부터 찾는다.
            if (isStackable)
            {
                for (int i = 0; i < MaxSlots; i++)
                {
                    Item existItem = Slots[i];

                    // 빈칸이 아니고, 같은 종류의 아이템이며, 아직 덜 찬 경우
                    if (existItem != null && existItem.ItemID == item.ItemID && existItem.Count < existItem.MaxStack)
                    {
                        int space = existItem.MaxStack - existItem.Count;
                        int addCount = Math.Min(space, item.Count);

                        // 기존 아이템에 개수 추가
                        existItem.Count += addCount;
                        item.Count -= addCount;

                        // ★ DB 저장을 위해 기존 아이템의 상태를 Dirty로 마킹
                        existItem.IsDirty = true;

                        // 모두 합쳤으면 성공 반환
                        if (item.Count <= 0)
                        {
                            return true;
                        }
                    }
                }
            }

            // 2단계: 여전히 아이템이 남았거나, 처음부터 중첩 불가(장비)인 경우 빈 슬롯에 넣는다.
            // (조건: 파이프라인 단에서 이미 item.Count <= item.MaxStack 으로 맞춰져 들어왔다고 가정)
            for (int i = 0; i < MaxSlots; i++)
            {
                if (Slots[i] == null)
                {
                    // 투트랙(Two-Track) 컬렉션 양쪽에 등록
                    item.SlotIndex = i;
                    Slots[i] = item;
                    Items[item.ItemID] = item;

                    // DB 저장을 위해 새 아이템의 상태를 Dirty로 마킹
                    item.IsDirty = true;

                    return true;
                }
            }

            // 빈칸이 없으면 실패 (클라이언트에 인벤토리 풀(Full) 패킷 전송 필요)
            return false;

        }
    }
}
public class GameObject
{
    public int Id { get; set; } // 방 내부에서의 고유 ID (Entity ID)
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public float RotY { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public struct Pos { public float x, z; }
    public int CellX { get; set; }
    public int CellY { get; set; }

    public GameRoom_Logic Room { get; set; }
    public List<GameObject> AOIObjects { get; set; } = new List<GameObject>();
    public PlayerState State { get; protected set; }
    public GameObjectType gameObjectType { get; set; }
    // GameObject나 Monster 클래스 내부의 명시적 함수
    public void ChangeStateAndBroadcast(PlayerState newState, GameRoom_Logic room)
    {
        // [Guard Clause] 이전 상태와 똑같다면 패킷을 쏘지 않고 즉시 리턴 (대역폭 낭비 방어)
        if (this.State == newState)
            return;

        // 1. 상태 갱신
        this.State = newState;

        // 2. 패킷 조립 (Zero-Allocation을 위한 struct 또는 Pool 사용 권장)
        S_ChangeState statePacket = new S_ChangeState();
        statePacket.playerid = this.Id;
        statePacket.state = (byte)this.State;
        Console.WriteLine($"상태 변화 {(PlayerState)this.State}");
        // 3. 룸을 통한 브로드캐스트
        room.Broadcast(statePacket.Write(), this);
    }
    public virtual void Initialize()
    {
        this.State = PlayerState.IDLE;//예외적으로 GameObject는 초기화 시 IDLE 상태로 시작하도록 설정. Player나 Monster에서 별도의 초기화 로직이 있다면 그 안에서 다시 상태를 조정할 수 있음.
    }

}
public class Equipment : Item
{
    public int EnchantLevel { get; set; }
    public int Durability { get; set; }
}
public class Item : GameObject
{
    public const int INVALID_SLOT = -1;
    internal bool IsDirty;

    public int ItemID { get; set; } // 아이템 종류 식별용 ID (예: Potion=1, Sword=2 등)
    public int Count { get; set; } // 아이템 개수 (소모품인 경우 1~99, 장비는 항상 1)
    public byte ItemType { get; set; } // 아이템 타입 (예: Equipment, Consumables 등)
    public int SlotIndex { get; set; } = INVALID_SLOT; // 인벤토리 슬롯 인덱스 (0~19), INVALID_SLOT은 인벤토리에 없는 상태를 의미
    public int MaxStack { get; set; } // 최대 스택 수량 (소모품인 경우 99, 장비는 1 등)

    public Item()
    {
        gameObjectType = GameObjectType.Item; // Item 타입
        SlotIndex = INVALID_SLOT; // 기본값은 인벤토리에 없는 상태
        Count = 1; // 기본적으로 1개로 시작 (소모품은 나중에 합쳐질 때 Count가 증가할 수 있음)
    }
    public void Initialize(float x, float y, float z, GameRoom_Logic Room, int itemId, byte type, int maxstack)
    {
        Room.Push(() =>
        {
            PosX = x;
            PosY = y;
            PosZ = z;
            CellX = (int)(PosX * GameRoom_Logic.InvCellSize);
            CellY = (int)(PosZ * GameRoom_Logic.InvCellSize);
            this.Room = Room;
            ItemID = itemId;
            ItemType = type;
            MaxStack = maxstack;
            Id = ObjectManager.GenerateId(GameObjectType.Item);
            S_MMOSpawnItem packet = new S_MMOSpawnItem()
            {
                ItemID = this.ItemID,
                entityID = this.Id,
                posX = this.PosX,
                posY = this.PosY,
                posZ = this.PosZ
            };
            Room.Broadcast(packet.Write(), this);
            Room.SetObject(this);
            Room.Item();
        });
        Room.PushAfter(30000, () =>
        {
            // 30초 뒤 아이템 소멸
            Room.Broadcast(new S_MMODespawnItem() { entityID = this.Id }.Write(), this);
            Room.GetGameUsers().Remove(this.Id); // 룸의 오브젝트 리스트에서도 제거
        });
    }
}
public class ProJectile : GameObject
{
    public GameObject Owner { get; set; }
    public int OwnerId { get; set; }
    public float Speed { get; set; }
    public float DirectionX { get; set; }
    public float DirectionZ { get; set; }
    public long ExpireTime { get; set; }
    public int ProjectileId { get; set; }//DataManager에서 스킬 아이디로 데미지나 속도 가져올 때 사용할 예정

    public int Damage { get; set; }

    public ProJectile()
    {
        gameObjectType = GameObjectType.Projectile; // Projectile 타입

    }
    public void Initialize(int ownerId, float startX, float startY, float startZ, float dirX, float dirZ, long ExpireTime, int ProjectileId, int damage, GameObject owner, float speed)
    {
        this.Room = owner.Room;
        Room.Push(() =>
        {
            this.Damage = damage;
            this.Owner = owner;
            OwnerId = ownerId;
            PosX = startX;
            PosY = startY;
            PosZ = startZ;
            DirectionX = dirX;
            Id = ObjectManager.GenerateId(GameObjectType.Projectile);
            DirectionZ = dirZ;
            this.CellX = (int)(this.PosX * GameRoom_Logic.InvCellSize);
            this.CellY = (int)(this.PosZ * GameRoom_Logic.InvCellSize);
            Speed = speed;
            this.ExpireTime = ExpireTime + Environment.TickCount64;
            this.ProjectileId = ProjectileId;//생성 시 누른 키에 따라 스킬 아이디가 달라질 예정
                                             //예시로는 csv로 파싱된 스킬 파이어볼이 유니티 상에서도 똑같은 csv 파일을 읽어와 유니티에서도 파이어볼에 매칭시켜놓은
                                             //키를 누르면 그에 맞는 id를 보냄, typeid 같은 경우에는 더 윗단계에서 일반공격인지 투사체공격인지 판단하기 위해 구분지을 때 쓸 예정
            Room.SetObject(this);
        });

        Update();
    }
    public void Update()
    {
        // [중요] Update는 이미 Room.PushAfter에 의해 Job Queue 내부에서 실행 중이므로,
        // 내부에서 별도의 Room.Push를 또 호출할 필요가 없다. (Lock-Free)

        // 1. 수명 체크
        if (Environment.TickCount64 >= ExpireTime)
        {
            //Console.WriteLine($"[Projectile] 수명 만료 소멸. 현재 틱카운트:{Environment.TickCount64}, 소멸 카운트 : {ExpireTime} ");
            Room.Return_Projectile(this); // 풀 반납 및 리스트 제거
            Room.GetGameUsers().Remove(this.Id);
            return; // 반드시 종료!
        }

        // 2. 이동 로직
        this.PosX += DirectionX * Speed * 0.1f;
        this.PosZ += DirectionZ * Speed * 0.1f;
        this.CellX = (int)(this.PosX * GameRoom_Logic.InvCellSize);
        this.CellY = (int)(this.PosZ * GameRoom_Logic.InvCellSize);
        // Console.WriteLine($"투사체 위치 추적 : {this.PosX}, {this.PosZ}");

        //Console.WriteLine($"투사체 주변 오브젝트 수 : {Room.GetAOIGameUsers(this, this.AOIObjects).Count()}");
        foreach (var monster in Room.GetAOIGameUsers(this, this.AOIObjects))
        {
            if (monster is Monster targetmonster)
            {
                // 사망한 몬스터는 무시
                if (targetmonster.State == PlayerState.DIE) continue;

                float dx = targetmonster.PosX - this.PosX;
                float dz = targetmonster.PosZ - this.PosZ;

                if ((dx * dx) + (dz * dz) <= 4.0f)
                {
                    // [원자적 처리] 데미지 계산, 패킷 생성, 투사체 소멸을 한 번에 처리
                    Console.WriteLine("Hit");
                    HandleHit(targetmonster);
                    return; // 몬스터를 맞췄으므로 즉시 함수 종료 (Update 예약 안 함)
                }
            }
        }

        // 3. 충돌 검사

        // 4. 아무것도 안 맞았고 수명도 남았다면 다음 틱 예약
        Room.PushAfter(100, Update);
    }

    private void HandleHit(Monster targetmonster)
    {
        // 데미지 적용
        targetmonster.Hp -= this.Damage;

        // 패킷 생성 및 브로드캐스트
        S_Damage damagePacket = new S_Damage() { playerid = targetmonster.Id, damage = this.Damage };

        if (targetmonster.Hp <= 0)
        {
            targetmonster.Hp = 0;

            damagePacket.state = (byte)PlayerState.DIE;

            // 부활 예약 (5초 뒤)
            Room.PushAfter(5000, () => { targetmonster.OnDead(this.Room); });
        }
        else
        {
            damagePacket.state = (byte)PlayerState.HIT;
        }

        Room.Broadcast(damagePacket.Write(), targetmonster);

        // 투사체 소멸 및 반납
        Room.Return_Projectile(this);
    }
}

public class Player : GameObject
{
    public int PlayerID { get; set; }//플레이어 전용 고유 아이디 db에서 가져올 내용(계정 생성 순서에 따라 저정된 db의 pk값)
    public Inventory Inventory { get; set; } = new Inventory();
    public GameUser_Session Session { get; set; }
    private const int LOOT_COOLDOWN_MS = 200;
    private long _lastLootTick = 0;
    public Player(int playerID, GameUser_Session session)
    {
        gameObjectType = GameObjectType.Player; // Player 타입
        PlayerID = playerID;//db에서 발급받은 고유id
        Session = session;
        this.Id = ObjectManager.GenerateId(GameObjectType.Player);//방 접속 시 사용할 고유 id
    }
    public void RedisSave()
    {
        if (this.Room == null || !Session.UserToken.socket.Connected /* 혹은 this.Session.IsConnected == false 등 자네의 종료 조건 */)
        {
            Console.WriteLine($"[Redis 틱 종료] {this.Id}번 유저 퇴장. 주기적 저장 루프 정지.");
            return; // 여기서 return 해버리면 더 이상 PushAfter가 안 돌고 소멸한다.
        }
        RedisManager.SavePlayerState(this.Id, this.Hp, this.PosX, this.PosY, this.PosZ);
        this.Room.PushAfter(1000, RedisSave); // 1초마다 DB에 저장 예약
    }
    public bool CanLoot()
    {
        long now = Environment.TickCount64;
        if (now - _lastLootTick < LOOT_COOLDOWN_MS)
        {
            return false; // 아직 쿨타임이 안 지났음
        }
        _lastLootTick = now;
        return true;
    }
}
public class Monster : GameObject
{
    public int MonsterID { get; set; }  // 몬스터 종류 식별용 ID (예: Goblin=1, Orc=2 등)
    public long NextSearchTime { get; set; } = 0;
    public long NextMoveTime { get; set; } = 0;
    public long NextAttackTime { get; set; } = 0;
    public int TargetId { get; set; } = 0;
    int tickAfter;
    public float ReSpawnPosX { get; set; }
    public float ReSpawnPosZ { get; set; }
    public int ReSpawnTimeMs { get; set; } = 5000; // 5초 뒤 부활
    public int AttackRange { get; set; } = 2; // 공격 사거리 (2m)
    public int ViewRange { get; set; } = 7; // 시야 범위 (7m)
    public int AtkWaitMs { get; set; } // 공격 대기 시간 (애니메이션 준비 시간)
    public int AtkTotalTimeMs { get; set; } // 공격 총 시간 (애니메이션 전체 시간, 타격 판정 시점 포함)
    public long AttackTimer { get; set; } = 0; // 공격 타이머 (쿨타임 관리용)

    public Monster(int objectid)
    {

        this.State = PlayerState.IDLE; // 태어날 땐 무조건 대기 상태
        tickAfter = 1000; // 1초마다 AI 업데이트
        Room = null;
        MonsterID = objectid;
        gameObjectType = GameObjectType.Monster; // Monster 타입
        this.Id = ObjectManager.GenerateId(GameObjectType.Monster);//방 접속 시 사용할 고유 id

    }
    public void Initialize(float x, float y, float z, GameRoom_Logic Room, int id)
    {
        this.Id = id;
        PosX = x;
        PosY = y;
        PosZ = z;
        ReSpawnPosX = x;
        ReSpawnPosZ = z;
        Hp = 100;
        State = PlayerState.IDLE;
        MaxHp = 100;
        TargetId = 0;
        this.Room = Room;
        ReSpawnTimeMs = 5000;
        AtkWaitMs = DataManager.GetAniData(MonsterID).AtkWaitMs;
        AtkTotalTimeMs = DataManager.GetAniData(MonsterID).AtkTotalMs;
        Room.PushAfter(tickAfter, UpdateAI); // AI 업데이트 시작
        Console.WriteLine($"공격 토탈 시간 {AtkTotalTimeMs}");
    }
    public void OnDead(GameRoom_Logic Room)
    {
        this.Hp = 0;
        ChangeStateAndBroadcast(PlayerState.DIE, Room);
        //Console.WriteLine($"[몬스터 사망] 몬스터 {this.Id} 사망. {ReSpawnTimeMs / 1000}초 뒤 부활 예약.");
        int itemid = ItemDropManager.GetRandomLoot(this.MonsterID); // 몬스터 사망 시 아이템 드롭 처리
        byte itemType = DataManager.GetItemData(itemid).type;
        int maxstack = DataManager.GetItemData(itemid).MaxStack;
        Item droppedItem = new Item();
        droppedItem.Initialize(this.PosX, this.PosY, this.PosZ, Room, itemid, itemType, maxstack);
        Room.PushAfter(ReSpawnTimeMs, () =>
        {
            Respawn(Room);
        });
    }

    private void Respawn(GameRoom_Logic Room)
    {
        this.Hp = this.MaxHp;
        this.PosX = ReSpawnPosX;
        this.PosZ = ReSpawnPosZ;
        ChangeStateAndBroadcast(PlayerState.IDLE, Room);
        S_Sync s_Sync = new S_Sync()
        {
            playerid = this.Id,
            posX = this.PosX,
            posY = this.PosY,
            posZ = this.PosZ,
            rotY = this.RotY
        };
        Room.Broadcast(s_Sync.Write(), this);
        //Console.WriteLine($"[몬스터 부활] 몬스터 {this.Id} 부활 완료. 위치: ({this.PosX}, {this.PosZ})");
        UpdateAI(); // 부활과 동시에 AI 업데이트 시작
    }

    public void UpdateAI()
    {
        long now = Environment.TickCount64;
        //Console.WriteLine($"posx : {this.PosX}, posz : {this.PosZ}");
        if (this.Hp <= 0)
        {
            ChangeStateAndBroadcast(PlayerState.DIE, Room);
            return;
        }
        if (now < AttackTimer)
        {
            int remainingTime = (int)(AttackTimer - now);
            Room.PushAfter(remainingTime, UpdateAI); // 공격 애니메이션 준비 시간 동안 0.1초마다 상태 유지 체크
            return;
        }
        switch (this.State)
        {
            case PlayerState.IDLE:
                tickAfter = Update_Idle();
                break;
            case PlayerState.ATTACK:
                tickAfter = Update_Attack();
                AttackTimer = Environment.TickCount64 + AtkTotalTimeMs; // 공격 애니메이션 준비 시간 동안 AI 틱 멈춤
                break;
            case PlayerState.MOVE:
                tickAfter = Update_Move();
                break;
            case PlayerState.HIT:
                break;
        }
        //Console.WriteLine($"[AI Tick] 몬스터 {this.Id} 생존 : {this.Id}");
        //Console.WriteLine($"현재 상태 {(PlayerState)this.State}");
        this.Room.PushAfter(tickAfter, UpdateAI); // 1초마다 AI 업데이트
    }
    int Update_Idle()
    {
        foreach (var player in Room.GetAOIGameUsers(this, AOIObjects))
        {
            if (player is Player targetPlayer)
            {
                float dx = targetPlayer.PosX - this.PosX;
                float dz = targetPlayer.PosZ - this.PosZ;
                float sqrDist = (dx * dx) + (dz * dz);
                if (sqrDist <= 49.0f)
                {
                    TargetId = targetPlayer.Id;
                    ChangeStateAndBroadcast(PlayerState.MOVE, Room);
                    //Console.WriteLine($"[AI] 몬스터 {this.Id}가 유저 {targetPlayer.Id} 발견! 추적 모드 돌입. {sqrDist}");
                    return 100;
                }
                else
                {
                    //Console.WriteLine($"{sqrDist}");
                }
            }
        }
        return 1000; // 1초마다 주변 탐색
    }
    int Update_Move()
    {
        if (!Room.GetGameUsers().TryGetValue(TargetId, out var target) || target is not Player targetuser)
        {
            ChangeStateAndBroadcast(PlayerState.IDLE, Room); // 타겟이 사라지면 다시 대기 상태로
                                                             //Console.WriteLine($"[AI] 몬스터 {this.Id}의 타겟이 사라짐. 대기 모드로 전환.");
            return 1000;
        }
        float dx = target.PosX - this.PosX;
        float dz = target.PosZ - this.PosZ;
        float sqrDist = (dx * dx) + (dz * dz);

        // 사거리(예: 2m) 이내로 좁혀졌다면?
        if (sqrDist <= 4.0f) // 2의 제곱
        {
            ChangeStateAndBroadcast(PlayerState.ATTACK, Room);
            //Console.WriteLine($"[AI] 몬스터 {this.Id} 사거리 진입. 공격 모드 돌입.");
            return 100; // 공격 틱으로 즉시 전이
        }

        // 3. 방향 벡터 정규화 (Normalization)
        // 이동 좌표를 구해야 하므로 여기서는 Math.Sqrt가 불가피하다.
        float dist = (float)Math.Sqrt(sqrDist);
        float dirX = dx / dist;
        float dirZ = dz / dist;

        // 4. 좌표 갱신 (초당 5.0f 이동, 현재 틱은 100ms(0.1초) 단위)
        float speed = 5.0f;
        float tickDelta = 0.1f;

        this.PosX += dirX * speed * tickDelta;
        this.PosZ += dirZ * speed * tickDelta;
        //Console.WriteLine($"움직이는 중 현재 위치 {this.Id} : {this.PosX}, {this.PosZ}");
        // (선택) 몬스터가 바라보는 방향 Y축 회전값 갱신
        this.RotY = (float)(Math.Atan2(dx, dz) * (180 / Math.PI));

        // 5. 이동 패킷 브로드캐스트 (아까 낮에 추가한 state 필드 포함)
        MMO_S_Move mMO_S_Move = new MMO_S_Move();
        mMO_S_Move.playerid = this.Id;
        mMO_S_Move.posX = this.PosX;
        mMO_S_Move.posY = this.PosY;
        mMO_S_Move.posZ = this.PosZ;
        mMO_S_Move.rot = this.RotY;
        mMO_S_Move.state = (byte)PlayerState.MOVE; // 프로토콜에 state를 추가했다면 활성화
        Room.Broadcast(mMO_S_Move.Write());


        return 1000; // 0.1초(100ms) 뒤에 다시 이동 연산 수행
    }
    private int Update_Attack()
    {
        // 1. 타겟 유효성 검사 (유저가 접속을 끊었거나 죽었다면)
        if (!Room.GetGameUsers().TryGetValue(TargetId, out var target) || target is not Player targetPlayer || targetPlayer.Hp <= 0)
        {
            ChangeStateAndBroadcast(PlayerState.IDLE, Room);
            TargetId = 0;
            Console.WriteLine("타겟 상실");
            return 1000; // 타겟 상실. 1초 대기 모드로 복귀
        }

        // 2. 거리 재확인 (유저가 얄밉게 뒤로 빠져서 사거리 2m 밖으로 벗어났다면?)
        float dx = target.PosX - this.PosX;
        float dz = target.PosZ - this.PosZ;
        float sqrDist = (dx * dx) + (dz * dz);
        if (sqrDist > 49.0f) // 시야 범위 7m 밖으로 완전히 벗어났다면?
        {
            ChangeStateAndBroadcast(PlayerState.IDLE, Room);
            TargetId = 0;
            Console.WriteLine("타겟 완전 상실");
            return 1000; // 타겟 완전 상실. 1초 대기 모드로 복귀
        }
        else if (sqrDist > 4.0f) // 2의 제곱
        {
            ChangeStateAndBroadcast(PlayerState.MOVE, Room);
            return 100; // 다시 0.1초 틱으로 쫓아간다 (Trace 상태 복귀)
        }


        long now = Environment.TickCount64;
        // 3. 공격 쿨타임 검사 (핵심)
        if (now < NextAttackTime) return 100; // 쿨타임 대기

        // 1. 타격 발생
        NextAttackTime = now + 2000; // 2초 쿨타임
        Room.PushAfter(AtkWaitMs, () =>
        {
            // [중요: Ghost Attack 방어] 0.8초 동안 몬스터가 죽었거나 상태가 바뀌었다면 공격 취소!
            if (this.State != PlayerState.ATTACK) return;

            // 타겟이 방을 나갔거나 이미 죽었다면 공격 헛방(Miss)
            if (!Room.GetGameUsers().TryGetValue(TargetId, out var target) || !(target is Player targetPlayer) || targetPlayer.Hp <= 0)
            {
                return; // void 함수이므로 값 없이 return
            }

            // 2. 타격 판정 (사거리 검사)
            if (MathUtils.IsHitValid(this.PosX, this.PosZ, this.RotY, targetPlayer.PosX, targetPlayer.PosZ, 2.0f, 60.0f))
            {
                targetPlayer.Hp -= 10;
                // Redis/DB 실시간 호출 절대 금지. 메모리만 바꾸고 나중에 Write-Back.
                Console.WriteLine("타격");
                // 사망 처리
                if (targetPlayer.Hp <= 0)
                {
                    HandlePlayerDeath(targetPlayer);
                }
            }
            else
            {
                Console.WriteLine("공격 빗나감");
            }
        });

        // 3. 상태 고정 (핵심)
        // 공격 애니메이션 총 길이(AtkTotalMs) 동안 몬스터의 AI 틱을 멈춘다.
        return AtkTotalTimeMs;

    }

    private void HandlePlayerDeath(Player targetPlayer)
    {
        // 1. 상태 전이
        targetPlayer.ChangeStateAndBroadcast(PlayerState.DIE, Room);

        // 2. 몬스터는 타겟을 잃었으므로 타겟 초기화 (상태 전이는 Update 로직에서 처리됨)
        this.TargetId = 0;

        // 3. 5초 뒤 부활 예약
        Room.PushAfter(5000, () =>
        {
            // [중요: Disconnect 방어] 5초 동안 유저가 랜선을 뽑고 나갔다면 부활 로직 취소
            if (targetPlayer.Room != this.Room) return;

            targetPlayer.Hp = targetPlayer.MaxHp;
            targetPlayer.PosX = 2.5f; // 마을 좌표
            targetPlayer.PosZ = 2.5f;

            targetPlayer.ChangeStateAndBroadcast(PlayerState.IDLE, Room);

            // 클라이언트로 텔레포트(부활) 패킷 발송
            S_Sync spawnPacket = new S_Sync()
            {
                playerid = targetPlayer.Id,
                posX = targetPlayer.PosX,
                posZ = targetPlayer.PosZ
            };
            Room.Broadcast(spawnPacket.Write(), this);
        });
    }
}

