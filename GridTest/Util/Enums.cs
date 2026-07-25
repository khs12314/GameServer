using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Util
{
    public enum MoveResult
    {
        Success = 0,      // 정상 이동 가능
        Obstacle = 1,     // 장애물/벽에 막힘 (위치 보정 필요)
        OutOfBounds = 2,   // 방 경계선 이탈 (마이그레이션 필요)
        MapNull=3,
    }
    public enum SwapResult { 
        Failed, 
        Swapped, 
        MergedPartial, 
        MergedComplete 
    }
    public enum ItemType : byte
    {
        None = 0,
        Equipment = 1,//장비
        Consumables = 2,//소비템
        Miscellaneous = 3,//기타템
    }
    public enum PlayerState : byte
    {
        IDLE = 0,
        ATTACK = 1,
        MOVE = 2,
        HIT = 3,
        DIE = 4
    }
    public enum GameObjectType : long
    {

        Player = 0,
        Monster = 1,
        Projectile = 2,
        Item = 3,
        Temporary = 4,
    }
    public enum ConsumeItemType : byte
    {
        Hp = 0,
        Mp = 1,
        Both = 2,
    }
    [Flags]
    public enum BorderDirection : byte
    {
        None = 0,
        Up = 1 << 0, // 1
        Down = 1 << 1, // 2
        Left = 1 << 2, // 4
        Right = 1 << 3  // 8
    }
}
