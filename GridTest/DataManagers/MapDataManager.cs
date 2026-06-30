using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.DataManagers
{
    public class MapData()
    {
       public int RoomId;
        public string MapFileName;
        public int MinX, MaxX, MinY, MaxY;
    }
    public static class MapDataManager
    {
        // Key: 맵 이름 (또는 ID), Value: 충돌 2D 배열 (읽기 전용)
        public static Dictionary<int, int[,]> MapDict = new Dictionary<int, int[,]>();
        public static Dictionary<int, MapData> MapDataDic = new Dictionary<int, MapData>();
        public static void LoadMapData(string filePath, string MapDatacsv)
        {
            filePath = Path.Combine(filePath, MapDatacsv);
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
                MapData mapData = new MapData();
                mapData.RoomId = CsvParser.ParseNextInt(ref lineSpan);
                mapData.MapFileName = CsvParser.ParseNextString(ref lineSpan);
                mapData.MinX = CsvParser.ParseNextInt(ref lineSpan);
                mapData.MaxX = CsvParser.ParseNextInt(ref lineSpan);
                mapData.MinY = CsvParser.ParseNextInt(ref lineSpan);
                mapData.MaxY = CsvParser.ParseNextInt(ref lineSpan);
                MapDataDic.Add(mapData.RoomId, mapData); 
            }
        }
        public static MapData GetMapData(int Roomid)
        {
            return MapDataDic[Roomid];
        }
        public static void LoadAllMaps()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string mapDir = Path.Combine(baseDir, "Map");

            if (!Directory.Exists(mapDir))
            {
                Console.WriteLine($"[Error] Map 폴더가 존재하지 않습니다: {mapDir}");
                return;
            }
            int count = 0;
            // Map 폴더 안의 모든 .txt 파일을 가져온다
            string[] mapFiles = Directory.GetFiles(mapDir, "*.txt");

            foreach (string filePath in mapFiles)
            {
                // 경로에서 파일 이름만 추출 (예: "C:\...\Map_00_01.txt" -> "Map_00_01")
                string mapName = Path.GetFileNameWithoutExtension(filePath);

                // 자네가 짠 LoadMap 로직을 여기에 맞춰 수정해서 호출
                int[,] collisionData = ParseMapFile(filePath, mapName);

                if (collisionData != null)
                {
                    //추후 숫자 추출 후 정상적으로 만들 예정
                    MapDict.Add(count+1, collisionData);
                    Console.WriteLine($"{count}개의 맵 존재");
                    Console.WriteLine($"[Map Loaded] {mapName} 맵 캐싱 완료.");
                    count++;
                }
                else
                {
                    Console.WriteLine("맵없음");
                }
            }
        }
        private static int[,] ParseMapFile(string path, string mapName)
        {
            int MinX;
            int MaxX;
            int MinY;
            int MaxY;
            int count = 0;
            // 실제 사이즈
            int SizeX;
            int SizeY;

            // 충돌 데이터 (true: 벽/못감, false: 갈수있음)
            // 2차원 배열 [x, y]

            int[,] _collision;

            // 1. 파일 존재 여부 확인
            if (File.Exists(path) == false)
            {
                Console.WriteLine($"[Error] Map File Not Found: {path}");
                return null;
            }

            // 2. 모든 줄 읽기
            string[] lines = File.ReadAllLines(path);

            try
            {
                // 3. 헤더 파싱 (처음 4줄)
                MinX = int.Parse(lines[0]);
                MaxX = int.Parse(lines[1]);
                MinY = int.Parse(lines[2]);
                MaxY = int.Parse(lines[3]);

                // 4. 사이즈 계산 (50 - 0 = 50)
                SizeX = MaxX - MinX;
                SizeY = MaxY - MinY;

                // 5. 배열 할당
                _collision = new int[SizeX, SizeY];

                // 6. 데이터 파싱 (5번째 줄부터 맵 데이터)
                // lines[4]가 y=0(혹은 y=Max) 줄에 해당.
                // 보통 텍스트 파일은 위에서 아래로 읽으므로 y 루프를 0부터 돌린다.
                int lineIndex = 4;

                // y는 위(49)에서 아래(0)로 내려간다 (맵 좌표 기준)
                for (int y = SizeY - 1; y >= 0; y--)
                {
                    // [수정] y를 더하는 게 아니라, 순차적으로 증가하는 lineIndex를 쓴다.
                    // y=49일 때 lines[4]를 읽게 됨.
                    string line = lines[lineIndex];

                    // 다음 줄을 읽기 위해 인덱스 증가
                    lineIndex++;

                    if (line.Length != SizeX)
                    {
                        //Console.WriteLine($"[Error] Line Length Mismatch! Y:{y}, Len:{line.Length}");
                        continue;
                    }

                    // x는 좌우 반전 이슈 없으면 편한 대로 해도 되지만, 
                    // 보통 0부터 읽는 게 직관적이다. (네 코드는 x를 거꾸로 돌려도 로직은 맞음)
                    for (int x = 0; x < SizeX; x++)
                    {
                        char c = line[x];
                        if (c == '1')
                        {
                            _collision[x, y] = 1; // bool 배열이면 true
                                                  // 디버그: (0, 0)이 찍히는지 확인해라
                            Console.WriteLine($"{x},{y}");
                            count++;
                        }
                        else
                        {
                            _collision[x, y] = 0; // bool 배열이면 false
                        }
                    }
                }
                //Console.WriteLine($"[Sㅁㄴㅇㅁㄴㅇㄴㅁystem] Map Loaded Success: {mapName} ({SizeX}x{SizeY}) Range:({MinX}~{MaxX}, {MinY}~{MaxY})");
                Console.WriteLine(count);
                return _collision;

            }
            catch (Exception e)
            {
                //Console.WriteLine($"[Error] Failed to Parse Map: {e.Message}");
                return null;
            }
        }
    }
}
