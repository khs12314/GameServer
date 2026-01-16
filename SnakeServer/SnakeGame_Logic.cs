using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    //인게임 로직 처리
    public class SnakeGame_Logic
    {
        //여기는 인게임 유저를 다루는 딕셔너리 로비의 딕셔너리 키와 같다. 로그인 후 제공받은 고유 키가 여기서도 동일하게 적용된다
        Dictionary<int , GameUser_Session> game_users;
        public int Room_ID;
        static int Room_ID_Generator = 0;
        public SnakeGame_Logic()
        {
            game_users = new Dictionary<int, GameUser_Session>();
            Room_ID = System.Threading.Interlocked.Increment(ref Room_ID_Generator);
        }
        public void Enter_Game(User_Session user, int userid)
        {
            GameUser_Session gameuser = new GameUser_Session(user.CUserToken, userid);
            
            lock (game_users)
            {
                if (!game_users.ContainsKey(user.Uid))
                {
                    game_users.Add(gameuser.GameUser_Session_Info.userID, gameuser);
                }
            }
            Console.WriteLine($"게임 룸 생성 및 게임 접속 완료, 룸 아이디{Room_ID}");
        }
    }
}
