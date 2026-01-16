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
        Dictionary<int, SnakeGame_Logic> room;
        Stack<SnakeGame_Logic> RoomPool;
        public RoomManager(int poolsize)
        {
            room = new Dictionary<int, SnakeGame_Logic>();
            RoomPool = new Stack<SnakeGame_Logic>();
            initialize(poolsize);

        }
        public void initialize(int max_room_count)
        {
            room = new Dictionary<int, SnakeGame_Logic>();
            RoomPool = new Stack<SnakeGame_Logic>();
            for (int i = 0; i < max_room_count; i++)
            {
                RoomPool.Push(new SnakeGame_Logic());
            }
        }
        public bool Create_Room(User_Session user1, int userid)
        {
            if (RoomPool.Count == 0)
                return false;
            SnakeGame_Logic newRoom = RoomPool.Pop();
            newRoom.Enter_Game(user1, userid);
            room.Add(newRoom.Room_ID, newRoom);
            return true;
        }
    }
}
