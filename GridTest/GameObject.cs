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

        public PlayerState State { get; set; }
    }
    public class  Player : GameObject
    {
        public GameUser_Session Session { get; set; }
    }
    public class  Monster : GameObject
    {
        public int MonsterID { get; set; }
    }
}
