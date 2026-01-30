using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
        //static int _UserIDGenerator = 0;
        public CUserToken CUserToken;
        public GameServer Loby { get; set; }
        public int Uid { get; private set; }
        public User_Session(CUserToken token)
        {
            this.CUserToken = token;
            this.CUserToken.set_peer(this);
            this.Uid = 0;
            
        }
        public void SetUserID(int userid)
        {
            Uid = userid;
        }
        public void Enter_Loby(GameServer gameServer)
        {
            Loby = gameServer;
        }
        void IPeer.on_message(Const<byte[]> buffer)
        {
            short protocolid = BitConverter.ToInt16(buffer.Value, 2);
            //Console.WriteLine($"메시지 받음 {(PacketID)protocolid}");
            CPacket msg = CPacket.create(protocolid);
            msg.overwrite(buffer.Value, buffer.Value.Length);
            msg.SetOwner(this);
            if (msg.GetRefCount() < 0)
            {
                Console.WriteLine("레프카운트");
            }
            //msg.move_position(Define.HEADER_SIZE);
            //Console.WriteLine($"받은 메시지 포지션 : {msg.position}");
            Program.game_server.Enqueue_Job(msg, this.CUserToken);
        }

        void IPeer.on_removed()
        {
            Program.remove_user(this);
        }
        int IPeer.Get_MyID()
        {
            return this.Uid;
        }
        void IPeer.send(CPacket msg)
        {
            msg.is_RecvPacket = true;
            msg.is_sendPacket = true;
            msg.Retain();
            //Console.WriteLine($"보내는 패킷 아이디 유저 세션 {(PacketID)msg.protocol_id}");
            this.CUserToken.send(msg);
        }

        void IPeer.disconnect()
        {
            Loby.User_Disconnect(this);
        }

        void IPeer.process_user_operation(CPacket msg)
        {
            if (msg.GetRefCount() < 0)
            {
                Console.WriteLine("레프카운트");
            }
            PacketManager.Instance.OnRecvPakcet(this, msg);
            //msg.Release();
        }
    }
}
