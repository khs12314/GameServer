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
            Console.WriteLine("C_LoginDB");
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
                default:
                    Console.WriteLine("das");
                    break;
            }
        }
    }
}
