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
        public string UserID { get; private set; }
        public CGameRoom_Whack_a_Mole battle_room {  get; set; }
        Player player;
        public GameUser(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
            this.UserID = null;
        }
        void SendLoginOk()
        {
            CPacket msg = CPacket.create((short)PROTOCOL.LOGIN_OK);
            msg.push(this.UserID); // 문자열(String) 밀어넣기
            send(msg);
        }
        public string GetID()
        {
            return this.UserID;
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
            CPacket msg = CPacket.create((short)PROTOCOL.INTERNAL_DISCONNECT);
            msg.SetOwner(this);
            Program.game_main.enqueue_packet(msg, this);
        }
        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);
        }
        void IPeer.process_user_operation(CPacket msg, short protocolID = 0)
        {
            //Console.WriteLine("Enter_Game_Room_REQ");
            PROTOCOL protocol = (PROTOCOL)protocolID;
            //Console.WriteLine("protocol id " + protocol);

            switch (protocol)
            {
                case PROTOCOL.ENTER_GAME_ROOM_REQ:
                    this.UserID = $"User_{Guid.NewGuid().ToString().Substring(0, 4)}";
                    Console.WriteLine($"[Server] Client Connected. Assigned ID: {this.UserID}");
                    SendLoginOk();
                    Program.game_main.matching_req(this);

                    break;
                case PROTOCOL.LOADING_COMPLETED:
                    this.battle_room.loading_complete(this.player);
                    break;
                case PROTOCOL.HIT_MOLE_REQ:
                    int HitIndex = msg.pop_int32();
                    string ID = msg.pop_string();
                    Console.WriteLine($" User ID : {ID}");
                    if(this.battle_room != null)
                    {
                        this.battle_room.HandleHit(this, HitIndex, ID);
                    }

                    break;
            }
        }
        public void send(CPacket msg) 
        {
            Console.WriteLine($"3 - Null ref Test");
            this.token.send(msg);
        }
        public void enter_room(Player player, CGameRoom_Whack_a_Mole room)
        {
            this.player = player;
            this.battle_room = room;
        }
        
    }
}
