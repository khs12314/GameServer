using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetWorkModule;
namespace Multi_player_Whack_a_Mole
{
    public class GameUser :IPeer
    {
        CUserToken token;
        public CGameRoom_Whack_a_Mole battle_room {  get; set; }
        Player player;
        public GameUser(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
        }
        void IPeer.on_message(Const<byte[]> buffer)
        {
            //ex)
            byte[] clone = new byte[1024];
            Array.Copy(buffer.Value, clone, buffer.Value.Length);
            CPacket msg = new CPacket(clone,this);
            Program.game_main.enqueue_packet(msg, this);
        }
        void IPeer.on_removed()
        {
            Console.WriteLine("The client disconnected");
            //
        }
        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);
        }
        void IPeer.process_user_operation(CPacket msg)
        {
            Console.WriteLine("Enter_Game_Room_REQ");
            PROTOCOL protocol = (PROTOCOL)msg.pop_protocol_id();
            Console.WriteLine("protocol id " + protocol);

            switch (protocol)
            {
                case PROTOCOL.ENTER_GAME_ROOM_REQ:
                    Program.game_main.matching_req(this);
                    break;
                case PROTOCOL.LOADING_COMPLETED:
                    this.battle_room.loading_complete(player);
                    break;
            }
        }
        public void send(CPacket msg) 
        { 
            this.token.send(msg);
        }
        public void enter_room(Player player, CGameRoom_Whack_a_Mole room)
        {
            this.player = player;
            this.battle_room = room;
        }
        
    }
}
