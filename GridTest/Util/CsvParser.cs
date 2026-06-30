using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Util
{
    public static class CsvParser
    {
        public static int ParseNextInt(ref ReadOnlySpan<char> lineSpan)
        {
            //다음 쉼표 위치 탐색
            int commaIndex = lineSpan.IndexOf(',');
            ReadOnlySpan<char> cell = commaIndex != -1 ? lineSpan.Slice(0, commaIndex) : lineSpan;

            // 원본 Span을 쉼표 다음으로 넘김
            if (commaIndex != -1) lineSpan = lineSpan.Slice(commaIndex + 1);

            return int.Parse(cell);
        }

        // [Span 헬퍼 함수 2] 문자열용 파싱
        public static string ParseNextString(ref ReadOnlySpan<char> lineSpan)
        {
            int commaIndex = lineSpan.IndexOf(',');
            ReadOnlySpan<char> cell = commaIndex != -1 ? lineSpan.Slice(0, commaIndex) : lineSpan;
            if (commaIndex != -1) lineSpan = lineSpan.Slice(commaIndex + 1);
            return cell.ToString();
        }
        public static float ParseNextFloat(ref ReadOnlySpan<char> lineSpan)
        {
            int commaIndex = lineSpan.IndexOf(',');
            ReadOnlySpan<char> cell = commaIndex != -1 ? lineSpan.Slice(0, commaIndex) : lineSpan;
            if (commaIndex != -1) lineSpan = lineSpan.Slice(commaIndex + 1);
            return float.Parse(cell);
        }
        public static byte ParseNextByte(ref ReadOnlySpan<char> lineSpan)
        {
            int commaIndex = lineSpan.IndexOf(',');
            ReadOnlySpan<char> cell = commaIndex != -1 ? lineSpan.Slice(0, commaIndex) : lineSpan;
            if (commaIndex != -1) lineSpan = lineSpan.Slice(commaIndex + 1);
            return byte.Parse(cell);

        }
        public static long ParseNextLong(ref ReadOnlySpan<char> lineSpan) 
        {
            int commaindex = lineSpan.IndexOf(",");
            ReadOnlySpan<char> cell = commaindex != -1 ? lineSpan.Slice(0, commaindex) : lineSpan;
            if (commaindex != -1) lineSpan = lineSpan.Slice(commaindex + 1);
            return long.Parse(cell);

        }

    }
}
