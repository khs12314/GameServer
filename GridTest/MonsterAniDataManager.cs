using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
    public class MonsterAniData
    {

        public int AniId;
        public string Name;
        public int MaxHp;
        public float AttackRange;
        public int AtkWaitMs;
        public int AtkTotalMs;
    }
    public static class MonsterAniDataManager
    {
        public static Dictionary<int, MonsterAniData> aniDataDic = new Dictionary<int, MonsterAniData>();
        public static void LoadAniData(string filePath, string AniDatacsv)
        {

            filePath = Path.Combine(filePath, AniDatacsv);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] Monster_Ani.csv 파일이 없습니다.");
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
                if (lineSpan.IsEmpty)
                {
                    Console.WriteLine("빈 줄");
                    continue;
                }
                MonsterAniData aniData = new MonsterAniData();
                aniData.AniId = CsvParser.ParseNextInt(ref lineSpan);
                aniData.Name = CsvParser.ParseNextString(ref lineSpan);
                aniData.MaxHp = CsvParser.ParseNextInt(ref lineSpan);
                aniData.AttackRange = CsvParser.ParseNextFloat(ref lineSpan);
                aniData.AtkWaitMs = CsvParser.ParseNextInt(ref lineSpan);
                aniData.AtkTotalMs = CsvParser.ParseNextInt(ref lineSpan);
                if(aniDataDic.ContainsKey(aniData.AniId))
                {
                    Console.WriteLine($"[error] AniId {aniData.AniId}가 이미 존재합니다. 중복된 AniId는 허용되지 않습니다.");
                    Console.WriteLine($"{aniData.AniId} : {aniData.Name},{aniData.MaxHp},{aniData.AttackRange},{aniData.AtkWaitMs},{aniData.AtkTotalMs}");
                    string asd = CsvParser.ParseNextString(ref lineSpan);
                    Console.WriteLine($"{asd}");
                    Console.WriteLine($"{span.Length}");
                    return;
                }
                aniDataDic.Add(aniData.AniId, aniData);
                Console.WriteLine($"{aniData.AniId} 등록 완료");
            }
            Console.WriteLine($"[시스템] 총 {aniDataDic.Count}개의 몬스터 애니메이션 데이터가 메모리에 올라갔습니다.");
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
    }
}
