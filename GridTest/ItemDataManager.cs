using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
    public class ItemData
    {
        public int ItemId;
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
    public static class ItemDataManager
    {
        public static Dictionary<int, ItemData> itemDataDic = new Dictionary<int, ItemData>();
        public static Dictionary<int, List<DropData>> DropDataDic = new Dictionary<int, List<DropData>>();
        public static void LoadDropData(string filePath, string itemDatacsv,string DropDataCsv)
        {
            LoadItemData(filePath, itemDatacsv);
             filePath = Path.Combine(filePath, DropDataCsv);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] DropData.csv 파일이 없습니다.");
                return;
            }
            string fileContent = File.ReadAllText(filePath);
            ReadOnlySpan<char> span = fileContent.AsSpan();
            int headerIndex = span.IndexOf('\n');
            if (headerIndex != -1)
            {
                
                span = span.Slice(headerIndex +1);
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
                DropData dropData = new DropData();
                dropData.MonsterTemplateId = CsvParser.ParseNextInt(ref lineSpan);
                dropData.ItemId = CsvParser.ParseNextInt(ref lineSpan);
                dropData.Probability = CsvParser.ParseNextInt(ref lineSpan);
                if(!DropDataDic.ContainsKey(dropData.MonsterTemplateId))
                {
                    DropDataDic.Add(dropData.MonsterTemplateId, new List<DropData>());
                }
                DropDataDic[dropData.MonsterTemplateId].Add(dropData);
            }
        }
        private static void LoadItemData(string filePath, string itemDatacsv)
        {
            filePath = Path.Combine(filePath, itemDatacsv);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] DropData.csv 파일이 없습니다.");
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
                ItemData itemData = new ItemData();
                itemData.ItemId = CsvParser.ParseNextInt(ref lineSpan);
                itemData.Name = CsvParser.ParseNextString(ref lineSpan);
                itemData.Grade = CsvParser.ParseNextString(ref lineSpan);
                itemData.MaxStack = CsvParser.ParseNextInt(ref lineSpan);
                itemData.type = CsvParser.ParseNextByte(ref lineSpan);
                itemDataDic.Add(itemData.ItemId, itemData);
            }
            Console.WriteLine($"[시스템] 총 {itemDataDic.Count}개의 아이템 데이터가 메모리에 올라갔습니다.");
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
