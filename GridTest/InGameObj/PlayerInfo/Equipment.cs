using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.InGameObj.PlayerInfo
{
    public class Equipment
    {
        Item? Weapon;
        public void SetWeapon(Item Weapon)
        {
            this.Weapon = Weapon;
        }
        public void Clear()
        {
            Weapon = null;  
        }
        public Item? GetWeapon() { return Weapon; }
    }
}
