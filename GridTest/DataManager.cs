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
    public class DataManager
    {
        public static Dictionary<int, SkillData> SkillDataDic = new Dictionary<int, SkillData>();
        public static void LoadData()　
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
            for(int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if(string.IsNullOrEmpty(line))
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

        public static SkillData? GetSkillData(int skillId)
        {
            return SkillDataDic.TryGetValue(skillId, out var skill) ? skill : null;
        }
    }
}
