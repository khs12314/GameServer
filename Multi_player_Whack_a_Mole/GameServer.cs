using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_player_Whack_a_Mole
{
    public class GameServer
    {

        // 큐 접근용 락 (외부 스레드 <-> 로직 스레드 통신용)
        object operation_lock;
        Queue<CPacket> user_operations;

        // 로직 스레드
        Thread logic_thread;
        AutoResetEvent loop_event;

        // ------------------------------
        // 게임 로직 데이터 (싱글 스레드라 Lock 불필요)
        // --------------------------------
        public GameRoomManager room_manager { get; private set; }
        List<GameUser> matching_waiting_users;

        public GameServer()
        {
            this.operation_lock = new object();
            this.loop_event = new AutoResetEvent(false);
            this.user_operations = new Queue<CPacket>();

            this.room_manager = new GameRoomManager();
            this.matching_waiting_users = new List<GameUser>();

            this.logic_thread = new Thread(gameloop);
            this.logic_thread.Start();
        }

        void gameloop()
        {
            while (true)
            {
                CPacket packet = null;

                // 1. 큐에서 일감 꺼내기 (여기만 Lock 필요)
                lock (this.operation_lock)
                {
                    if (this.user_operations.Count > 0)
                    {
                        packet = this.user_operations.Dequeue();
                        Console.WriteLine($"디큐 버퍼 지워졌나 테스트");
                    }
                    else
                    {

                    }
                }

                // 2. 패킷 처리 (Lock 불필요 - 나 혼자 씀)
                if (packet != null)
                {
                    try
                    {
                        
                        
                        process_receive(packet);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Logic Error: {e.Message}\n{e.StackTrace}");
                    }
                    finally
                    {
                        // 다 쓴 패킷 반납 (Zero-Allocation 유지)
                        CPacket.destroy(packet);
                    }
                }
                else
                {
                    
                    // 일감이 없으면 잠듬 (CPU 절약)
                    this.loop_event.WaitOne();
                }
            }
        }

        public void enqueue_packet(CPacket packet, GameUser user)
        {
            
            lock (this.operation_lock)
            {

                Console.WriteLine($"인큐 버퍼 지워졌나 테스트");
                this.user_operations.Enqueue(packet);
               
                this.loop_event.Set(); // "일어납시오!"
            }
        }

        void process_receive(CPacket msg)
        {
            if (msg.owner == null) return;
            Console.WriteLine($"프로세스 리시브 {msg.protocol_id}");
            // ---------------------------------------------------------
            // [핵심 수정] Peek (훔쳐보기)
            // msg.pop_protocol_id()를 쓰면 커서가 이동해버림.
            // PacketManager가 또 읽어야 하므로, 여기서는 커서를 건드리지 않고 값만 읽음.
            // ---------------------------------------------------------

            // 현재 위치(Header 다음)에서 short(2byte)를 읽음
            short protocol_id = BitConverter.ToInt16(msg.buffer, msg.position);

            if (protocol_id == (short)SystemPacketID.InternalDisconnect)
            {
                // 시스템 패킷인 경우에만 팝(Pop)해서 처리 (사실 팝 안 해도 됨)
                msg.pop_protocol_id();

                GameUser user = (GameUser)msg.owner;
                this.user_disconnected(user);

                if (user.battle_room != null)
                {
                    this.room_manager.remove_room(user.battle_room);
                }
            }
            else
            {
                // 일반 패킷: 커서를 건드리지 않은 채로 GameUser -> PacketManager로 넘김
                msg.owner.process_user_operation(msg);
            }
        }

        public void matching_req(GameUser user)
        {
            // [최적화] 여기는 Logic Thread만 들어옴. Lock 제거.

            if (this.matching_waiting_users.Contains(user))
                return;

            this.matching_waiting_users.Add(user);

            if (this.matching_waiting_users.Count == 2)
            {
                // 2명 매칭 성공 -> 방 생성
                GameUser user1 = this.matching_waiting_users[0];
                GameUser user2 = this.matching_waiting_users[1];

                this.room_manager.create_room(user1, user2);

                this.matching_waiting_users.Clear();

                Console.WriteLine($"[Match] Room Created: {user1.UserID} vs {user2.UserID}");
            }
        }

        public void user_disconnected(GameUser user)
        {
            if (this.matching_waiting_users.Contains(user))
            {
                this.matching_waiting_users.Remove(user);
            }
        }
    }
}
