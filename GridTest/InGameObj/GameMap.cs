using GridTest.DataManagers;
using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.InGameObj
{
    public class GameMap
    {
        // 맵의 범위 (Global Coordinates)

        // 충돌 데이터 (true: 벽/못감, false: 갈수있음)
        // 2차원 배열 [x, y]
        const float PlayerRadius = 0.4f;

        int[,] _collision;

        public int MinX { get; private set; }
        public int MaxX { get; private set; }
        public int MinY { get; private set; }
        public int MaxY { get; private set; }
        public GameMap()
        {
            MinX = 0;
            MaxX = 0;
            MinY = 0;
            MaxY = 0;
        }
        public void GameMapInit(MapData mapData, int[,] collision)
        {
            MinX = mapData.MinX;
            MaxX = mapData.MaxX;
            MinY= mapData.MinY;
            MaxY = mapData.MaxY;
            _collision = collision;
        }
        public MoveResult CanGo(float x, float z)
        {
            // 캐릭터의 4귀퉁이 좌표 계산
            // (MinX, MaxX가 아니라 현재 좌표 기준임에 주의)
            float minX = x - PlayerRadius;
            float maxX = x + PlayerRadius;
            float minZ = z - PlayerRadius;
            float maxZ = z + PlayerRadius;

            // 4개의 모서리가 모두 안전지대인지 확인
            // 하나라도 벽이면 false (못 감)
            MoveResult res1 = CanGo((int)minX, (int)minZ);
            if (res1 != MoveResult.Success) return res1;

            MoveResult res2 = CanGo((int)maxX, (int)minZ);
            if (res2 != MoveResult.Success) return res2;

            MoveResult res3 = CanGo((int)minX, (int)maxZ);
            if (res3 != MoveResult.Success) return res3;

            MoveResult res4 = CanGo((int)maxX, (int)maxZ);
            if (res4 != MoveResult.Success) return res4;


            return MoveResult.Success; // 4곳 다 통과!
        }

        // 기존 int 버전 (단일 셀 체크)
        public MoveResult CanGo(int globalX, int globalY)
        {
            // 1. 맵 범위 체크
            if (globalX < MinX || globalX >= MaxX || globalY < MinY || globalY >= MaxY)
                return MoveResult.OutOfBounds;

            // 2. [핵심] 글로벌 좌표(75)를 로컬 인덱스(25)로 변환
            int localX = globalX - MinX; // 75 - 50 = 25
            int localY = globalY - MinY;

            // 3. 벽 체크
            if (_collision == null)
            {
                Console.WriteLine("맵이 로드되지 않았습니다.");
                return MoveResult.MapNull;
            }
            if(_collision[localX, localY] == 1)
            {
                return MoveResult.Obstacle; // 벽임
            }
            else
            {
                return MoveResult.Success; // 갈 수 있음
            }
            
        }



    }
}
