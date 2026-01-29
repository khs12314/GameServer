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
    
    using System.Net;
    using System.Security.Cryptography;

    public class CRemoteServerPeer : PacketSession
    {
       
        public int DummyIndex { get; private set; } // 내가 몇 번째 더미인지
        public int MyServerID { get;  set; } // 서버가 준 실제 ID
        public int CurrentDirection { get; set; } = 0;
        // [중요] PacketHandler에서 데이터를 저장할 변수들
        private int myID = 0;
        public byte myIndex = 0;
        public short duration = 0;
        public static int index_gen = 0;

        public CRemoteServerPeer()
        {
          
            this.myID = 0;
            this.duration = 0;
            this.DummyIndex = Interlocked.Increment(ref index_gen);
        }

        // [Setter/Getter] 캡슐화
        //public void SetUserID(int userid) { Uid = userid; }
        //public void Enter_Loby(GameServer gameServer) { Loby = gameServer; }

        // 2. 루키스 엔진이 패킷 하나를 완성하면 호출해주는 지점
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // [알맹이 유지] 자네가 짠 로직 그대로 복사
            // 다만, buffer(ArraySegment)를 CPacket으로 옮기는 로직만 추가한다.
            //short protocolid = BitConverter.ToInt16(buffer.Value, 2);
            //     CPacket msg = CPacket.create(protocolid);
            //     msg.overwrite(buffer.Value, buffer.Value.Length);
            //     msg.SetOwner(this);
            int idOffset = buffer.Offset + 2; // Size가 2바이트라면 +2, 아니면 0
            short protocolId = BitConverter.ToInt16(buffer.Array, idOffset);

            // 2. 자네가 원하던 대로 '진짜 ID'를 넣어서 생성
            CPacket msg = CPacket.create(protocolId);

            // 3. 데이터 덮어쓰기 (아까 내가 준 Overwrite 메서드 활용)
            // 이제 msg에는 ID도 맞고, 데이터도 다 들어간다.
            msg.overwrite(buffer);
            msg.SetOwner(this);

            // 3. 자네의 기존 JobQueue 시스템으로 던지기
            PacketManager.Instance.OnRecvPacket(this, msg);
        }
        public override int Get_MyID()
        {
            return this.myID;
        }
        // 4. 나머지 이벤트들 매핑
        public override void OnConnected(EndPoint endPoint)
        {

        }
        public override void OnDisconnected(EndPoint endPoint)
        {
           

        }
        public override void OnSend(int numOfBytes)
        {

        }
        public void Send(CPacket packet)
        {
            Console.WriteLine($"{packet.protocol_id}");
            Send(packet.Close(), packet);
        }
        public override void OnUserOperation(CPacket packet)
        {
            PacketManager.Instance.OnRecvPacket(this, packet);
        }
        public override void Set_MyID(int myID)
        {
            this.myID = myID;
        }
    }


}

