using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{

    public class PacketHandler
    {
        public static void C_LoginDBHandler(IPeer peer, C_Login_DB packet)
        {
            User_Session? session = peer as User_Session;
            S_Login_DB login = new S_Login_DB();
            string nickname = "";

            int userid = DBMananger.LoginAccount(packet.ID, packet.PassWord, out nickname);
            if (userid != 0)
            {
                login.Sucess = true;
                login.UserID = userid;
                login.Nickname = nickname;
                peer.send(login.Write());
            }
            else
            {
                login.Sucess = false;
                login.UserID = 0;
                login.Nickname = "";
                peer.send(login.Write());
            }
            Program.game_server.Set_User_Session(session, userid);

        }
        public static void C_SignUpDBHandler(IPeer peer, C_SignUp_DB packet)
        {
            User_Session? session = peer as User_Session;
            S_SignUp_DB signUp = new S_SignUp_DB();

            bool success = DBMananger.CreateAccount(packet.ID, packet.PassWord, packet.nickname);
            if (success)
            {
                signUp.Sucess = true;
                signUp.nickname = packet.nickname;
                peer.send(signUp.Write());
            }
            else
            {
                signUp.Sucess = false;
                signUp.nickname = "";
                peer.send(signUp.Write());
            }


        }
        public static void C_EnterGame_SnakeHandler(IPeer peer, C_EnterGame_Snake packet)
        {
            User_Session? session = peer as User_Session;
            S_EnterGame_Snake enterGame = new S_EnterGame_Snake();
            int room_id = packet.Room_id;
            Console.WriteLine($"패킷 핸들러 실행");
            Program.game_server.Enter_Game_Request(session, packet.userId, room_id);

            //enterGame.posX = 0.0f;
            //enterGame.posY = 0.0f;
            //enterGame.userid = session.Uid;
            //peer.send(enterGame.Write());


        }
        public static void C_Move_SnakeHnadler(IPeer peer, C_Move pakcet)
        {
            GameUser_Session session = peer as GameUser_Session;
            
            SnakeGame_Logic room = session.room;
            
            room.HandleMove(session,pakcet);
            
        }
    }
}
