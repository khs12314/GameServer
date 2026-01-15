using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    class Define
    {
        public static readonly short HEADER_SIZE = 2;
    }
    //접속한 유저의 세션(인 게임은 다루지 않음)
    public class User_Session : IPeer
    {
        static int _UserIDGenerator = 0;
        CUserToken CUserToken;
        public int Uid { get; private set; }
       public User_Session(CUserToken token)
        {
            this.CUserToken = token;
            this.CUserToken.set_peer(this);
            this.Uid = Interlocked.Increment(ref _UserIDGenerator);
        }

        void IPeer.on_message(Const<byte[]> buffer)
        {
            Console.WriteLine("메시지 받음");
            short protocolid = BitConverter.ToInt16(buffer.Value, 2);
            CPacket msg = CPacket.create(protocolid);
            msg.overwrite(buffer.Value, buffer.Value.Length);
            msg.SetOwner(this);

            Program.game_server.Enqueue_Job(msg, this);
        }

        void IPeer.on_removed()
        {
            Internal_Disconnect packet = new Internal_Disconnect();

            CPacket msg = packet.Write();
            msg.SetOwner(this);

            Program.game_server.Enqueue_Job(msg, this);
        }

        void IPeer.send(CPacket msg)
        {
            this.CUserToken.send(msg);
        }

        void IPeer.disconnect()
        {
            throw new NotImplementedException();
        }

        void IPeer.process_user_operation(CPacket msg)
        {
            PacketManager.Instance.OnRecvPakcet(this, msg);
        }
    }
}
