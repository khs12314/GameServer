using GridTest.Util;

public class ConsumeItemData
{
    public int ItemID { get; set; }
    public ConsumeItemType ConsumeItemType { get; set; }
    public int Healamount { get; set; }

}
public static class ConsumeItemDataManager
{
    public static Dictionary<int, ConsumeItemData> ConsumeItemDic = new Dictionary<int, ConsumeItemData>();
    public static void LoadConsumeItemData(string filePath, string ConsumeItemDatacsv)
    {
        filePath = Path.Combine(filePath, ConsumeItemDatacsv);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("[error] ConsumeItemData 파일이 없습니다");
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
            int lineEnd = span.IndexOf("\n");
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
            ConsumeItemData consumeItemData = new ConsumeItemData();
            consumeItemData.ItemID = CsvParser.ParseNextInt(ref lineSpan);
            consumeItemData.ConsumeItemType = (ConsumeItemType)CsvParser.ParseNextByte(ref lineSpan);
            consumeItemData.Healamount = CsvParser.ParseNextInt(ref lineSpan);
            if (ConsumeItemDic.ContainsKey(consumeItemData.ItemID))
            {
                Console.WriteLine($"[error] consumeItemId {consumeItemData.ItemID}가 이미 존재");
                return;
            }
            ConsumeItemDic.Add(consumeItemData.ItemID, consumeItemData);
        }
        Console.WriteLine($"[시스템] 총 {ConsumeItemDic.Count} 소비 아이템 등록 완료");

    }
    public static ConsumeItemData GetConsumeItemData(int itemID)
    {
        if (ConsumeItemDic.TryGetValue(itemID, out ConsumeItemData item))
        {
            return item;
        }
        else
        {
            return null;
        }
    }
}