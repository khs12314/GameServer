using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    class PacketFormat
    {
        // {0}: 패킷 종류(Enum)
        // {1}: 패킷 클래스들의 실제 구현 코드
        public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using NetWorkModule;

public enum PacketID
{{
    {0}
}}

// 공통 인터페이스
public interface IPacket
{{
	short Protocol {{ get; }}
	void Read(CPacket packet);
	CPacket Write();
}}

{1}
";

        // {0}: 패킷 이름 (예: PlayerInfoReq)
        // {1}: 멤버 변수 선언부
        // {2}: Read(pop) 구현부
        // {3}: Write(push) 구현부
        public static string packetFormat =
@"
public class {0} : IPacket
{{
    {1}

    public short Protocol {{ get {{ return (short)PacketID.{0}; }} }}

    public void Read(CPacket packet)
    {{
        {2}
    }}

    public CPacket Write()
    {{
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.{0});
        {3}
        return packet;
    }}
}}
";
        // {0}: 변수 타입 (int, float...)
        // {1}: 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        // {0}: 변수 이름
        // {1}: 타입별 메서드명 (int32, string 등) -> pop_{1}() 호출
        public static string readFormat =
@"this.{0} = packet.pop_{1}();";

        // {0}: 변수 이름
        // {1}: (사용 안 함, 확장성 위해 남겨둠)
        public static string writeFormat =
@"packet.push(this.{0});";

    }
}