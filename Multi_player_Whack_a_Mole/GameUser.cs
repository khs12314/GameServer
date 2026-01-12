using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetWorkModule;
namespace Multi_player_Whack_a_Mole
{
    class Define
    {
        public static readonly short HEADER_SIZE = 2;
    }
    public class GameUser : IPeer
    {

        CUserToken token;

        // ID 발급용 정적 변수 (Thread-Safe하게 관리 필요)
        private static int _globalPlayerIdCounter = 0;

        public int UserID { get; private set; }

        // GameRoom, Player 참조
        public CGameRoom_Whack_a_Mole battle_room { get; set; }
        public Player MyPlayer { get; private set; } // Property 간소화

        public GameUser(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
            this.UserID = 0;
        }

        // ----------------------------------------------------------------
        // [수정 1] 로그인 OK 전송 (GenPackets 활용)
        // ----------------------------------------------------------------
        public void SendLoginOk()
        {
            S_LoginOk packet = new S_LoginOk();

            // Interlocked를 사용하여 멀티스레드 환경에서 안전하게 ID 증가
            this.UserID = Interlocked.Increment(ref _globalPlayerIdCounter);
            packet.ServerUserID = this.UserID;

            // Write()가 반환하는 CPacket을 전송 (Zero-Allocation)
            send(packet.Write());
        }

        public int GetID()
        {
            return this.UserID;
        }

        // ----------------------------------------------------------------
        // [수정 2] 핵심: Zero-Copy 수신 처리
        // ----------------------------------------------------------------
        void IPeer.on_message(Const<byte[]> buffer)
        {
            // [Bad] 기존 코드: msg.overwrite(...) -> 메모리 복사 발생! 성능 저하 원인.

            // [Good] 버퍼를 감싸기만 함 (Wrapping)
            // CPacket 생성자 중 (byte[], IPeer)를 받는 버전을 사용하십시오.
            // *주의: 이 생성자는 내부적으로 '참조'만 복사하므로 성능 비용이 거의 0입니다.
            CPacket msg = new CPacket(buffer.Value, this);
            Console.WriteLine($"온 메시지{msg.position}");
            //msg.protocol_id = (short)PacketID.C_Login;
            Console.WriteLine($"온 메시지 프로토콜 아이디 {msg.protocol_id}");
            // 큐에 넣어서 처리 (GameServer 로직 스레드가 가져감)
            Program.game_main.enqueue_packet(msg, this);
        }

        // ----------------------------------------------------------------
        // [수정 3] 연결 종료 처리 (System Packet 활용)
        // ----------------------------------------------------------------
        void IPeer.on_removed()
        {
            // 시스템 패킷 생성
            Internal_Disconnect packet = new Internal_Disconnect();

            CPacket msg = packet.Write();
            msg.SetOwner(this);

            Program.game_main.enqueue_packet(msg, this);
        }

        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);
        }

        // ----------------------------------------------------------------
        // [수정 4] 로직 처리 (PacketManager 위임)
        // ----------------------------------------------------------------
        void IPeer.process_user_operation(CPacket msg)
        {
            // [청소 완료] 
            // 더럽게 남아있던 PROTOCOL switch문을 전부 지웠습니다.
            // 이제 모든 처리는 PacketManager가 담당합니다.
            Console.WriteLine($"패킷 포지션{msg.position}");
            Console.WriteLine($"패킷 아이디 오퍼레이션{msg.protocol_id}");
            PacketManager.Instance.OnRecvPacket(this, msg);
        }

        // ----------------------------------------------------------------
        // Helper Methods
        // ----------------------------------------------------------------
        public void send(CPacket msg)
        {
            Console.WriteLine("보내기");
            this.token.send(msg);
        }

        public void enter_room(Player player, CGameRoom_Whack_a_Mole room)
        {
            this.MyPlayer = player; // Setter 수정
            this.battle_room = room;
        }

    }
}
