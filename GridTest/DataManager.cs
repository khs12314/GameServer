using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
    public class SkillData
    {
        public int Id;
        public string Name;
        public int SkillType;
        public int Damage;
        public float Range;
        public int Cooldown;
        public float Speed;
        public float Angle;
    }
    public class MonsterAniData
    {

        public int TemplateId;
        public string Name;
        public int MaxHp;
        public float AttackRange;
        public int AtkWaitMs;
        public int AtkTotalMs;
    }
    public class MonsterData
    {
        public int Id;
        public int TemplateId;
        public int Hp;
        public float X;
        public float Y;
    }
    public class ItemData
    {
        public int Id;
        public string Name;
        public string Grade;
        public int MaxStack;
        public byte type;
        
    }
    public class DropData
    {
        public int MonsterTemplateId;
        public int ItemId;
        public int Probability;
    }
    public class DataManager
    {
        public static Dictionary<int, SkillData> SkillDataDic = new Dictionary<int, SkillData>();
        public static Dictionary<int, MonsterAniData> aniDataDic = new Dictionary<int, MonsterAniData>();
        public static Dictionary<int, ItemData> itemDataDic = new Dictionary<int, ItemData>();
        public static Dictionary<int, List<DropData>> DropDataDic = new Dictionary<int, List<DropData>>();
        public static void LoadAllData()
        {
            LoadAniData();
            LoadSkillData();
            LoadDropData();
        }

        public static void LoadAniData()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(baseDir, "Data");
            string DataFileName = "Monster_Ani";
            string filePath = Path.Combine(folderPath, DataFileName + ".csv");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] AniData.csv 파일이 없습니다.");
                return;
            }
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                string[] row = line.Split(',');
                MonsterAniData aniData = new MonsterAniData();
                aniData.TemplateId = int.Parse(row[0]);
                aniData.Name = row[1];
                aniData.MaxHp = int.Parse(row[2]);
                aniData.AttackRange = float.Parse(row[3]);
                aniData.AtkWaitMs = int.Parse(row[4]);
                aniData.AtkTotalMs = int.Parse(row[5]);
                aniDataDic.Add(aniData.TemplateId, aniData);
                Console.WriteLine($"[Data Load] {aniData.TemplateId}번 몬스터, '{aniData.Name}로드'");
            }
            Console.WriteLine($"[시스템] 총 {aniDataDic.Count}개의 몬스터 애니메이션 데이터가 메모리에 올라갔습니다.");
        }
        public static void LoadDropData()
        {
            LoadItemData();
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(baseDir, "Data");
            string DataFileName = "MMO_Drop";
            string filePath = Path.Combine(folderPath, DataFileName + ".csv");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] DropData.csv 파일이 없습니다.");
                return;
            }
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                string[] row = line.Split(',');
                int monsterId = int.Parse(row[0]);
                var loot = new DropData()
                {
                    ItemId = int.Parse(row[1]),
                    Probability = int.Parse(row[2]),
                };
                if (!DropDataDic.ContainsKey(monsterId))
                {
                    DropDataDic.Add(monsterId, new List<DropData>());
                }
                DropDataDic[monsterId].Add(loot);
                Console.WriteLine($"[Data Load] 몬스터 템플릿 {monsterId}번, 아이템 {loot.ItemId}번 드롭 확률 {loot.Probability}%");
            }
        }
        private static void LoadItemData()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(baseDir, "Data");
            string DataFileName = "MMO_Item";
            string filePath = Path.Combine(folderPath, DataFileName + ".csv");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] ItemData.csv 파일이 없습니다.");
                return;
            }
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                string[] row = line.Split(',');
                ItemData itemData = new ItemData();
                itemData.Id = int.Parse(row[0]);
                itemData.Name = row[1];
                itemData.Grade = row[2];
                itemData.MaxStack = int.Parse(row[3]);
                itemData.type = byte.Parse(row[4]);
                itemDataDic.Add(itemData.Id, itemData);
                Console.WriteLine($"[Data Load] {itemData.Id}번 아이템, '{itemData.Name}로드'");
            }
            Console.WriteLine($"[시스템] 총 {itemDataDic.Count}개의 아이템 데이터가 메모리에 올라갔습니다.");
        }
        public static void LoadSkillData()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(baseDir, "Data");
            string DataFileName = "MMO_Data";
            string filePath = Path.Combine(folderPath, DataFileName + ".csv");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] SkillData.csv 파일이 없습니다.");
                return;
            }
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                string[] row = line.Split(',');
                SkillData skill = new SkillData();
                skill.Id = int.Parse(row[0]);
                skill.Name = row[1];
                skill.SkillType = int.Parse(row[2]);
                skill.Damage = int.Parse(row[3]);
                skill.Range = float.Parse(row[4]);
                skill.Cooldown = int.Parse(row[5]);
                skill.Speed = float.Parse(row[6]);
                skill.Angle = float.Parse(row[7]);
                SkillDataDic.Add(skill.Id, skill);

                Console.WriteLine($"[Data Load] {skill.Id}번 스킬, '{skill.Name}로드'");
            }
            Console.WriteLine($"[시스템] 총 {SkillDataDic.Count}개의 스킬 데이터가 메모리에 올라갔습니다.");
        }
        public static MonsterAniData GetAniData(int templateId)
        {
            if (aniDataDic.TryGetValue(templateId, out var aniData))
            {
                return aniData;
            }
            else
            {
                return null;
            }
        }
        public static SkillData GetSkillData(int skillId)
        {

            if (SkillDataDic.TryGetValue(skillId, out var skill))
            {
                return skill;
            }
            else
            {
                return null;
            }
        }
        public static ItemData GetItemData(int itemId)
        {
            if (itemDataDic.TryGetValue(itemId, out var itemData))
            {
                return itemData;
            }
            else
            {
                return null;
            }
        }
        public static List<DropData> GetDropData(int monsterTemplateId)
        {
            if (DropDataDic.TryGetValue(monsterTemplateId, out var dropList))
            {
                return dropList;
            }
            else
            {
                return null;
            }
        }
    }
}
