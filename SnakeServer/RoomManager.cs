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
            while (RoomPool.Count > 0)
            {
                SnakeGame_Logic room_PoP = RoomPool.Pop();
                room.Add(room_PoP.Room_ID, room_PoP);
                
            }
            

        }
        public bool Enter_Room(User_Session user1, int userid, int Room_id)
        {
            if (!room.ContainsKey(Room_id))
            {
                Console.WriteLine($"룸 아이디 검색 실패{Room_id}");
                return false;
            }
            Console.WriteLine($"룸 아이디 검색 성공{Room_id}, 유저아이디 {userid}");
            room[Room_id].Enter_Game(user1, userid);
            return true;
        }
        
    }
}
