using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.DataManagers
{
   
    public static class DataManager
    {
              
        public static void LoadAllData()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dataDir = Path.Combine(baseDir, "Data");
            QuestDataManager.LoadQuestData(dataDir, "Quest_Data.csv");
            NpcDataManager.LoadNpcData(dataDir, "NPC_Data.csv");
            LevelDataManager.LoadLevelData(dataDir, "LevelTemplate.csv");
            MonsterAniDataManager.LoadAniData(dataDir, "Monster_Ani.csv");
            SkillDataManager.LoadSkillData(dataDir, "MMO_Data.csv");
            ConsumeItemDataManager.LoadConsumeItemData(dataDir, "Item_Consumable.csv");
            ItemDataManager.LoadDropData(dataDir, "MMO_item.csv", "MMO_Drop.csv");
            MonsterDataManager.LoadMonsterData(dataDir, "MMO_Monster.csv");
            MapDataManager.LoadAllMaps();
            MapDataManager.LoadMapData(dataDir, "GameMapData.csv");
            WeaponDataManager.LoadWeaponData(dataDir, "Item_Weapon.csv");
        }      
    }
}
