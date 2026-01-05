using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_player_Whack_a_Mole
{
    public class GameServer
    {

        object operation_lock;
        Queue<CPacket> user_operations;

        //로직은 하나의 스레드로만 처리한다.
        Thread locgic_thread;
        AutoResetEvent loop_event;

        //------------------------------
        //게임 로직 처리 관련 변수들
        //--------------------------------
        //게임방을 관리하는 매니저
        public GameRoomManager room_manager { get; private set; }
        //매칭 대기 리스트
        List<GameUser> matching_waiting_users;

        public GameServer()
        {
            this.operation_lock = new object();
            this.loop_event = new AutoResetEvent(false);
            this.user_operations = new Queue<CPacket>();
            //게임 로직 관련
            this.room_manager = new GameRoomManager();
            this.matching_waiting_users = new List<GameUser>();

            this.locgic_thread = new Thread(gameloop);
            this.locgic_thread.Start();
        }


        void gameloop()
        {

            while (true)
            {

                CPacket packet = null;
                lock (this.operation_lock)
                {
                    if (this.user_operations.Count > 0)
                    {
                        packet = this.user_operations.Dequeue();
                    }
                }
                if (packet != null)
                {
                    try
                    {
                        // 여기서 에러 터지면 catch로 가거나 스레드가 멈춤 -> destroy 안 됨 -> Leak
                        process_receive(packet);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Logic Error: {e.Message}");
                    }
                    finally
                    {
                        
                        CPacket.destroy(packet);
                    }
                }

                //더이성 처리할 패킷이 없으면 스레드 대기

                if (this.user_operations.Count <= 0)
                {
                    this.loop_event.WaitOne();
                }

            }
        }

        public void enqueue_packet(CPacket packet, GameUser user)
        {

            lock (this.operation_lock)
            {
                this.user_operations.Enqueue(packet);
                this.loop_event.Set();
            }

        }

        void process_receive(CPacket msg)
        {
            if (msg.owner == null)
            {

                CPacket.destroy(msg);
                return;
            }
            //todo
            //user msg fillter 체크

            short protocol_id = msg.pop_protocol_id();
            if (protocol_id == (short)PROTOCOL.INTERNAL_DISCONNECT)
            {
                GameUser user = (GameUser)msg.owner;
                //대기열에 있었다면 제거
                this.user_disconnected(user);
                //게임방에 있었다면 게임방에서 제거
                //내부 로직도 싱글 스레드라 lock 불필요
                if (user.battle_room != null)
                {
                    this.room_manager.remove_room(user.battle_room);
                }
            }
            else
            {
                //3 일반이면 하던대로
                msg.owner.process_user_operation(msg);
            }
        }


        /// <summary>
        /// 유저로부터 매칭 요청이 왔을 때 호출됨
        /// </summary>
        /// <param name="user">매칭을 신청한 유저 객체</param>
        public void matching_req(GameUser user)
        {

            //대기 리스트에 중복 추가되지 않도록 체크.
            if (this.matching_waiting_users.Contains(user))
            {
                return;
            }
            //매칭 대기 리스트에 추가.
            this.matching_waiting_users.Add(user);
            //2명이 모이면 매칭 성공
            if (this.matching_waiting_users.Count == 2)
            {
                lock (this.operation_lock)
                {

                    //게임 방 생성
                    this.room_manager.create_room(this.matching_waiting_users[0], this.matching_waiting_users[1]);
                    //매칭 대기 리스트 삭제
                    this.matching_waiting_users.Clear();
                }

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
