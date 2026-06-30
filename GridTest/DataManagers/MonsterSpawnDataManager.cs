using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.DataManagers
{
    public class MonsterSpawnData
    {
        public int TemplateID;
        public int Roomid;
        public float PosX;
        public float PosY;
        public float PosZ;
        public int SpawnTime;

    }
    public static class MonsterSpawnDataManager
    {
        public static List<MonsterSpawnData> spawnList = new List<MonsterSpawnData>();
        public static void LoadSpawnData(string filePath, string SpawnDatacsv)
        {
            filePath = Path.Combine(filePath, SpawnDatacsv);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] NpcData.csv 파일이 없습니다.");
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
                MonsterSpawnData monsterSpawnData = new MonsterSpawnData();
                monsterSpawnData.TemplateID = CsvParser.ParseNextInt(ref lineSpan);
                monsterSpawnData.Roomid = CsvParser.ParseNextInt(ref lineSpan);
                monsterSpawnData.PosX = CsvParser.ParseNextFloat(ref lineSpan);
                monsterSpawnData.PosY = CsvParser.ParseNextFloat(ref lineSpan);
                monsterSpawnData.PosZ = CsvParser.ParseNextFloat(ref lineSpan);
                monsterSpawnData.SpawnTime = CsvParser.ParseNextInt(ref lineSpan);
                spawnList.Add(monsterSpawnData);
            }
            Console.WriteLine($"{spawnList.Count}개의 스폰 데이터가 올라갔습니다.");

        }
    }
}
