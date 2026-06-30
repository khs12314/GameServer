using GridTest.DataManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Manager
{
    public static class LevelManager
    {
        public static LevelData GetLevelData(int index)
        {
            return LevelDataManager._levelDatas[index];
        }
    }
}
