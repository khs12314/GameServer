using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Util
{
    public enum SystemPacketID : short
    {
        // 일반 PacketID와 겹치지 않게 MaxValue 근처값 사용
        InternalDisconnect = short.MaxValue - 1, // 32766
    }

    // 내부 연결 해제 요청용 가짜 패킷 클래스
    public class Internal_Disconnect : IPacket
    {
        public short Protocol { get { return (short)SystemPacketID.InternalDisconnect; } }

        public void Read(CPacket packet)
        {
            // 내부 신호이므로 읽을 데이터가 없음 (필요하면 DisconnectReason 등을 추가 가능)
        }

        public CPacket Write()
        {
            // 데이터 없이 ID만 찍어서 보냄
            return CPacket.create((short)SystemPacketID.InternalDisconnect);
        }
        public void Clear()
        {
            return;
        }
    }
}
