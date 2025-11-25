using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_player_Whack_a_Mole
{
    public class Player
    {
        GameUser owner;
        public byte player_index { get; private set; }
        int score;
        public Player(GameUser user, byte player_index)
        {
            this.owner = user;
            this.player_index = player_index;
            score = 0;
        }
        public void Reset()
        {
            score = 0;
        }
        public int AddScore(int score)
        {
            this.score = ++score;
            return this.score;
        }
        //1회용 바로 반납
        public void send(CPacket msg)
        {
            this.owner.send(msg);
            CPacket.destroy(msg);
        }
        /// <summary>
        /// 사용시 사용한 메소드 내에서 반납.
        /// </summary>
        /// <param name="msg"></param>
        public void send_for_broadcast(CPacket msg) 
        {
            this.owner.send(msg);
        }
        
    }
}
