using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{

    //패킷 매니저
    public class PacketManager
    {
        static PacketManager instance = new PacketManager();
        //풀링을 위한 스택, 추후 구현 예정
        //static Stack<IPacket> iPacketPool = new Stack<IPacket>();
        public static PacketManager Instance { get { return instance; } }

        public PacketManager()
        {

        }

        public void OnRecvPakcet(IPeer session, CPacket packet)
        {
            short protocol_id = packet.pop_protocol_id();
            //Console.WriteLine($"패킷 받음{protocol_id}");
            switch ((PacketID)protocol_id)
            {
                case PacketID.C_Login_DB:

                    C_Login_DB chatPacket = new C_Login_DB();
                    chatPacket.Read(packet);
                    PacketHandler.C_LoginDBHandler(session, chatPacket);
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
                    c_Move.Read(packet);
                    PacketHandler.C_Move_SnakeHandler(session, c_Move);
                        break;
                case PacketID.C_PlayerLeave:
                    C_PlayerLeave c_PlayerLeave = new C_PlayerLeave();
                    c_PlayerLeave.Read(packet);
                    PacketHandler.C_Player_Leave_Handler(session, c_PlayerLeave);
                    break;
                default:
                    Console.WriteLine($"디폴트 {(PacketID)protocol_id}");
                    break;
            }
            packet.Release();
        }
    }
}
