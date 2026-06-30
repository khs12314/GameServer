using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.DataManagers
{
    public class MonsterData
    {

        public int TemplateId;
        public int MaxHp;
        public long Exp;

    }
    public static class MonsterDataManager
    {
        public static Dictionary<int, MonsterData> _monsterData = new Dictionary<int, MonsterData>();
        public static void LoadMonsterData(string filePath, string MonsterDatacsv)
        {
            filePath = Path.Combine(filePath, MonsterDatacsv);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] Monster.csv 파일이 없습니다.");
                return;
            }
            string fileContent = File.ReadAllText(filePath);
            ReadOnlySpan<char> span = fileContent.AsSpan();
            int headerIndex = span.IndexOf('\n');
            if (headerIndex != -1)
            {
                span = span.Slice(headerIndex + 1);
            }
            while (!span.IsEmpty)
            {
                int lineEnd = span.IndexOf('\n');
                ReadOnlySpan<char> lineSpan = lineEnd != -1 ? span.Slice(0, lineEnd) : span;
                span = lineEnd != -1 ? span.Slice(lineEnd + 1) : ReadOnlySpan<char>.Empty;

                if (lineSpan.Length > 0 && lineSpan[^1] == '\r')
                {
                    lineSpan = lineSpan.Slice(0, lineSpan.Length - 1);
                }
                if (lineSpan.IsEmpty) continue;
                MonsterData monsterdata = new MonsterData();
                monsterdata.TemplateId = CsvParser.ParseNextInt(ref lineSpan);
                monsterdata.MaxHp = CsvParser.ParseNextInt(ref lineSpan);
                monsterdata.Exp = CsvParser.ParseNextLong(ref lineSpan);
                _monsterData.Add(monsterdata.TemplateId, monsterdata);
            }
            Console.WriteLine($"총 {_monsterData.Count}개의 몬스터 정보가 올라갔습니다.");
        }
        public static MonsterData GetMonsterData(int templateid)
        {
            return _monsterData[templateid];
        }
    }
}
