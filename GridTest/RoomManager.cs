using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{

    //룸 생성 및 삭제 관리
    public class RoomManager
    {
        Dictionary<int, GameRoom_Logic> room;
        Stack<GameRoom_Logic> RoomPool;
        GameRoom_Logic[] RoomCache;
        public RoomManager(int poolsize)
        {
            room = new Dictionary<int, GameRoom_Logic>();
            RoomPool = new Stack<GameRoom_Logic>();
            initialize(poolsize);

        }
        public void initialize(int max_room_count)
        {
            room = new Dictionary<int, GameRoom_Logic>();
            RoomPool = new Stack<GameRoom_Logic>();
            for (int i = 0; i < max_room_count; i++)
            {
                RoomPool.Push(new GameRoom_Logic());
            }
            while (RoomPool.Count > 0)
            {
                GameRoom_Logic room_PoP = RoomPool.Pop();
                room.Add(room_PoP.Room_ID, room_PoP);
            }
            RoomCache = room.Values.ToArray();
        }

        public GameRoom_Logic[] GetAllRooms()
        {
            return RoomCache;
        }
        public bool Enter_Room(User_Session user, int userid, int Room_id)
        {
            if (room.TryGetValue(Room_id, out GameRoom_Logic? targetRoom) == false)
            {
                Console.WriteLine($"룸 아이디 검색 실패 {Room_id}");
                return false;
            }

            Console.WriteLine($"룸 아이디 검색 성공 {Room_id}, 유저아이디 {userid}");

            // [핵심] 찾은 방(targetRoom)에 큐를 넣음
            targetRoom.Push(() =>
            {
                // 여기는 나중에 Room 스레드가 실행함
                targetRoom.Enter_Game(user, userid);
            });
            return true;
        }

    }
}
