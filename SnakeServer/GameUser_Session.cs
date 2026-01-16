using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    public struct GameUser_Session_Info
    {
        public int userIndex;
        public int userID;
        public string userNickName;
        public int userScore;
        public float userPositionX;
        public float userPositionY;

    }
    //인게임 유저 세션
    public class GameUser_Session : IPeer
    {
        //인게임에서 유저가 사용할 정보 혹은 필요한 정보
        public GameUser_Session_Info GameUser_Session_Info;
        //통신을 위한 필수 토큰
        public CUserToken UserToken;
        public SnakeGame_Logic room {  get; set; }
        public GameUser_Session(CUserToken token, int id)
        {
            UserToken = token;
            GameUser_Session_Info = new GameUser_Session_Info();
            GameUser_Session_Info.userID = id;
            UserToken.set_peer(this);
        }

        void IPeer.disconnect()
        {
            throw new NotImplementedException();
        }

        void IPeer.on_message(Const<byte[]> buffer)
        {
            Console.WriteLine("메시지 받음");
            short protocolid = BitConverter.ToInt16(buffer.Value, 2);
            CPacket msg = CPacket.create(protocolid);
            msg.overwrite(buffer.Value, buffer.Value.Length);
            msg.SetOwner(this);

            Program.game_server.Enqueue_Job(msg, this.UserToken);
        }

        void IPeer.on_removed()
        {
            Internal_Disconnect packet = new Internal_Disconnect();

            CPacket msg = packet.Write();
            msg.SetOwner(this);

            Program.game_server.Enqueue_Job(msg, this.UserToken);
        }

        void IPeer.process_user_operation(CPacket msg)
        {
            PacketManager.Instance.OnRecvPakcet(this, msg);
        }

        void IPeer.send(CPacket msg)
        {
            this.UserToken.send(msg);
        }
        public void Enter_Room(SnakeGame_Logic room)
        {
            this.room = room;
        }
    }
}
