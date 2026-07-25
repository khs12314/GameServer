using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.DataManagers
{
    public class WeaponData
    {
        public int Itemid;
        public int ReqLevel;
        public int AttackPower;
    }
    public static class WeaponDataManager
    {

        public static Dictionary<int, WeaponData> WeaponDataDic = new Dictionary<int, WeaponData>();
        public static void LoadWeaponData(string filePath, string WeaponDatacsv)
        {
            filePath = Path.Combine(filePath, WeaponDatacsv);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[error] Item_Weapon.csv 파일이 없습니다.");
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
                WeaponData weaponData = new WeaponData();
                weaponData.Itemid = CsvParser.ParseNextInt(ref lineSpan);
                weaponData.ReqLevel = CsvParser.ParseNextInt(ref lineSpan);
                weaponData.AttackPower = CsvParser.ParseNextInt(ref lineSpan);
                WeaponDataDic.Add(weaponData.Itemid, weaponData);

            }
            Console.WriteLine($"[시스템] 총 {WeaponDataDic.Count}개의 무기 데이터가 메모리에 올라갔습니다.");
        }

    }
}