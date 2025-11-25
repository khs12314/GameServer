using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            players = new Player[2];
            player_state = new Dictionary<byte, PLAYER_STATE>();
            duration = 60;
            gameboard = new short[25];
            table_board = new short[25];
            //보드 초기화
            for (int i = 0; i < 25; i++) {
                gameboard[i] = 0;
                table_board[i] = 0;
            }
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
                item.send_for_broadcast(msg);
            }
            //반납
            CPacket.destroy(msg);
        }
        void change_player_state(Player player, PLAYER_STATE state) {

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
            foreach(KeyValuePair<byte, PLAYER_STATE> item in this.player_state)
            {
                if(item.Value != state)
                {
                    return false;
                }
            }
            return true;
        }
        public void Enter_Game_Room(GameUser user1, GameUser user2)
        {
            //인덱스 부여
            Player player1 = new Player(user1 as GameUser, 0);
            Player player2 = new Player(user2 as GameUser, 1);
            Array.Clear(players);
            players[0] = player1;
            players[1] = player2;

            //상태지정
            player_state.Clear();
            change_player_state(player1, PLAYER_STATE.ENTER_ROOM);
            change_player_state(player2, PLAYER_STATE.ENTER_ROOM);
            //로딩 시작
            foreach (var item in players)
            {
                CPacket msg = CPacket.create((short)PLAYER_STATE.LOADING_COMPLETE);
                msg.push(item.player_index);
                item.send(msg);
            }
            user1.enter_room(player1, this);
            user2.enter_room(player2, this);

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
