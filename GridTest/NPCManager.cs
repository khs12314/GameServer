using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
    public static class NPCManager
    {
        public static NPCData GetNpcData(int id)
        {
            if (NpcDataManager.npcDataDic.TryGetValue(id, out var npcData))
            {
                return npcData;
            }
            else
            {
                return null;
            }
        }
    }
}
