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
        public float Hp { get; set; }
        public float MaxHp { get; set; }
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
        public long TargetId { get; set; } = 0;
        public Monster()
        {

            this.State = PlayerState.IDLE; // 태어날 땐 무조건 대기 상태
            Room = null;
        }
        void Update_Attack()
        {

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
                    if(sqrDist <= 25.0f)
                    {
                        TargetId = targetPlayer.Id;
                        this.State = PlayerState.MOVE;
                        Console.WriteLine($"[AI] 몬스터 {this.Id}가 유저 {targetPlayer.Id} 발견! 추적 모드 돌입.");
                        return 100;
                    }
                }
            }
            return 1000; // 1초마다 주변 탐색
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
                    break;
                case PlayerState.MOVE:
                    break;
                case PlayerState.HIT:
                    break;
            }
            Console.WriteLine($"[AI Tick] 몬스터 {this.Id} 생존 : {this.Id}");
            Console.WriteLine($"현재 상태 {(PlayerState)this.State}");
            this.Room.PushAfter(tickAfter, UpdateAI); // 1초마다 AI 업데이트
        }
    }
}
