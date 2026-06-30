using GridTest.DataManagers;
using GridTest.InGameObj;
using GridTest.Room;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GridTest.Server
{

    //룸 생성 및 삭제 관리
    public class Channel
    {
        public int ChannelId;
        public readonly Dictionary<int, GameRoom_Logic> room;
        //이전 작업 흔적 추후 지울 예정, pool은 불필요
        Stack<GameRoom_Logic> RoomPool;
        GameRoom_Logic[] RoomCache;
        public Channel(int channelid)
        {
            ChannelId = channelid;
            room = new Dictionary<int, GameRoom_Logic>();
            RoomPool = new Stack<GameRoom_Logic>();
            InitializeRoom();

        }

        public GameRoom_Logic[] GetAllRooms()
        {
            return RoomCache;
        }
        
        public GameRoom_Logic GetRoom(int roomid)
        {
            return room[roomid];
        }
        public void InitializeRoom()
        {
            // 전역 메모리에 올라간 맵 데이터들을 순회
            foreach (var kvp in MapDataManager.MapDict)
            {
                Console.WriteLine($"채널 로그: {MapDataManager.MapDict.Count}개의 맵 존재");
                int mapid = kvp.Key;
                int[,] collisionData = kvp.Value;

                GameRoom_Logic newRoom = new GameRoom_Logic();
                // [핵심] Room을 초기화할 때, 새 배열을 만들지 않고 원본 배열의 참조(Reference)만 넘긴다.
                newRoom.RoomInit(ChannelId, mapid, collisionData);

                room.Add(mapid, newRoom);
            }
        }
        // Channel.cs 또는 MapDataManager 내부에 존재해야 할 핵심 탐색 함수
        public GameRoom_Logic FindRoomByGlobalPosition(float globalX, float globalY)
        {
            // O(N) 순회 탐색 (방이 100개 정도면 Dictionary나 List 순회로도 CPU 연산량은 0에 가깝다)
            foreach (var room in room.Values)
            {
                GameMap gameMap = room.GetGameMap();
                Console.WriteLine($"현재 룸 X :{gameMap.MinX}{gameMap.MaxX}현재 룸 z{gameMap.MinY}{gameMap.MaxY}");
                Console.WriteLine($"현재 글로벌 좌표 : {globalX},{globalY}");
                // 글로벌 좌표가 방의 (Min ~ Max) 영역 안에 들어가는지 검사
                if (globalX >= gameMap.MinX && globalX < gameMap.MaxX &&
                    globalY >= gameMap.MinY && globalY < gameMap.MaxY)
                {
                    return room; // 빙고! 이 방이다.
                }
            }
            Console.WriteLine("맵이 존재하지않음");
            return null; // 맵의 완전한 바깥(절벽, 심해)으로 떨어짐
        }

    }
}
