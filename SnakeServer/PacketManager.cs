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
        PacketManager instance = new PacketManager();
        public static PacketManager Instance { get { return Instance; } }

        public PacketManager()
        {

        }

        public void OnRecvPakcet(IPeer session, CPacket packet)
        {
            short protocol_id = packet.pop_protocol_id();
            switch ((PacketID)protocol_id)
            {
                case PacketID.C_Login_DB:
                    C_Login_DB chatPacket = new C_Login_DB();
                    chatPacket.Read(packet);
                    PacketHandler.C_LoginDBHandler(session, chatPacket);
                    break;

                default:
                    Console.WriteLine("das");
                    break;
            }
        }
    }
}
