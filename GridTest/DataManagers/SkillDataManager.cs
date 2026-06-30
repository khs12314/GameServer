using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.DataManagers
{
    public class SkillData
    {
        public int SkillId;
        public string Name;
        public int SkillType;
        public int Damage;
        public float Range;
        public int Cooldown;
        public float Speed;
        public float Angle;
    }
    public static class SkillDataManager
    {
        public static Dictionary<int, SkillData> SkillDataDic = new Dictionary<int, SkillData>();
        public static void LoadSkillData(string filePath, string SkillDatacsv)
        {
        
             filePath = Path.Combine(filePath, SkillDatacsv);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] SkillData.csv 파일이 없습니다.");
                return;
            }
            string fileContent = File.ReadAllText(filePath);
            ReadOnlySpan<char> span = fileContent.AsSpan();
            int headerIndex = span.IndexOf('\n');
            if (headerIndex != -1)
            {
                // 헤더 다음 글자부터 끝까지로 시야(Span)를 좁힘
                span = span.Slice(headerIndex + 1);
            }
           
            while(!span.IsEmpty)
            {
                int lineEnd = span.IndexOf('\n');
                ReadOnlySpan<char> lineSpan = lineEnd != -1 ? span.Slice(0, lineEnd) : span;

                span = lineEnd != -1 ? span.Slice(lineEnd + 1) : ReadOnlySpan<char>.Empty;

                if (lineSpan.Length > 0 && lineSpan[^1] == '\r')
                {
                    lineSpan = lineSpan.Slice(0, lineSpan.Length - 1);
                }
                if(lineSpan.IsEmpty) continue;
                SkillData skillData = new SkillData();
                skillData.SkillId = CsvParser.ParseNextInt(ref lineSpan);
                skillData.Name = CsvParser.ParseNextString(ref lineSpan);
                skillData.SkillType = CsvParser.ParseNextInt(ref lineSpan);
                skillData.Damage = CsvParser.ParseNextInt(ref lineSpan);
                skillData.Range = CsvParser.ParseNextFloat(ref lineSpan);
                skillData.Cooldown = CsvParser.ParseNextInt(ref lineSpan);
                skillData.Speed = CsvParser.ParseNextFloat(ref lineSpan);
                skillData.Angle = CsvParser.ParseNextFloat(ref lineSpan);
                SkillDataDic.Add(skillData.SkillId, skillData);
            }
            Console.WriteLine($"[시스템] 총 {SkillDataDic.Count}개의 스킬 데이터가 메모리에 올라갔습니다.");
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
    }

}
