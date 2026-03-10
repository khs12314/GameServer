using NetWorkModule;
using SnakeServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
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

        public PlayerState State { get; set; }
    }
    public class Player : GameObject
    {
        public GameUser_Session Session { get; set; }

    }
    public class Monster : GameObject
    {
        public int MonsterID { get; set; }
        public long NextSearchTime { get; set; } = 0;
        public long NextMoveTime { get; set; } = 0;
        public long NextAttackTime { get; set; } = 0;
        public int TargetId { get; set; } = 0;
        public Monster()
        {

            this.State = PlayerState.IDLE; // 태어날 땐 무조건 대기 상태
            Room = null;
        }
        public void UpdateAI()
        {
            if (this.Hp <= 0)
            {
                this.State = PlayerState.DIE;
                return;
            }
            int tickAfter = 1000;
            switch (this.State)
            {
                case PlayerState.IDLE:
                    tickAfter = Update_Idle();
                    break;
                case PlayerState.ATTACK:
                    tickAfter = Update_Attack();
                    break;
                case PlayerState.MOVE:
                    tickAfter = Update_Move();
                    break;
                case PlayerState.HIT:
                    break;
            }
            Console.WriteLine($"[AI Tick] 몬스터 {this.Id} 생존 : {this.Id}");
            Console.WriteLine($"현재 상태 {(PlayerState)this.State}");
            this.Room.PushAfter(tickAfter, UpdateAI); // 1초마다 AI 업데이트
        }
        int Update_Idle()
        {
            foreach (var player in Room.GetGameUsers().Values)
            {
                if (player is Player targetPlayer)
                {
                    float dx = targetPlayer.PosX - this.PosX;
                    float dz = targetPlayer.PosZ - this.PosZ;
                    float sqrDist = (dx * dx) + (dz * dz);
                    if (sqrDist <= 49.0f)
                    {
                        TargetId = targetPlayer.Id;
                        this.State = PlayerState.MOVE;
                        Console.WriteLine($"[AI] 몬스터 {this.Id}가 유저 {targetPlayer.Id} 발견! 추적 모드 돌입. {sqrDist}");
                        return 100;
                    }
                    else
                    {
                        Console.WriteLine($"{sqrDist}");
                    }
                }
            }
            return 1000; // 1초마다 주변 탐색
        }
        int Update_Move()
        {
            if (!Room.GetGameUsers().TryGetValue(TargetId, out var target) || target is not Player targetuser)
            {
                this.State = PlayerState.IDLE; // 타겟이 사라지면 다시 대기 상태로
                Console.WriteLine($"[AI] 몬스터 {this.Id}의 타겟이 사라짐. 대기 모드로 전환.");
                return 1000;
            }
            float dx = target.PosX - this.PosX;
            float dz = target.PosZ - this.PosZ;
            float sqrDist = (dx * dx) + (dz * dz);

            // 사거리(예: 2m) 이내로 좁혀졌다면?
            if (sqrDist <= 4.0f) // 2의 제곱
            {
                this.State = PlayerState.ATTACK;
                Console.WriteLine($"[AI] 몬스터 {this.Id} 사거리 진입. 공격 모드 돌입.");
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
            Console.WriteLine($"움직이는 중 현재 위치 {this.Id} : {this.PosX}, {this.PosZ}");
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
                this.State = PlayerState.IDLE;
                TargetId = 0;
                Console.WriteLine("타겟 상실");
                return 1000; // 타겟 상실. 1초 대기 모드로 복귀
            }

            // 2. 거리 재확인 (유저가 얄밉게 뒤로 빠져서 사거리 2m 밖으로 벗어났다면?)
            float dx = target.PosX - this.PosX;
            float dz = target.PosZ - this.PosZ;
            float sqrDist = (dx * dx) + (dz * dz);

            if (sqrDist > 4.0f) // 2의 제곱
            {
                this.State = PlayerState.MOVE;
                return 100; // 다시 0.1초 틱으로 쫓아간다 (Trace 상태 복귀)
            }

            // 3. 공격 쿨타임 검사 (핵심)
            long now = Environment.TickCount64;
            if (now < NextAttackTime) return 100; // 쿨타임 대기

            // 1. 타격 발생
            NextAttackTime = now + 2000; // 2초 쿨타임
            targetPlayer.Hp -= 10;
            RedisManager.SavePlayerHp(targetPlayer.Id, targetPlayer.Hp);
            Console.WriteLine($"[AI 타격] 몬스터 {this.Id} -> 유저 {targetPlayer.Id} (남은 HP: {targetPlayer.Hp})");

            // (선택) 클라이언트에 타격 애니메이션용 S_Attack 패킷 전송 로직...

            // 2. 사망 판정
            if (targetPlayer.Hp <= 0)
            {
                Console.WriteLine($"[사망] 유저 {targetPlayer.Id} 사망. 5초 뒤 부활 예약.");

                // 3. 상태 전이 및 브로드캐스트
                targetPlayer.State = PlayerState.DIE;

                // 주변 클라이언트에게 "얘 죽었어, 시체로 눕혀" 라고 방송
                //S_Die diePacket = new S_Die() { objectId = targetPlayer.Id };
                //Room.BroadcastAll(diePacket.Write());

                // 타겟이 죽었으니 몬스터는 다시 평화로운 탐색 상태로 복귀
                this.State = PlayerState.IDLE;
                this.TargetId = 0;

                // 4. 부활 스케줄링 (5초 뒤 실행)
                // 주의: Job Queue 안에서 실행되므로 Lock 없이 안전함.
                Room.PushAfter(5000, () =>
                {
                    // 이 람다식 안의 로직은 5초 뒤 Job Queue가 알아서 꺼내서 실행해 줌
                    targetPlayer.Hp = targetPlayer.MaxHp; // HP 풀 회복
                    targetPlayer.State = PlayerState.IDLE; // 상태 복구

                    // 시작 지점(마을) 좌표로 강제 텔레포트
                    targetPlayer.PosX = 2.5f;
                    targetPlayer.PosZ = 2.5f;

                    Console.WriteLine($"[부활] 유저 {targetPlayer.Id} 부활 완료. 마을로 귀환.");

                    // 클라이언트에게 부활 및 위치 이동 동기화 패킷 전송
                    S_Sync spawnPacket = new S_Sync();
                    // ... 패킷 세팅 (새 좌표, 풀 HP, Idle 상태 등) ...
                    spawnPacket.playerid = targetPlayer.Id;
                    spawnPacket.posX = targetPlayer.PosX;
                    spawnPacket.posY = targetPlayer.PosY;
                    spawnPacket.posZ = targetPlayer.PosZ;
                    spawnPacket.rotY = targetPlayer.RotY;
                    Room.Broadcast(spawnPacket.Write());
                    RedisManager.SavePlayerHp(targetPlayer.Id, targetPlayer.Hp); // DB에도 최신 HP 저장
                });

                return 1000; // 몬스터는 1초 탐색 틱으로 전환
            }

            return 100; // 유저가 안 죽었으면 계속 0.1초마다 쫓아다니며 쿨타임 검사
        }
    }
    
}
