using SnakeServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace GridTest
{
    public class GridManager
    {
        // _cells[row(y), column(x)]
        List<GameObject>[,] _cells;
        int _cellSize;      // 한 셀의 월드 단위 길이
        int _gridSizeX;     // 칸(열) 개수
        int _gridSizeY;     // 칸(행) 개수
        readonly object _lock = new object();
        
        public GridManager(int mapSize, int cellSize)
        {
            if (cellSize <= 0) throw new ArgumentOutOfRangeException(nameof(cellSize));
            _cellSize = cellSize;

            // 전체 맵 길이를 셀 크기로 나눈 올림 -> 셀 개수
            _gridSizeX = Math.Max(1, (mapSize + cellSize - 1) / cellSize);
            _gridSizeY = Math.Max(1, (mapSize + cellSize - 1) / cellSize);

            _cells = new List<GameObject>[_gridSizeY, _gridSizeX];
            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x = 0; x < _gridSizeX; x++)
                {
                    _cells[y, x] = new List<GameObject>();
                }
            }
        }

        public void Add(GameObject gameobject)
        {
            if (gameobject == null) throw new ArgumentNullException(nameof(gameobject));

            int x = (int)(gameobject.PosX / _cellSize);
            int y = (int)(gameobject.PosZ / _cellSize);

            // 범위 검사
            if (x < 0 || x >= _gridSizeX || y < 0 || y >= _gridSizeY) return;

            gameobject.CellX = x;
            gameobject.CellY = y;

            lock (_lock)
            {
                var list = _cells[y, x];
                if (list == null)
                {
                    list = new List<GameObject>();
                    _cells[y, x] = list;
                }
                list.Add(gameobject);
            }
        }

        public void Remove(GameObject gameobject)
        {
            if (gameobject == null) return;

            int x = gameobject.CellX;
            int y = gameobject.CellY;

            // 방어: 유효한 캐시 인덱스인지 확인
            if (x < 0 || x >= _gridSizeX || y < 0 || y >= _gridSizeY) return;

            lock (_lock)
            {
                _cells[y, x]?.Remove(gameobject);
            }
        }

        public void Move(GameObject gameobject, GameObject.Pos newPos)
        {
            if (gameobject == null) return;

            int newX = (int)(newPos.x / _cellSize);
            int newY = (int)(newPos.z / _cellSize);

            // 맵 밖이면 무시
            if (newX < 0 || newX >= _gridSizeX || newY < 0 || newY >= _gridSizeY) return;

            int oldX = gameobject.CellX;
            int oldY = gameobject.CellY;

            // 같은 셀이면 아무것도 안 함
            if (oldX == newX && oldY == newY) return;

            lock (_lock)
            {
                // 기존 셀에서 제거 (방어적 검사)
                if (oldX >= 0 && oldX < _gridSizeX && oldY >= 0 && oldY < _gridSizeY)
                {
                    _cells[oldY, oldX]?.Remove(gameobject);
                }

                // 새 셀에 추가
                var list = _cells[newY, newX];
                if (list == null)
                {
                    list = new List<GameObject>();
                    _cells[newY, newX] = list;
                }
                list.Add(gameobject);

                // 캐시 갱신
                gameobject.CellX = newX;
                gameobject.CellY = newY;
            }

            //Console.WriteLine($"현재 aoi : {session.GameUser_Session_Info.CellX},{session.GameUser_Session_Info.CellY}");
        }
        
        public void GetNeighbors(GameObject gameobject, List<GameObject> outBuffer)
        {
            outBuffer.Clear();
          
            if (gameobject == null) return;

            int centerX = gameobject.CellX;
            int centerY = gameobject.CellY;

            // 방어: 캐시가 유효한지 확인
            if (centerX < 0 || centerX >= _gridSizeX || centerY < 0 || centerY >= _gridSizeY) return;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nextX = centerX + dx;
                    int nextY = centerY + dy;

                    if (nextX < 0 || nextX >= _gridSizeX || nextY < 0 || nextY >= _gridSizeY) continue;

                    var list = _cells[nextY, nextX];
                    if (list == null) continue;

                    outBuffer.AddRange(list);
                }
            }

            
        }
    }
}
