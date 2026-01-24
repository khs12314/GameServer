using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{

    public class PacketHandler
    {
        public static async void C_LoginDBHandler(IPeer peer, C_Login_DB packet)
        {
            try
            {
                User_Session? session = peer as User_Session;
                S_Login_DB login = new S_Login_DB();
                if (session == null) 
                {
                    return;
                }
                
                var (resultCode, nickname) = await DBMananger.LoginAsync(packet.ID, packet.PassWord);
                if (resultCode != 0)
                {
                    session.SetUserID(resultCode);
                    login.Sucess = true;
                    login.UserID = resultCode;
                    login.Nickname = nickname;
                    peer.send(login.Write());
                    session.Enter_Loby(Program.game_server);
                    Program.game_server.Set_User_Session(session, resultCode);
                    return;
                }
                else
                {
                    login.Sucess = false;
                    login.UserID = 0;
                    login.Nickname = "";
                    peer.send(login.Write());
                    return;
                }

            }
            catch (Exception ex) 
            {
                Console.WriteLine($"{ex}");
            }

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
            //GameUser_Session gameUser_Session = peer as GameUser_Session;
            if (session == null) {
                Console.WriteLine("여기서 널 터짐");
                return;
            }
            //if (gameUser_Session == null) {
            //    Console.WriteLine("이것도 널");
            //}
            S_EnterGame_Snake enterGame = new S_EnterGame_Snake();
            int room_id = packet.Room_id;
            Console.WriteLine($"패킷 핸들러 실행");
            Program.game_server.Enter_Game_Request(session, packet.userId, room_id);

            //enterGame.posX = 0.0f;
            //enterGame.posY = 0.0f;
            //enterGame.userid = session.Uid;
            //peer.send(enterGame.Write());


        }
        public static void C_Move_SnakeHandler(IPeer peer, C_Move pakcet)
        {
            
            if(peer is GameUser_Session session)
            {
                SnakeGame_Logic room = session.room;
                room.Push(() => {
                    session.room.ChangeDir(session, pakcet);
                });
                
            }
            
            
        }

        public static void C_Player_Leave_Handler(IPeer peer, C_PlayerLeave c_PlayerLeave)
        {
            if (peer is GameUser_Session session)
            {
                session.room.Push(() =>
                {
                    session.room.LeavGame(session, c_PlayerLeave.userid);
                });
                Console.WriteLine("무브에서 널 발견");
                return;

            }
        }
    }
}
