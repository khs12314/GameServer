using NetWorkModule;
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
        Dictionary<int, GameUser_Session> game_users;
        public int Room_ID;
        static int Room_ID_Generator = 0;
        object _lock;
        public SnakeGame_Logic()
        {
            game_users = new Dictionary<int, GameUser_Session>();
            Room_ID = System.Threading.Interlocked.Increment(ref Room_ID_Generator);
            _lock = new object();
        }
        public void Enter_Game(User_Session user, int userid)
        {
            GameUser_Session gameuser = new GameUser_Session(user.CUserToken, userid);

            lock (game_users)
            {
                if (!game_users.ContainsKey(user.Uid))
                {
                    game_users.Add(gameuser.GameUser_Session_Info.userID, gameuser);
                    gameuser.Enter_Room(this);
                }
            }
            Console.WriteLine($"게임 룸 생성 및 게임 접속 완료, 룸 아이디{Room_ID}");
        }
        public void HandleMove(GameUser_Session session,C_Move packet)
        {
            Console.WriteLine($"{session.GameUser_Session_Info.userID}");
            switch (packet.direction)
            {
                case 0:
                    game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionY += 1.0f;
                    
                    break;
                case 1:
                    game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionY -= 1.0f;
                    break;
                case 2:
                    game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionX -= 1.0f;
                    break;
                case 3:
                    game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionX += 1.0f;
                    break;
            }
            S_Move move = new S_Move();
            move.playerId = session.GameUser_Session_Info.userID;
            move.posX = game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionX;
            move.posY = game_users[session.GameUser_Session_Info.userID].GameUser_Session_Info.userPositionY;
            Broadcast(move.Write());
        }
        public void Broadcast(CPacket msg) {
            lock (_lock)
            {
                foreach (var packet in game_users)
                {
                    packet.Value.UserToken.send(msg);
                }
            }
        
        }
    }
}
