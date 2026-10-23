using GridTest.InGameObj.PlayerInfo;
using GridTest.Manager;
using GridTest.Network;
using GridTest.Room;
using GridTest.Util;

public class ProJectile : GameObject, IPoolable
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
    public void Reset()
    {
               // 초기화 로직 (필요에 따라 구현)
        this.Owner = null;
        this.OwnerId = 0;
        this.Speed = 0;
        this.DirectionX = 0;
        this.DirectionZ = 0;
        this.ExpireTime = 0;
        this.ProjectileId = 0;
        this.Damage = 0;
    }
    private void HandleHit(Monster targetmonster)
    {
        // 데미지 적용
        targetmonster.Hp -= this.Damage;

        // 패킷 생성 및 브로드캐스트
        S_Damage damagePacket = NetworkPacketPool<S_Damage>.Pop();
        damagePacket.Clear();

        damagePacket.playerid = targetmonster.Id;
        damagePacket.damage = this.Damage;

        if (targetmonster.Hp <= 0)
        {
            targetmonster.Hp = 0;

            damagePacket.state = (byte)PlayerState.DIE;

            // 부활 예약 (5초 뒤)
            Room.Push(() =>
            {
                targetmonster.OnDead(this.Room);
                targetmonster.OnDead(Owner);
            });
        }
        else
        {
            damagePacket.state = (byte)PlayerState.HIT;
        }

        Room.Broadcast(damagePacket.Write(), targetmonster);
        NetworkPacketPool<S_Damage>.Push(damagePacket);
        // 투사체 소멸 및 반납
        Room.Return_Projectile(this);
    }
}

