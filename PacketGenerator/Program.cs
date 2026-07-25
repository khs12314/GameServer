using System.Xml;

namespace PacketGenerator
{
    internal class Program
    {
        // 1. 결과물이 담길 변수들
        static string genPackets; // 클래스 코드들이 여기 다 합쳐짐
        static ushort packetId;   // 패킷 번호 (0부터 자동 증가)
        static string packetEnums; // "PlayerInfoReq = 0," 같은 Enum 정의
        static void Main(string[] args)
        {
            string pdlPath = "PDL.xml";

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true, // 주석 무시
                IgnoreWhitespace = true // 공백 무시
            };

            // [1] XML 파일 읽기 시작
            using (XmlReader r = XmlReader.Create(pdlPath, settings))
            {
                r.MoveToContent();

                while (r.Read())
                {
                    // <Packet> 태그를 발견하면 파싱 시작
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                }
            }

            // [2] Format의 구멍({0}, {1})에 파싱한 내용 끼워 넣기
            string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);

            string[] outputPaths =
 {
    @"C:\Users\qpqpq\OneDrive\문서\GitHub\mmo_unity\My project (33)\Assets\Scripts\Application\GenPackets.cs",
    @"C:\Users\qpqpq\source\repos\NetWorkModule\GridTest\Network\GenPackets.cs"
};

            foreach (string path in outputPaths)
            {
                File.WriteAllText(path, fileText);
            }

            Console.WriteLine("----------------------------------");
            Console.WriteLine(" [Success] GenPackets.cs 생성 완료!");
            Console.WriteLine("----------------------------------");
        }

        // <Packet> ... </Packet> 블록 하나를 처리하는 함수
        static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement) return;
            if (r.Name.ToLower() != "packet") return;

            string packetName = r["name"]; // 예: "PlayerInfoReq"
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Error: Packet name is missing");
                return;
            }

            // [핵심] 내부 멤버 변수들(<long>, <string> 등) 파싱해오기
            // t.Item1 = 변수 선언 코드
            // t.Item2 = Read(pop) 코드
            // t.Item3 = Write(push) 코드
            Tuple<string, string, string> t = ParseMembers(r);

            // 포맷에 끼워 넣기
            string newPacket = string.Format(PacketFormat.packetFormat,
                packetName, t.Item1, t.Item2, t.Item3);

            // 결과물에 누적
            genPackets += newPacket;

            // Enum 등록 (번호 1 증가)
            packetEnums += string.Format("{0} = {1},\n\t", packetName, packetId++);
        }

        // 패킷 안의 멤버들 (<int>, <float> 등) 처리
        static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = ""; // public int age;
            string readCode = "";   // this.age = packet.pop_int();
            string writeCode = "";  // packet.push(this.age);

            int depth = r.Depth + 1; // <Packet>보다 한 단계 깊은 곳

            while (r.Read())
            {
                if (r.Depth != depth) break; // </Packet> 만나면 종료

                string memberName = r["name"]; // 예: "playerId"
                if (string.IsNullOrEmpty(memberName)) continue;

                string memberType = r.Name.ToLower(); // 예: "long"

                // [중요] XML 타입 <-> 자네 CPacket 메서드 매핑
                // 자네 CPacket에 있는 메서드 이름(pop_int32 등)과 정확히 같아야 함!
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, "byte");
                        writeCode += string.Format(PacketFormat.writeFormat, memberName);
                        break;
                    case "bool":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, "bool"); // bool은 byte로 처리
                        writeCode += string.Format(PacketFormat.writeFormat, memberName);
                        break;
                    case "short":
                    case "ushort":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, "int16"); // pop_int16
                        writeCode += string.Format(PacketFormat.writeFormat, memberName);
                        break;
                    case "int":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, "int32"); // pop_int32
                        writeCode += string.Format(PacketFormat.writeFormat, memberName);
                        break;
                    case "long":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, "int64"); // pop_int64
                        writeCode += string.Format(PacketFormat.writeFormat, memberName);
                        break;
                    case "float":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, "float"); // pop_float
                        writeCode += string.Format(PacketFormat.writeFormat, memberName);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, "string"); // pop_string
                        writeCode += string.Format(PacketFormat.writeFormat, memberName);
                        break;
                    case "list":
                        // 1. 리스트의 자식 노드들을 파싱해오는 전용 함수 호출
                        Tuple<string, string, string> t = ParseList(r);

                        // 2. 아까 PacketFormat.cs에 추가했던 리스트 전용 템플릿에 데이터 삽입
                        // t.Item1: 구조체 안에 들어갈 변수들
                        // t.Item2: 역직렬화(Read) 루프 안의 pop 코드들
                        // t.Item3: 직렬화(Write) 루프 안의 push 코드들
                        memberCode += string.Format(PacketFormat.listMemberFormat, memberName, t.Item1);
                        readCode += string.Format(PacketFormat.listReadFormat, memberName, t.Item2);
                        writeCode += string.Format(PacketFormat.listWriteFormat, memberName, t.Item3);
                        break;
                }
            }

            // 보기 좋게 탭(\t) 정리
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }
        static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1; // <list>보다 한 단계 깊은 곳

            while (r.Read())
            {
                if (r.Depth != depth) break; // </list> 만나면 종료

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName)) continue;

                string memberType = r.Name.ToLower();

                // [핵심] 리스트 안에서는 this. 가 아니라 item. 을 사용해야 한다!
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format("public {0} {1};\n", memberType, memberName);
                        readCode += string.Format("item.{0} = packet.pop_byte();\n", memberName);
                        writeCode += string.Format("packet.push(item.{0});\n", memberName);
                        break;
                    case "int":
                        memberCode += string.Format("public {0} {1};\n", memberType, memberName);
                        readCode += string.Format("item.{0} = packet.pop_int32();\n", memberName);
                        writeCode += string.Format("packet.push(item.{0});\n", memberName);
                        break;
                    case "string":
                        memberCode += string.Format("public {0} {1};\n", memberType, memberName);
                        readCode += string.Format("item.{0} = packet.pop_string();\n", memberName);
                        writeCode += string.Format("packet.push(item.{0});\n", memberName);
                        break;
                    case "float":
                        memberCode += string.Format("public {0} {1};\n", memberType, memberName);
                        readCode += string.Format("item.{0} = packet.pop_float();\n", memberName);
                        writeCode += string.Format("packet.push(item.{0});\n", memberName);
                        break;
                        // 필요하다면 short, long, float 등도 위 패턴에 맞춰 똑같이 추가해라!
                }
            }

            // 보기 좋게 들여쓰기 탭(\t) 정리
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }
    }
}
