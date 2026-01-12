using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CSampleClient
{
    using GameServer;
    using Multi_player_Whack_a_Mole;

    public class CRemoteServerPeer : IPeer
    {
        public CUserToken token { get; private set; }

        // [중요] PacketHandler에서 데이터를 저장할 변수들
        private int myID = 0;
        public byte myIndex = 0;
        public short duration = 0;

        public CRemoteServerPeer(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
            this.myID = 0;
            this.duration = 0;
        }

        // [Setter/Getter] 캡슐화
        public void SetMyId(int ID) { myID = ID; }
        public int GetMyId() { return myID; }

        // ---------------------------------------------------------
        // [핵심 수정] 패킷 수신부 (Receive)
        // ---------------------------------------------------------
        void IPeer.on_message(Const<byte[]> buffer)
        {
            // 1. 버퍼를 CPacket으로 감싼다.
            CPacket msg = new CPacket(buffer.Value, this);
            Console.WriteLine("dasasdasd");
            // 2. [중요] 내가 직접 뜯어보지(pop) 않고, 전문가(PacketManager)에게 넘긴다.
            //    여기서 switch-case를 쓰면 하수입니다.
            PacketManager.Instance.OnRecvPacket(this, msg);
        }

        // ---------------------------------------------------------
        // [삭제됨] SendHitPacket 등 잡다한 로직은 PacketHandler로 이동했음.
        // ---------------------------------------------------------

        void IPeer.on_removed()
        {
            Console.WriteLine("[Client] Server Disconnected");
            // 연결 끊김 처리 (PacketHandler.S_RoomRemovedHandler 등에서 처리 가능)
        }

        public void send(CPacket msg)
        {
            this.token.send(msg);
        }

        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);
        }

        void IPeer.process_user_operation(CPacket msg)
        {
            // 더미 클라이언트는 보통 on_message에서 즉시 처리하므로 
            // 여기는 비워두거나, 스레드 모델에 따라 사용함.
            // 현재 구조에서는 on_message에서 PacketManager를 호출했으므로 비워둠.
        }
    }


}

