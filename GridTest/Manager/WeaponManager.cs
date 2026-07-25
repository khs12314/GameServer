using GridTest.DataManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GridTest.Manager
{
    public static class WeaponManager
    {
        public static WeaponData GetWeaponData(int index)
        {
            return WeaponDataManager.WeaponDataDic[index];
        }
    }
}
