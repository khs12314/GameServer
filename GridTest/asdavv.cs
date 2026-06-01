//using SnakeServer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static System.Collections.Specialized.BitVector32;

//namespace GridTest
//{
//    public class asdas
//    {
//        List<GameUser_Session>[,] _cells;//각 리스트의 요소에 n만큼 탐색 속도가 점점 늘어나기 때문에 다른 자료 구조 고려. LinkedList는 잦은 위치 이동 시 메모리 릭 유발해 일단 배제

//        int _cellSize;
//        int _gridSizeX;
//        int _gridSizeY;

//        public asdas(int mapSize, int cellSize)
//        {
//            _gridSizeX = mapSize;
//            _gridSizeY = mapSize;
//            _cellSize = cellSize;
//            _cells = new List<GameUser_Session>[_cellSize, _cellSize];
//            for (int y = 0; y < _cellSize; y++)
//            {
//                for (int x = 0; x < _cellSize; x++)
//                {
//                    _cells[y, x] = new List<GameUser_Session>();
//                }
//            }
//        }
//        public void Add(GameUser_Session session)
//        {
//            // 1. 현재 좌표를 기준으로 방 번호 계산
//            // (주의: 반드시 float 나눗셈 후 int 캐스팅 할 것)
//            int x = (int)(session.GameUser_Session_Info.userPositionX / _cellSize);
//            int y = (int)(session.GameUser_Session_Info.userPositionY / _cellSize);

//            // [방어 코드] 배열 범위 벗어나면 서버 터짐. 꼭 막아라.
//            if (x < 0 || x >= _gridSizeX || y < 0 || y >= _gridSizeY) return;

//            // 2. 세션에 "나 여기 살아요"라고 명찰 달아줌 (Caching)
//            session.GameUser_Session_Info.CellX = x;
//            session.GameUser_Session_Info.CellY = y;
//            if (session == null)
//            {
//               
//            }
//            lock (this)
//            {
//                // 3. 실제 방에 입주  
//                _cells[y, x].Add(session);
//            }
//        }

//        public void Remove(GameUser_Session session)
//        {
//            // 1. 저장해둔 명찰(인덱스)을 보고 바로 찾는다. (계산 X)
//            int x = session.GameUser_Session_Info.CellX;
//            int y = session.GameUser_Session_Info.CellY;

//            // 방어: 혹시 초기화 안 된 상태면 무시
//            if (x < 0 || y < 0) return;

//            // 2. 방에서 짐 뺌
//            _cells[y, x].Remove(session);
//        }

//        public void Move(GameUser_Session session, GameUser_Session.Pos newPos)
//        {
//            // 1. 새로운 목적지의 방 번호 계산
//            int newX = (int)(newPos.x / _cellSize);
//            int newY = (int)(newPos.y / _cellSize);

//            // [방어 코드] 맵 밖으로 나가면? (로직에 따라 튕겨내거나 클램핑)
//            if (newX < 0 || newX >= _gridSizeX || newY < 0 || newY >= _gridSizeY) return;

//            // 2. [최적화 핵심] "방이 안 바뀌었으면 아무것도 하지 마라"
//            if (session.GameUser_Session_Info.CellX == newX && session.GameUser_Session_Info.CellY == newY)
//            {
//                return;
//            }

//            // 3. 방이 바뀌었다! (이사)
//            // 옛날 방에서 빼고 (Remove 로직 인라인 or 호출)
//            _cells[session.GameUser_Session_Info.CellY, session.GameUser_Session_Info.CellX].Remove(session);

//            // 새 방에 넣고
//            _cells[newY, newX].Add(session);

//            // 명찰 갱신
//            session.GameUser_Session_Info.CellX = newX;
//            session.GameUser_Session_Info.CellY = newY;

//            // 로그: "이사 완료: (1,1) -> (1,2)" 찍어보면 쾌감 쩐다.
//            ($"현재 aoi : {session.GameUser_Session_Info.CellX},{session.GameUser_Session_Info.CellY}");
//        }
//        public List<GameUser_Session> GetNeighbors(GameUser_Session session)
//        {
//            List<GameUser_Session> sessions = new List<GameUser_Session>();
//            int CenterX = session.GameUser_Session_Info.CellX;
//            int CenterY = session.GameUser_Session_Info.CellY;

//            for (int dy = -1; dy <= 1; dy++)
//            {
//                for (int dx = -1; dx <= 1; dx++)
//                {
//                    int nextX = CenterY + dx;
//                    int nextY = CenterX + dy;
//                    if (nextX < 0 || nextX >= _gridSizeX || nextY < 0 || nextY >= _gridSizeY)
//                    {
//                        continue;
//                    }
//                    if (_cells[nextX, nextY] == null) { continue; }
//                    sessions.AddRange(_cells[nextX, nextY]);
//                }
//            }
//            return sessions;
//        }
//    }
//}
