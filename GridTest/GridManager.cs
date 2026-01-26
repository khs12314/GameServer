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
        List<GameUser_Session>[,] _cells;
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

            _cells = new List<GameUser_Session>[_gridSizeY, _gridSizeX];
            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x = 0; x < _gridSizeX; x++)
                {
                    _cells[y, x] = new List<GameUser_Session>();
                }
            }
        }

        public void Add(GameUser_Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            int x = (int)(session.GameUser_Session_Info.userPositionX / _cellSize);
            int y = (int)(session.GameUser_Session_Info.userPositionY / _cellSize);

            // 범위 검사
            if (x < 0 || x >= _gridSizeX || y < 0 || y >= _gridSizeY) return;

            session.GameUser_Session_Info.CellX = x;
            session.GameUser_Session_Info.CellY = y;

            lock (_lock)
            {
                var list = _cells[y, x];
                if (list == null)
                {
                    list = new List<GameUser_Session>();
                    _cells[y, x] = list;
                }
                list.Add(session);
            }
        }

        public void Remove(GameUser_Session session)
        {
            if (session == null) return;

            int x = session.GameUser_Session_Info.CellX;
            int y = session.GameUser_Session_Info.CellY;

            // 방어: 유효한 캐시 인덱스인지 확인
            if (x < 0 || x >= _gridSizeX || y < 0 || y >= _gridSizeY) return;

            lock (_lock)
            {
                _cells[y, x]?.Remove(session);
            }
        }

        public void Move(GameUser_Session session, GameUser_Session.Pos newPos)
        {
            if (session == null) return;

            int newX = (int)(newPos.x / _cellSize);
            int newY = (int)(newPos.y / _cellSize);

            // 맵 밖이면 무시
            if (newX < 0 || newX >= _gridSizeX || newY < 0 || newY >= _gridSizeY) return;

            int oldX = session.GameUser_Session_Info.CellX;
            int oldY = session.GameUser_Session_Info.CellY;

            // 같은 셀이면 아무것도 안 함
            if (oldX == newX && oldY == newY) return;

            lock (_lock)
            {
                // 기존 셀에서 제거 (방어적 검사)
                if (oldX >= 0 && oldX < _gridSizeX && oldY >= 0 && oldY < _gridSizeY)
                {
                    _cells[oldY, oldX]?.Remove(session);
                }

                // 새 셀에 추가
                var list = _cells[newY, newX];
                if (list == null)
                {
                    list = new List<GameUser_Session>();
                    _cells[newY, newX] = list;
                }
                list.Add(session);

                // 캐시 갱신
                session.GameUser_Session_Info.CellX = newX;
                session.GameUser_Session_Info.CellY = newY;
            }

            Console.WriteLine($"현재 aoi : {session.GameUser_Session_Info.CellX},{session.GameUser_Session_Info.CellY}");
        }

        public List<GameUser_Session> GetNeighbors(GameUser_Session session)
        {
            var sessions = new List<GameUser_Session>();
            if (session == null) return sessions;

            int centerX = session.GameUser_Session_Info.CellX;
            int centerY = session.GameUser_Session_Info.CellY;

            // 방어: 캐시가 유효한지 확인
            if (centerX < 0 || centerX >= _gridSizeX || centerY < 0 || centerY >= _gridSizeY) return sessions;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nextX = centerX + dx;
                    int nextY = centerY + dy;

                    if (nextX < 0 || nextX >= _gridSizeX || nextY < 0 || nextY >= _gridSizeY) continue;

                    var list = _cells[nextY, nextX];
                    if (list == null) continue;

                    sessions.AddRange(list);
                }
            }

            return sessions;
        }
    }
}
