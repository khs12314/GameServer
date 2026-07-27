using GridTest.DataManagers;
using GridTest.InGameObj.PlayerInfo;
using GridTest.Manager;
using GridTest.Network;
using GridTest.Room;
using GridTest.Util;
using NetWorkModule;

public class Monster : GameObject
{
   
    public long NextSearchTime { get; set; } = 0;
    public long NextMoveTime { get; set; } = 0;
    public long NextAttackTime { get; set; } = 0;
    public int TargetId { get; set; } = 0;
    public long Exp { get; set; }
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
        ObjectId = objectid;
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
        MonsterData data = MonsterDataManager.GetMonsterData(this.ObjectId);
        if (data != null)
        {
            this.MaxHp = data.MaxHp;
            this.Hp = data.MaxHp; // 생성 시 체력을 꽉 채워줌
            this.Exp = data.Exp;
        }
        Hp = MaxHp;
        State = PlayerState.IDLE;
        TargetId = 0;
        this.Room = Room;
        ReSpawnTimeMs = 5000;
        AtkWaitMs = MonsterAniDataManager.GetAniData(ObjectId).AtkWaitMs;
        AtkTotalTimeMs = MonsterAniDataManager.GetAniData(ObjectId).AtkTotalMs;
        Room.PushAfter(tickAfter, UpdateAI); // AI 업데이트 시작
        Console.WriteLine($"공격 토탈 시간 {AtkTotalTimeMs}");
    }
    public void OnDead(GameRoom_Logic Room)
    {
        this.Hp = 0;
        int itemid = ItemDropManager.GetRandomLoot(this.ObjectId); // 몬스터 사망 시 아이템 드롭 처리
        ItemType itemType = (ItemType)ItemDataManager.GetItemData(itemid).type;
        int maxstack = ItemDataManager.GetItemData(itemid).MaxStack;
        Item droppedItem = new Item();
        droppedItem.Initialize(this.PosX, this.PosY, this.PosZ, Room, itemid, itemType, maxstack);

        ChangeStateAndBroadcast(PlayerState.DIE, Room);
        //Console.WriteLine($"[몬스터 사망] 몬스터 {this.Id} 사망. {ReSpawnTimeMs / 1000}초 뒤 부활 예약.");
        Room.PushAfter(ReSpawnTimeMs, () =>
        {
            Respawn(Room);
        });
    }
    public void OnDead(GameObject attacker)
    {
        if (attacker is Player playerAttacker)
        {
            playerAttacker.OnKillMonster(this);

        }
        else
        {

        }
    }

    private void Respawn(GameRoom_Logic Room)
    {
        this.Hp = this.MaxHp;
        this.PosX = ReSpawnPosX;
        this.PosZ = ReSpawnPosZ;
        ChangeStateAndBroadcast(PlayerState.IDLE, Room);
        S_Sync s_Sync = NetworkPacketPool<S_Sync>.Pop();
        s_Sync.Clear();
        s_Sync.posX = this.PosX;
        s_Sync.playerid = this.Id;
        s_Sync.posY = this.PosY;
        s_Sync.posZ = this.PosZ;
        s_Sync.rotY = this.RotY;

        Room.Broadcast(s_Sync.Write(), this);
        NetworkPacketPool<S_Sync>.Push(s_Sync);
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
        if (sqrDist > 49.0f) // 시야 범위 7m 밖으로 완전히 벗어났다면?
        {
            ChangeStateAndBroadcast(PlayerState.IDLE, Room);
            TargetId = 0;
            //Console.WriteLine($"[AI] 몬스터 {this.Id} 타겟 {targetuser.Id} 완전 상실. 대기 모드로 전환.");
            return 1000; // 타겟 완전 상실. 1초 대기 모드로 복귀
        }
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

        // 5. 이동 패킷 브로드캐스트 (아까 낮에 추가한 state 필드 포함) 추후 룸 로직으로 뺄 예정 여기에 있으면 안됨
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
                // Redis DB 실시간 호출 절대 금지. 메모리만 바꾸고 나중에 Write-Back.
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
            S_Sync spawnPacket = NetworkPacketPool<S_Sync>.Pop();
            spawnPacket.Clear();
            spawnPacket.playerid = targetPlayer.Id;
            spawnPacket.posX = targetPlayer.PosX;
            spawnPacket.posZ = targetPlayer.PosZ;

            Room.Broadcast(spawnPacket.Write(), this);
            NetworkPacketPool<S_Sync>.Push(spawnPacket);
        });
    }
}

