using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
   
    
    
   
    
    public class DataManager
    {
        
        
        
       
       
        public static void LoadAllData()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dataDir = Path.Combine(baseDir, "Data");
            QuestDataManager.LoadQuestData(dataDir, "Quest_Data.csv");
            NpcDataManager.LoadNpcData(dataDir, "NPC_Data.csv");

            MonsterAniDataManager.LoadAniData(dataDir, "Monster_Ani.csv");
            SkillDataManager.LoadSkillData(dataDir, "MMO_Data.csv");
            ItemDataManager.LoadDropData(dataDir, "MMO_item.csv", "MMO_Drop.csv");
        }

        

     
        
        
        
        
       
        
        
    }
}
