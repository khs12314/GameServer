using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Multi_player_Whack_a_Mole
{
    public class CGameRoom_Whack_a_Mole
    {
        //게임 룸 내부를 다루는 클래스
        public enum PLAYER_STATE
        {
            ENTER_ROOM,
            LOADING_COMPLETE,
        }

        Random rand = null;
        List<GameUser> _users = new List<GameUser>();


        Timer _moleTimer;


        int _currentMoleIndex = -1;//0~9 -1이면 없음
        //기존에는 bool로 설계하였지만 lock-free 방식 도입을 통해 성능 최적화를 위해 Interlocked.Decremnet를 사용해 생존해 있는 두더지는 1, 아닌 두더지는 0으로 제어
        int _isMoleAlive = 0;

        //동기화 객체
        object _lock = new object();

        //플레이어 관리
        Player[] players;
        //플레이어들의 상태 관리
        Dictionary<byte, PLAYER_STATE> player_state;
        //두더지 잡기 보드판
        short[] gameboard;
        //0~25까지의 인덱스를 갖고 있는 보드판 데이터
        short[] table_board;
        //타이머
        short duration;
        static byte column_count = 5;
        readonly short EMPTY_SLOT = short.MaxValue;

        public CGameRoom_Whack_a_Mole()
        {
            rand = new Random();
            players = new Player[2];
            player_state = new Dictionary<byte, PLAYER_STATE>();
            duration = 60;
            gameboard = new short[9];
            table_board = new short[9];
            //보드 초기화
            for (int i = 0; i < 9; i++)
            {
                gameboard[i] = 0;
                table_board[i] = 0;
            }

            _moleTimer = new Timer(SpawnMole, null, 0, 3000);
            
        }

        public void destroy()
        {
            CPacket msg = CPacket.create((short)PROTOCOL.ROOM_REMOVED);
            broadcast(msg);
            Array.Clear(players);
        }
        void SpawnMole(object state)
        {

            _currentMoleIndex = rand.Next(0, 9);

            //1. 랜덤 위치 결정(0~9)
            Interlocked.Increment(ref _isMoleAlive);
            //2 패킷 생성 브로드 캐스트 구현
            CPacket msg = CPacket.create((short)PROTOCOL.SPAWN_MOLE);
            msg.push(_currentMoleIndex);
            broadcast(msg);
        }
        //메시지 전송
        /// <summary>
        /// 브로드캐스트
        /// </summary>
        /// <param name="msg"></param>
        void broadcast(CPacket msg)
        {

            //참조 카운팅


            foreach (var item in players)
            {
                if (item == null) continue;
                item.send_for_broadcast(msg);
            }

            
            CPacket.destroy(msg);
            //CPacketBufferManger.get_pool_count();
        }
        void change_player_state(Player player, PLAYER_STATE state)
        {

            if (this.player_state.ContainsKey(player.player_index))
            {
                this.player_state[player.player_index] = state;
            }
            else
            {
                this.player_state.Add(player.player_index, state);
            }

        }
        //유저 행동
        bool all_player_ready(PLAYER_STATE state)
        {
            foreach (KeyValuePair<byte, PLAYER_STATE> item in this.player_state)
            {
                if (item.Value != state)
                {
                    return false;
                }
            }
            return true;
        }
        public void EnterRoom(GameUser user)
        {
            lock (_lock)
            {
                _users.Add(user);
            }
        }

        //void Spawn_Mole(object state)
        //{
        //    lock (_lock)
        //    {
        //        Random random = new Random();
        //        _currentMoleIndex = random.Next(0, 25);
        //        _isMoleAlive = true;

        //        Console.WriteLine($"Server Mole Spawned at Index {_currentMoleIndex}");

        //        CPacket msg = CPacket.create((short)PROTOCOL.SPAWN_MOLE);
        //        msg.push(_currentMoleIndex);

        //        broadcast(msg);

        //    }
        //}
        public void HandleHit(GameUser user, int hitIndex, int ID)
        {

            if (_isMoleAlive == 0) return;
            if (hitIndex != _currentMoleIndex) return;
            //Console.WriteLine($"Hit index :{hitIndex}, User Id : {ID} ");
            Interlocked.Decrement(ref _isMoleAlive);

            //점수 증가
            //MockDB.AddScore("TestUser", 10);

           
        }
        public void Enter_Game_Room(GameUser user1, GameUser user2)
        {
            //인덱스 부여
            Player player1 = new Player(user1, 0);
            Player player2 = new Player(user2, 1);
            if (players != null)
            {
                Array.Clear(players);
            }
            user1.enter_room(player1, this);
            user2.enter_room(player2, this);

            players[0] = player1;
            players[1] = player2;
            

            //상태지정
            player_state.Clear();
            change_player_state(player1, PLAYER_STATE.ENTER_ROOM);
            change_player_state(player2, PLAYER_STATE.ENTER_ROOM);
            //로딩 시작
            //Task.Run(async () =>
            //{
            //    await Task.Delay(1000);

            //});
            foreach (var item in players)
            {
                CPacket msg = CPacket.create((short)PROTOCOL.START_LOADING);
                msg.push(item.player_index);
                item.send(msg);
            }

        }

        public void loading_complete(Player player)
        {
            //해당 플레이어를 로딩완료 상태로 변경한다
            change_player_state(player, PLAYER_STATE.LOADING_COMPLETE);
            //모든 요주가 준비 상태인지 체크
            if (!all_player_ready(PLAYER_STATE.LOADING_COMPLETE))
            {
                return;
            }
            battle_start();
        }
        //유저 상태 변경
        //게임 진행 상태
        void battle_start()
        {
            //게임 시작시 초기화
            reset_gamedata();
            //시작 메시지 전송
            CPacket msg = CPacket.create((short)PROTOCOL.GAME_START);
            //타이머 시작
            //두더지 생성 이벤트 시작
            msg.push((byte)this.players.Length);
            //duration을 전체에게 send
            foreach (var item in players)
            {
                msg.push(item.player_index);
                msg.push(duration);
            }
            broadcast(msg);
        }
        void reset_gamedata()
        {
            //데이터 초기화
            Array.Clear(gameboard);
            Array.Clear(table_board);

        }



        //게임 규칙 설정


    }
}
