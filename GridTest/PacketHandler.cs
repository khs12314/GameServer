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
                Console.WriteLine($"로그인받음{packet.ID}");
                if (session == null) 
                {
                    return;
                }
                if (packet.ID.StartsWith("Dummy_"))
                {
                    // 2. 고유 ID 생성 (Dummy_1 -> 1, Dummy_500 -> 500)
                    // 문자열 파싱해서 숫자로 변환. (실제 DB Unique ID 대신 사용)
                    string numberPart = packet.ID.Replace("Dummy_", "");
                    int dummyIndex = 0;

                    // 파싱 실패 방지 (혹시 Dummy_Asdf 이렇게 보낼까봐)
                    if (!int.TryParse(numberPart, out dummyIndex))
                    {
                        dummyIndex = packet.ID.GetHashCode(); // 실패하면 해시값 사용
                    }

                    // 3. 가짜 UserID 부여 (실제 유저와 겹치지 않게 100,000번대부터 시작)
                    int fakeUserId = 100000 + dummyIndex;
                    string fakeNickname = packet.ID;

                    // 4. 로그인 성공 처리 (DB 안 가고 바로 세팅)
                    session.SetUserID(fakeUserId);

                    login.Sucess = true;
                    login.UserID = fakeUserId;
                    login.Nickname = fakeNickname;

                    // 5. 응답 전송 및 입장
                    peer.send(login.Write());

                    // 로비 입장 및 서버 메모리에 유저 등록
                    session.Enter_Loby(Program.game_server);
                    Program.game_server.Set_User_Session(session, fakeUserId);

                    // 로그 하나 찍어주면 간지남
                    Console.WriteLine($"[Test] Dummy Login Success: {fakeNickname} (UID: {fakeUserId})");

                    return; // 더미는 여기서 끝. 아래 로직 실행 안 함.
                }

                {
                    // 일반 유저도 테스트 중엔 그냥 로그인 시키기 (임시)
                    int tempId = packet.ID.GetHashCode();
                    session.SetUserID(tempId);
                    login.Sucess = true;
                    login.UserID = tempId;
                    login.Nickname = packet.ID;

                    peer.send(login.Write());
                    session.Enter_Loby(Program.game_server);
                    Program.game_server.Set_User_Session(session, tempId);
                }
                //var (resultCode, nickname) = await DBMananger.LoginAsync(packet.ID, packet.PassWord);
                //if (resultCode != 0)
                //{
                //    session.SetUserID(resultCode);
                //    login.Sucess = true;
                //    login.UserID = resultCode;
                //    login.Nickname = nickname;
                //    peer.send(login.Write());
                //    session.Enter_Loby(Program.game_server);
                //    Program.game_server.Set_User_Session(session, resultCode);
                //    return;
                //}
                //else
                //{
                //    login.Sucess = false;
                //    login.UserID = 0;
                //    login.Nickname = "";
                //    peer.send(login.Write());
                //    return;
                //}

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
                
                return;

            }
        }
    }
}
