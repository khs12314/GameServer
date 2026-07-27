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

        // {0}: 패킷 이름
        // {1}: 멤버 변수 선언부
        // {2}: Read(pop) 구현부
        // {3}: Write(push) 구현부
        // {4}: Clear 구현부 <-- 추가됨
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
        CPacket packet = CPacket.create((short)PacketID.{0});
        {3}
        return packet;
    }}

    
    public void Clear()
    {{
        {4}
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

        public static string listMemberFormat =
@"public struct {0}Item
{{
    {1}
}}
public List<{0}Item> {0} = new List<{0}Item>();";

        // [리스트 Read (역직렬화) 포맷]
        // {0}: 리스트 이름 (예: Characters)
        // {1}: 루프 안에서 item에 데이터를 pop 하는 코드들 (예: item.CharacterId = packet.pop_int32();)
        public static string listReadFormat =
        @"this.{0}.Clear();
int {0}Count = packet.pop_int32();
for (int i = 0; i < {0}Count; i++)
{{
    {0}Item item = new {0}Item();
    {1}
    this.{0}.Add(item);
}}";

        // [리스트 Write (직렬화) 포맷]
        // {0}: 리스트 이름 (예: Characters)
        // {1}: 루프 안에서 item의 데이터를 push 하는 코드들 (예: packet.push(item.CharacterId);)
        public static string listWriteFormat =
        @"packet.push(this.{0}.Count);
foreach ({0}Item item in this.{0})
{{
    {1}
}}";
        // [일반 변수 Clear 포맷]
        // {0}: 변수 이름
        // {1}: 기본값 (숫자는 0, bool은 false, string은 null 등)
        public static string clearFormat =
@"this.{0} = {1};";

        // [리스트 Clear 포맷]
        // {0}: 리스트 이름
        public static string listClearFormat =
@"this.{0}.Clear();";
    }

}