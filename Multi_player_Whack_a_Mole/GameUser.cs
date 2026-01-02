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
        private static int _globalPlayerIdCounter = 0;
        public int UserID { get; private set; }
        static int count;
        public CGameRoom_Whack_a_Mole battle_room { get; set; }
        Player player;
        public GameUser(CUserToken token)
        {

            this.token = token;
            this.token.set_peer(this);
            this.UserID = 0;
            count = 0;
        }
        void SendLoginOk()
        {
            CPacket msg = CPacket.create((short)PROTOCOL.LOGIN_OK);
            msg.push(this.UserID); // 문자열(String) 밀어넣기
            send(msg);

        }
        public int GetID()
        {
            return this.UserID;
        }
        void IPeer.on_message(Const<byte[]> buffer)
        {
            CPacket msg = CPacket.create(0);


            msg.overwrite(buffer.Value, buffer.Value.Length);

            msg.reset_position();

            msg.move_position(Define.HEADER_SIZE);

            msg.SetOwner(this);

            Program.game_main.enqueue_packet(msg, this);

        }
        void IPeer.on_removed()
        {

            //
            CPacket msg = CPacket.create((short)PROTOCOL.INTERNAL_DISCONNECT);
            msg.SetOwner(this);
            Program.game_main.enqueue_packet(msg, this);
        }
        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);
        }
        void IPeer.process_user_operation(CPacket msg)
        {
            //Interlocked.Increment(ref count);
            //Console.WriteLine($"{protocol}");
            PakcetManager.Instance.OnRecvPacket(this, msg);
            //PROTOCOL protocol = (PROTOCOL)protocolID;
            //switch (protocol)
            //{
            //    case PROTOCOL.ENTER_GAME_ROOM_REQ:
            //        this.UserID = Interlocked.Increment(ref _globalPlayerIdCounter);

            //        SendLoginOk();
            //        Program.game_main.matching_req(this);

            //        break;

            //    // 2. 로딩 완료 (방에 들어간 후)
            //    case PROTOCOL.LOADING_COMPLETED:
            //        if (this.battle_room != null)
            //        {
            //            this.battle_room.loading_complete(this.player);
            //        }

            //        break;

            //    // 3. 두더지 타격 (게임 진행 중)
            //    case PROTOCOL.HIT_MOLE_REQ:
            //        // [주의] 패킷 구조에 맞춰서 데이터를 순서대로 꺼내야 합니다.
            //        // 예: { int HitIndex, string UserID } 라고 가정
            //        int hitIndex = msg.pop_int32();
            //        int userId = msg.pop_int32();
            //        //Console.WriteLine($"{count}");
            //        if (this.battle_room != null)
            //        {
            //            // 방으로 로직 위임 (GameRoom은 싱글 스레드 안전지대)
            //            this.battle_room.HandleHit(this, hitIndex, userId);
            //        }


            //        break;

            //    default:
            //        Console.WriteLine($"Unknown Protocol: {protocol}");
            //        break;
            //}

        }
        public void send(CPacket msg)
        {
            this.token.send(msg);
        }
        public void enter_room(Player player, CGameRoom_Whack_a_Mole room)
        {
            this.player = player;
            this.battle_room = room;
        }

    }
}
