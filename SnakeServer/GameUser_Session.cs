using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    struct GameUser_Session_Info
    {
        public int userIndex;
        public string userID;
        public string userNickName;
        public int userScore;
        public float userPositionX;
        public float userPositionY;
        
    }
    //인게임 유저 세션
    public class GameUser_Session : IPeer
    {
        //인게임에서 유저가 사용할 정보 혹은 필요한 정보
        GameUser_Session_Info GameUser_Session_Info;
        //통신을 위한 필수 토큰
        CUserToken UserToken;
        public GameUser_Session(CUserToken token)
        {
            UserToken = token;
            UserToken.set_peer(this);
        }

        void IPeer.disconnect()
        {
            throw new NotImplementedException();
        }

        void IPeer.on_message(Const<byte[]> buffer)
        {
            throw new NotImplementedException();
        }

        void IPeer.on_removed()
        {
            throw new NotImplementedException();
        }

        void IPeer.process_user_operation(CPacket msg)
        {
            throw new NotImplementedException();
        }

        void IPeer.send(CPacket msg)
        {
            throw new NotImplementedException();
        }
    }
}
