using GridTest.DataManagers;
using GridTest.Manager;
using GridTest.Room;
using GridTest.Server;
using GridTest.Session;
using GridTest.Util;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Network
{
    public static class TaskExtensions
    {
        public static async void SafeFireAndForget(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                // 여기서 로그를 남기면 서버가 죽지 않고 에러를 추적할 수 있다.
                Console.WriteLine($"[비동기 치명적 에러] {ex.Message}");
            }
        }
    }
    public class PacketHandler
    {
        public static bool USE_DB = false;
        public static async Task ReqCharacterList(User_Session peer)
        {
            if (peer is User_Session user_Session)
            {
                List<CharacterInfo> dbInfos = await DBMananger.GetCharacterInfos((peer as User_Session).Uid);
                S_ResCharacterList resPacket = NetworkPacketPool<S_ResCharacterList>.Pop();
                resPacket.Clear();
                resPacket.Characters.Clear();
                foreach (CharacterInfo dbInfo in dbInfos)
                {
                    S_ResCharacterList.CharactersItem item = new S_ResCharacterList.CharactersItem();
                    item.CharacterType = dbInfo.CharacterType;
                    item.Nickname = dbInfo.Nickname;
                    item.Level = dbInfo.Level;
                    item.CharacterId = dbInfo.CharacterId;
                    resPacket.Characters.Add(item);
                }
                peer.CUserToken.send(resPacket.Write());
                NetworkPacketPool<S_ResCharacterList>.Push(resPacket);
            }
        }

        public static async Task C_LoginDBHandler(IPeer peer, C_Login_DB packet)
        {
            try
            {
                User_Session? session = peer as User_Session;
                S_Login_DB login = NetworkPacketPool<S_Login_DB>.Pop();
                login.Clear();
                Console.WriteLine($"로그인받음{packet.ID}");
                if (session == null)
                {
                    return;
                }
                if (USE_DB == false)
                {
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
                        NetworkPacketPool<S_Login_DB>.Push(login);
                        // 로비 입장 및 서버 메모리에 유저 등록
                        session.Enter_Loby(Program.game_server);
                        Program.game_server.Set_User_Session(session, fakeUserId);


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
                        NetworkPacketPool<S_Login_DB>.Push(login);
                        session.Enter_Loby(Program.game_server);
                        Program.game_server.Set_User_Session(session, tempId);
                    }
                    Program.game_server.Push(() =>
                    {
                        Program.game_server.Set_User_Session(session, session.Uid);
                    });
                    return;
                }
                else
                {
                    var (resultCode, nickname) = await DBMananger.LoginAsync(packet.ID, packet.PassWord);
                    if (resultCode != 0)
                    {
                        session.SetUserID(resultCode);
                        login.Sucess = true;
                        login.UserID = resultCode;
                        login.Nickname = nickname;
                        peer.send(login.Write());
                        NetworkPacketPool<S_Login_DB>.Push(login);
                        session.Enter_Loby(Program.game_server);
                        Program.game_server.Set_User_Session(session, resultCode);
                        await ReqCharacterList(session);
                        return;
                    }
                    else
                    {
                        login.Sucess = false;
                        login.UserID = 0;
                        login.Nickname = "";
                        peer.send(login.Write());
                        NetworkPacketPool<S_Login_DB>.Push(login);
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
            }
            NetworkPacketPool<C_Login_DB>.Push(packet);
        }
        public static void C_SignUpDBHandler(IPeer peer, C_SignUp_DB packet)
        {
            User_Session? session = peer as User_Session;
            S_SignUp_DB signUp = NetworkPacketPool<S_SignUp_DB>.Pop();


        }
        public static void C_EnterGame_SnakeHandler(IPeer peer, C_EnterGame_Snake packet)
        {
            User_Session? session = peer as User_Session;
            //GameUser_Session gameUser_Session = peer as GameUser_Session;
            if (session == null)
            {
                Console.WriteLine("여기서 널 터짐");
                return;
            }
            //if (gameUser_Session == null) {
            //    Console.WriteLine("이것도 널");
            //}
            S_EnterGame_Snake enterGame = NetworkPacketPool<S_EnterGame_Snake>.Pop();
            enterGame.Clear();
            int room_id = packet.Room_id;
            Console.WriteLine($"패킷 핸들러 실행");
            Program.game_server.Enter_Game_Request(session, packet.userId, room_id);
            NetworkPacketPool<S_EnterGame_Snake>.Push(enterGame);
        }
        public static void C_Move_SnakeHandler(IPeer peer, C_Move pakcet)
        {

            if (peer is GameUser_Session session)
            {
                GameRoom_Logic room = session.room;
                room.Push(() =>
                {
                    session.room.ChangeDir(session, pakcet);
                });
            }
            NetworkPacketPool<C_Move>.Push(pakcet);

        }
        //public static void MMO_Move_Handler(IPeer peer, MMO_C_Move c_MMO_Move)
        //{
        //    if (peer is GameUser_Session session)
        //    {
        //        session.room.Push(() =>
        //        {
        //            session.room.MMO_Move(session, c_MMO_Move);
        //        });

        //    }
        //}

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
            NetworkPacketPool<C_PlayerLeave>.Push(c_PlayerLeave);
        }
        public static void C_MMO_Move_Handler(IPeer peer, MMO_C_Move c_MMO)
        {
            if (peer is GameUser_Session session)
            {
                if (session.MyPlayer.State == PlayerState.ATTACK || session.MyPlayer.State == PlayerState.DIE)
                {
                    session.room.Push(() =>
                    {
                        session.room.Sync_Pos(session);
                    });
                    return;
                }
                session.room.Push(() =>
                {
                    session.room.MMO_Move(session, c_MMO);
                });
            }
            NetworkPacketPool<MMO_C_Move>.Push(c_MMO);
        }
        public static void C_MMO_Attack_Handler(IPeer peer, MMO_C_Attack c_MMO_Attack)
        {

            if (peer is GameUser_Session session)
            {

                session.room.Push(() =>
                {
                    if (session.MyPlayer.State == PlayerState.DIE || session.MyPlayer.State == PlayerState.ATTACK)
                        return;
                    session.room.MMO_Attack(session, c_MMO_Attack);
                    session.room.PushAfter(500, () =>
                    {
                        session.MyPlayer.ChangeStateAndBroadcast(PlayerState.IDLE, session.room);
                    });
                });

            }
            NetworkPacketPool<MMO_C_Attack>.Push(c_MMO_Attack);
        }



        public static void C_UseSkill_Handler(IPeer peer, C_UseSkill c_UseSkill)
        {

            //보안검사 후
            if (peer is GameUser_Session session)
            {
                session.room.Push(() =>
                {
                    if (session.MyPlayer.State == PlayerState.DIE || session.MyPlayer.State == PlayerState.ATTACK)
                        return;
                    session.MyPlayer.UseSkill(c_UseSkill);

                });

            }
            NetworkPacketPool<C_UseSkill>.Push(c_UseSkill);
        }

        internal static void C_ResCharacterList_Handler(IPeer session, C_ResCharacterList c_ResCharacterList)
        {
            throw new NotImplementedException();
        }

        public static async Task C_CreateCharacter_HandlerAsync(IPeer session, C_CreateCharacter c_CreateCharacter)
        {
            bool success = false;

            if (session is User_Session user_Session)
            {
                Task.Run(async () =>
                {
                    success = await DBMananger.CreateCharacterAsync(user_Session.Uid, c_CreateCharacter.Nickname, c_CreateCharacter.CharacterType);
                }).SafeFireAndForget();

                if (success)
                {
                    Console.WriteLine($"캐릭터 생성 성공: {c_CreateCharacter.Nickname} (ID: {user_Session.Uid})");
                    S_CreateCharacter resPacket = NetworkPacketPool<S_CreateCharacter>.Pop();
                    resPacket.Clear();
                    resPacket.Success = true;
                    resPacket.Nickname = c_CreateCharacter.Nickname;
                    resPacket.CharacterType = c_CreateCharacter.CharacterType;
                    user_Session.CUserToken.send(resPacket.Write());
                    NetworkPacketPool<S_CreateCharacter>.Push(resPacket);
                    if (USE_DB)
                        await ReqCharacterList(user_Session); // 캐릭터 리스트 갱신
                }
                else
                {
                    Console.WriteLine($"캐릭터 생성 실패: {c_CreateCharacter.Nickname}");
                    S_CreateCharacter resPacket = NetworkPacketPool<S_CreateCharacter>.Pop();
                    resPacket.Clear();
                    resPacket.Success = false;
                    resPacket.Nickname = c_CreateCharacter.Nickname;
                    resPacket.CharacterType = c_CreateCharacter.CharacterType;
                    user_Session.CUserToken.send(resPacket.Write());
                    NetworkPacketPool<S_CreateCharacter>.Push(resPacket);
                    // 실패 응답 패킷 보내기 (필요하다면)
                }

            }
            NetworkPacketPool<C_CreateCharacter>.Push(c_CreateCharacter);
        }

        public static void C_MMOGetItem_Handler(IPeer session, C_MMOGetItem c_MMOGetItem)
        {
            if (session is GameUser_Session gameUser_Session)
            {
                if (!gameUser_Session.MyPlayer.CanLoot())
                {
                    return;
                }
                gameUser_Session.room.Push(() =>
                {
                    gameUser_Session.room.Get_Item(gameUser_Session, c_MMOGetItem);
                });
            }
            NetworkPacketPool<C_MMOGetItem>.Push(c_MMOGetItem);
        }

        public static void C_SwapItem_Handler(IPeer session, C_SwapItem c_SwapItem)
        {
            if (session is GameUser_Session gameUser_Session)
            {
                gameUser_Session.room.Push(() =>
                {
                    gameUser_Session.room.Swap_Item(gameUser_Session, c_SwapItem);
                });
            }
            NetworkPacketPool<C_SwapItem>.Push(c_SwapItem);
        }

        public static void C_NpcInteract_Handler(IPeer session, C_NpcInteract c_NpcInteract)
        {
            if (session is GameUser_Session gameUser_Session)
            {
                gameUser_Session.room.Push(() =>
                {
                    gameUser_Session.room.Npc_Interact(gameUser_Session, c_NpcInteract);
                });
            }
            NetworkPacketPool<C_NpcInteract>.Push(c_NpcInteract);
        }

        public static void C_AcceptQuest_Handler(IPeer session, C_AcceptQuest c_AcceptQuest)
        {
            if (session is GameUser_Session gameUser_Session)
            {
                gameUser_Session.room.Push(() =>
                {
                    gameUser_Session.room.Accept_Quest(gameUser_Session, c_AcceptQuest);
                });
            }
            NetworkPacketPool<C_AcceptQuest>.Push(c_AcceptQuest);   
        }

        public static void C_CompeleteQuest_Handler(IPeer session, C_CompleteQuest c_CompleteQuest)
        {
            if (session is GameUser_Session gameUser_Session)
            {
                gameUser_Session.room.Push(() =>
                {

                    gameUser_Session.room.CompleteQuest(gameUser_Session, c_CompleteQuest);
                });
            }
        }

        public static void C_CUseItem_Handler(IPeer session, C_UseItem c_consumeItme)
        {
            if (session is GameUser_Session gameUser_Sessions)
            {
                gameUser_Sessions.room.Push(() =>
                {
                    gameUser_Sessions.MyPlayer.UseItem(c_consumeItme);

                });
            }
            NetworkPacketPool<C_UseItem>.Push(c_consumeItme);
        }

        internal static void C_UnequipItem_Handler(IPeer session, C_UnequipItem c_equipItem)
        {
            if (session is GameUser_Session gameUser_Sessions)
            {
                gameUser_Sessions.room.Push(() =>
                {
                    gameUser_Sessions.MyPlayer.UnEquipWeapon(c_equipItem);
                });
            }
            NetworkPacketPool<C_UnequipItem>.Push(c_equipItem);
        }
    }
}


