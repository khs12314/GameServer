using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
            Console.WriteLine($"샌드 로그인 유저 아이디{UserID}");
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
            short protocolId = BitConverter.ToInt16(buffer.Value, 2);
            
            
            // 2. 패킷 생성 (풀링)
            CPacket msg = CPacket.create(protocolId);
            
            // 3. [★생명줄★] 데이터 복사 (Overwrite)
            // 잡큐에 넣으면 나중에 처리되니까, 그 전에 내 버퍼로 데이터를 피신시켜야 함.
            // 이걸 안 하면 Dequeue 했을 때 데이터가 0으로 나옴.
            msg.overwrite(buffer.Value, buffer.Value.Length);
            msg.SetOwner(this);
            
            // 4. 잡큐에 투입 (Enqueue)
            // 이제 msg는 안전한 데이터(Deep Copy됨)를 가지고 있으니, 
            // 1초 뒤에 꺼내든 10초 뒤에 꺼내든 데이터가 살아있음.
            
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

        public int Get_MyID()
        {
            throw new NotImplementedException();
        }
    }
}
