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
            GameUser_Session session = peer as GameUser_Session;
            
            
            long userid = DBMananger.LoginAccount(packet.ID, packet.PassWord);
            if(userid == -1)
            {

            }
            else
            {

            }


        }
    }
}
