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

            // [3] 파일로 내보내기 (일단은 실행 폴더인 bin/Debug/... 에 생김)
            File.WriteAllText("GenPackets.cs", fileText);

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
                        readCode += string.Format(PacketFormat.readFormat, memberName, "byte"); // bool은 byte로 처리
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
                        // 리스트는 나중에 (Phase 2) 구현
                        break;
                }
            }

            // 보기 좋게 탭(\t) 정리
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }
    }
}
