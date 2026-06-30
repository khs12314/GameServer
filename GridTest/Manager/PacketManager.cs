using GridTest.Network;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Manager
{

    //패킷 매니저
    public class PacketManager
    {
        static PacketManager instance = new PacketManager();
        //풀링을 위한 스택, 추후 구현 예정
        //static Stack<IPacket> iPacketPool = new Stack<IPacket>();
        public static PacketManager Instance { get { return instance; } }
        int count = 0;
        public PacketManager()
        {

        }

        public void OnRecvPacket(IPeer session, CPacket packet)
        {

            short protocolid = BitConverter.ToInt16(packet.buffer, packet.position);
            if (packet.protocol_id != protocolid)
            {
                Console.WriteLine($"다름 {(PacketID)protocolid}, {protocolid}");
                return;
            }
            short protocol_id = packet.pop_protocol_id();
            //Console.WriteLine($"패킷 받음{(PacketID)protocol_id}");
            switch ((PacketID)protocol_id)
            {
                case PacketID.C_Login_DB:

                    C_Login_DB chatPacket = new C_Login_DB();
                    chatPacket.Read(packet);
                    Task.Run(async () => await PacketHandler.C_LoginDBHandler(session, chatPacket));
                    break;
                case PacketID.C_SignUp_DB:
                    C_SignUp_DB c_SignUp_DB = new C_SignUp_DB();
                    c_SignUp_DB.Read(packet);
                    PacketHandler.C_SignUpDBHandler(session, c_SignUp_DB);
                    break;
                case PacketID.C_EnterGame_Snake:
                    C_EnterGame_Snake c_EnterGame_Snake = new C_EnterGame_Snake();
                    c_EnterGame_Snake.Read(packet);
                    Console.WriteLine("엔터 게임 실행");
                    PacketHandler.C_EnterGame_SnakeHandler(session, c_EnterGame_Snake);
                    break;
                case PacketID.C_Move:
                    C_Move c_Move = new C_Move();
                    Interlocked.Increment(ref count);

                    c_Move.Read(packet);
                    PacketHandler.C_Move_SnakeHandler(session, c_Move);
                    break;
                case PacketID.C_PlayerLeave:
                    C_PlayerLeave c_PlayerLeave = new C_PlayerLeave();
                    c_PlayerLeave.Read(packet);
                    PacketHandler.C_Player_Leave_Handler(session, c_PlayerLeave);
                    break;
                case PacketID.MMO_C_Move:
                    MMO_C_Move c_MMO_Move = new MMO_C_Move();
                    c_MMO_Move.Read(packet);
                    PacketHandler.C_MMO_Move_Handler(session, c_MMO_Move);
                    //Console.WriteLine($"mmo 무빙 {session.Get_MyID()} : {c_MMO_Move.posX}, {c_MMO_Move.posY}, {c_MMO_Move.posZ}, {c_MMO_Move.rot}");
                    break;
                case PacketID.MMO_C_Attack:
                    MMO_C_Attack c_MMO_Attack = new MMO_C_Attack();
                    c_MMO_Attack.Read(packet);
                    PacketHandler.C_MMO_Attack_Handler(session, c_MMO_Attack);
                    break;
                case PacketID.C_ResCharacterList:
                    C_ResCharacterList c_ResCharacterList = new C_ResCharacterList();
                    c_ResCharacterList.Read(packet);
                    PacketHandler.C_ResCharacterList_Handler(session, c_ResCharacterList);
                    break;
                case PacketID.C_UseSkill:
                    C_UseSkill c_UseSkill = new C_UseSkill();
                    c_UseSkill.Read(packet);
                    PacketHandler.C_UseSkill_Handler(session, c_UseSkill);
                    break;
                case PacketID.C_CreateCharacter:
                    C_CreateCharacter c_CreateCharacter = new C_CreateCharacter();
                    c_CreateCharacter.Read(packet);
                    Task.Run(async () =>
                    await PacketHandler.C_CreateCharacter_HandlerAsync(session, c_CreateCharacter)
                    );
                    break;
                case PacketID.C_MMOGetItem:
                    C_MMOGetItem c_MMOGetItem = new C_MMOGetItem();
                    c_MMOGetItem.Read(packet);
                    PacketHandler.C_MMOGetItem_Handler(session, c_MMOGetItem);
                    Console.WriteLine("getitem packet");
                    break;
                case PacketID.C_SwapItem:
                    C_SwapItem c_SwapItem = new C_SwapItem();
                    c_SwapItem.Read(packet);
                    PacketHandler.C_SwapItem_Handler(session, c_SwapItem);
                    Console.WriteLine("swap inventory packet");
                    break;
                case PacketID.C_NpcInteract:
                    C_NpcInteract c_NpcInteract = new C_NpcInteract();
                    c_NpcInteract.Read(packet);
                    PacketHandler.C_NpcInteract_Handler(session, c_NpcInteract);
                    break;
                case PacketID.C_AcceptQuest:
                    C_AcceptQuest c_AcceptQuest = new C_AcceptQuest();
                    c_AcceptQuest.Read(packet);
                    PacketHandler.C_AcceptQuest_Handler(session, c_AcceptQuest);
                    break;
                case PacketID.C_CompleteQuest:
                    C_CompleteQuest c_CompleteQuest = new C_CompleteQuest();
                    c_CompleteQuest.Read(packet);
                    PacketHandler.C_CompeleteQuest_Handler(session, c_CompleteQuest);
                    break;
                    case PacketID.C_UseItem:
                        C_UseItem c_consumeItme = new C_UseItem();
                    c_consumeItme.Read(packet);
                    PacketHandler.C_ConsumeItem_Handler( session, c_consumeItme);
                    break;
                    
                default:
                    Console.WriteLine($"디폴트 {(PacketID)protocol_id}");
                    Console.WriteLine($"{packet.position}");
                    break;
            }
            packet.Release();
        }
    }
}
