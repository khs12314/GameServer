using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_player_Whack_a_Mole
{
    public class GameRoomManager
    {
        List<CGameRoom_Whack_a_Mole> rooms;
        public GameRoomManager()
        {
            rooms = new List<CGameRoom_Whack_a_Mole>();
        }
        /// <summary>
        /// 매칭한 유저들을 넘겨 받아 게임 방을 생성한다.
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        public void create_room(GameUser user1, GameUser user2)
        {
            //게임 방을 생성하여 입장시킴.
            CGameRoom_Whack_a_Mole battleroom = new CGameRoom_Whack_a_Mole();
            battleroom.Enter_Game_Room(user1, user2);
            //방 리스트에 추가하여 관리
            this.rooms.Add(battleroom);
            
        }
        public void remove_room(CGameRoom_Whack_a_Mole room)
        {
            room.destroy();
            this.rooms.Remove(room);
        }
    }
}
