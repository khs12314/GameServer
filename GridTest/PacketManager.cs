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
        static int packetCount = 0;
        static int ErrorCount = 0;
        public static PacketManager Instance { get { return instance; } }

        public PacketManager()
        {

        }

        public void OnRecvPacket(PacketSession session, CPacket packet)
        {
            short protocol_id = packet.pop_protocol_id();
            //Console.WriteLine($"패킷 받음{(PacketID)protocol_id}");
           // Interlocked.Increment(ref packetCount);
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
                    //Console.WriteLine($"로스 패킷의 참조 값 아이디{(PacketID)protocol_id}");
                   // Console.WriteLine($"로스 패킷의 값 아이디{packet.protocol_id}");
                    //Console.WriteLine($"{packet.position}");
                   // Console.Write("반납된 로스 패킷의 메모리 : ");
                    //Console.WriteLine($"반납된 로스 패킷에 대한 정보 : 유저 아이디{packet.owner.Get_MyID()}, {packet.is_using}, {packet._refCount}");
                   // packet.PrintMemoryAddress();
                   // Interlocked.Increment(ref ErrorCount);
                    break;
            }
            //Console.WriteLine($"받은 패킷 반납 전 레프 카운트 : {packet._refCount}");
            packet.Release();

        }
    }
}
