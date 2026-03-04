using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
    public class GameMap
    {
        // 맵의 범위 (Global Coordinates)
        public int MinX { get; private set; }
        public int MaxX { get; private set; }
        public int MinY { get; private set; }
        public int MaxY { get; private set; }
        int count = 0;
        // 실제 사이즈
        public int SizeX { get; private set; }
        public int SizeY { get; private set; }

        // 충돌 데이터 (true: 벽/못감, false: 갈수있음)
        // 2차원 배열 [x, y]

        int[,] _collision;

        public int[,]? LoadMap(string folderPath, string mapName)
        {
            // 경로 결합: C:\...\Map + \ + Map_00_01 + .txt
            string path = Path.Combine(folderPath, mapName + ".txt");
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
                        Console.WriteLine($"[Error] Line Length Mismatch! Y:{y}, Len:{line.Length}");
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
                Console.WriteLine($"[System] Map Loaded Success: {mapName} ({SizeX}x{SizeY}) Range:({MinX}~{MaxX}, {MinY}~{MaxY})");
                Console.WriteLine(count);
                return _collision;

            }
            catch (Exception e)
            {
                Console.WriteLine($"[Error] Failed to Parse Map: {e.Message}");
                return null;
            }
        }
        const float PlayerRadius = 0.4f;
        public bool CanGo(float x, float z)
        {
            // 캐릭터의 4귀퉁이 좌표 계산
            // (MinX, MaxX가 아니라 현재 좌표 기준임에 주의)
            float minX = x - PlayerRadius;
            float maxX = x + PlayerRadius;
            float minZ = z - PlayerRadius;
            float maxZ = z + PlayerRadius;

            // 4개의 모서리가 모두 안전지대인지 확인
            // 하나라도 벽이면 false (못 감)
            if (!CanGo((int)minX, (int)minZ)) return false; // 좌하
            if (!CanGo((int)maxX, (int)minZ)) return false; // 우하
            if (!CanGo((int)minX, (int)maxZ)) return false; // 좌상
            if (!CanGo((int)maxX, (int)maxZ)) return false; // 우상

            return true; // 4곳 다 통과!
        }

        // 기존 int 버전 (단일 셀 체크)
        public bool CanGo(int x, int y)
        {
            // 1. 맵 범위 체크
            if (x < MinX || x >= MaxX || y < MinY || y >= MaxY)
                return false;

            // 2. 좌표 변환 (Global -> Local)
            int idxX = x - MinX;
            int idxY = y - MinY;

            // 3. 벽 체크
            if(_collision == null)
            {
                Console.WriteLine("맵이 로드되지 않았습니다.");
                return false;
            }
            if(_collision[idxX, idxY] == 1)
            {
                return false; // 벽임
            }
            else
            {
                return true; // 갈 수 있음
            }
            
        }



    }
}
