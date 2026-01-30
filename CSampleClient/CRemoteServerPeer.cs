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
        public int DummyIndex { get; private set; } // 내가 몇 번째 더미인지
        public int MyServerID { get;  set; } // 서버가 준 실제 ID
        public int CurrentDirection { get; set; } = 0;
        // [중요] PacketHandler에서 데이터를 저장할 변수들
        private int myID = 0;
        public byte myIndex = 0;
        public short duration = 0;

        public CRemoteServerPeer(CUserToken token, int dummyindex)
        {
            this.token = token;
            this.token.set_peer(this);
            this.myID = 0;
            this.duration = 0;
            this.DummyIndex = dummyindex;
        }

        // [Setter/Getter] 캡슐화
        public void SetMyId(int ID) { myID = ID; }
        public int GetMyId() { return myID; }

        // ---------------------------------------------------------
        // [핵심 수정] 패킷 수신부 (Receive)
        // ---------------------------------------------------------
        void IPeer.on_message(Const<byte[]> buffer)
        {
            short protocolid = BitConverter.ToInt16(buffer.Value, 2);
            CPacket msg = CPacket.create(protocolid);
            msg.is_RecvPacket = true;
            //Console.WriteLine($"메시지 받음 {(PacketID)protocolid}");
            msg.overwrite(buffer.Value, buffer.Value.Length);
            msg.SetOwner(this);
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

        public int Get_MyID()
        {
            return DummyIndex;
        }
    }


}

