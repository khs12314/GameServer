using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GridTest
{
    
    public static class ItemDropManager
    {
        public static int GetRandomLoot(int monsterId)
        {
            // 1. 해당 몬스터의 드랍 리스트를 가져온다.
            var lootList = ItemDataManager.GetDropData(monsterId);
            int rand = Random.Shared.Next(0, 10000); // 시드 초기화
                                                     // 2. 0 ~ 9999 사이의 난수 하나 발생

            if(lootList == null || lootList.Count == 0)
            {
                return -1; // 드랍 없음
            }
            int cumulative = 0;
            foreach (var loot in lootList)
            {
                cumulative += loot.Probability;
                if (rand < cumulative)
                {
                    Console.WriteLine($"{rand}, 당첨된 아이템{loot.ItemId}");
                    return loot.ItemId; // 당첨!
                }
            }

            return -1; // 드랍 없음
        }
    }
}
