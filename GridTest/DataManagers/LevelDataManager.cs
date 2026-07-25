using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.DataManagers
{
    public class LevelData
    {
        public int Level;
        public long RequiredExp; // [핵심] 이 레벨을 탈출하기 위해 필요한 경험치 통
        public int MaxHp;
        public int MaxMp;
        
    }
    public static class LevelDataManager
    {
        public static int MaxLevel;
        public static List<LevelData> _levelDatas = new List<LevelData>();
        public static void LoadLevelData(string filePath, string LevelDatacsv)
        {
            _levelDatas.Add(new LevelData { Level = 0, RequiredExp = 0, MaxHp = 0, MaxMp = 0 });
            filePath = Path.Combine(filePath, LevelDatacsv);
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
                LevelData levelData = new LevelData();
                levelData.Level = CsvParser.ParseNextInt(ref lineSpan);
                levelData.RequiredExp = CsvParser.ParseNextLong(ref lineSpan);
                levelData.MaxHp = CsvParser.ParseNextInt(ref lineSpan);
                levelData.MaxMp = CsvParser.ParseNextInt(ref lineSpan);
                _levelDatas.Add(levelData);

            }

            Console.WriteLine($"[시스템] 총 {_levelDatas.Count}개의 레벨 데이터가 메모리에 올라갔습니다.");
            MaxLevel = _levelDatas.Count;
        }
    }
}
