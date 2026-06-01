using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
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
    public enum GameObjectType
    {

        Player = 0,
        Monster = 1,
        Projectile = 2,
        Item = 3,
    }
}
