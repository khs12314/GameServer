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


            int userid = DBMananger.LoginAccount(packet.ID, packet.PassWord);
            if (userid != 0)
            {
                login.Sucess = true;
                login.UserID = userid;
                peer.send(login.Write());
            }
            else
            {
                login.Sucess = false;
                login.UserID = 0;
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
    }
}
