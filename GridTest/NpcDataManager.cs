using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
    public class NPCData
    {
        public int NpcId;
        public string Name;
        public float X;
        public float Z;
    }
    public static class NpcDataManager
    {
        public static Dictionary<int, NPCData> npcDataDic = new Dictionary<int, NPCData>();
        public static void LoadNpcData(string filePath, string NpcDatacsv)
        {
            filePath = Path.Combine(filePath, NpcDatacsv);
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
                NPCData npcData = new NPCData();
                npcData.NpcId = CsvParser.ParseNextInt(ref lineSpan);
                npcData.Name = CsvParser.ParseNextString(ref lineSpan);
                npcData.X = CsvParser.ParseNextFloat(ref lineSpan);
                npcData.Z = CsvParser.ParseNextFloat(ref lineSpan);
                npcDataDic.Add(npcData.NpcId, npcData);

            }
            Console.WriteLine($"[시스템] 총 {npcDataDic.Count}개의 NPC 데이터가 메모리에 올라갔습니다.");
        }
       

    }
}
