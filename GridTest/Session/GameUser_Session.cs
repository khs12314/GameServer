using GridTest.Manager;
using GridTest.Room;
using GridTest.Server;
using GridTest.Util;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Session
{
    //public struct GameUser_Session_Info
    //{
    //    public int userIndex;
    //    public int userID;
    //    public string userNickName;
    //    public int CellX;
    //    public int CellY;

    //}
   
    //인게임 유저 세션
    public class GameUser_Session : IPeer
    {
        //인게임에서 유저가 사용할 정보 혹은 필요한 정보
        //public GameUser_Session_Info GameUser_Session_Info;
        public Player MyPlayer { get; set; }
        public struct Pos { public float x, z; }
        public LinkedList<Pos> BodyList;
        public bool Is_Alive;
        //구조변경
        //통신을 위한 필수 토큰
        public CUserToken UserToken;
        public User_Session User_session;
        public GameRoom_Logic room {  get; set; }
        public GameUser_Session(CUserToken token, int id, User_Session user_session, Channel channel, GameRoom_Logic room)
        {
            UserToken = token;
            //GameUser_Session_Info = new GameUser_Session_Info();
            //GameUser_Session_Info.userID = id;
            
            UserToken.set_peer(this);
            BodyList = new LinkedList<Pos>();
            Is_Alive = true;
            User_session = user_session;
            //GameUser_Session_Info.CellX = 0;
            //GameUser_Session_Info.CellY = 0;
            MyPlayer = ObjectPool<Player>.Pop();
            MyPlayer.Initialize(User_session.Uid,this, channel);
            this.room = room;


        }
        int IPeer.Get_MyID()
        {
            return MyPlayer.PlayerID;
        }
        void IPeer.disconnect()
        {
            Task.Run(async() =>
            await DBMananger.SavePlayerToDBAsync(MyPlayer.Id)
            );
            Console.WriteLine("유저 연결 끊김");
            room?.LeavGame(this, MyPlayer.PlayerID);
            
        }

        void IPeer.on_message(Const<byte[]> buffer)
        {
            short protocolid = BitConverter.ToInt16(buffer.Value, 2);
            CPacket msg = CPacket.create(protocolid);
            msg.is_RecvPacket = true;
            //Console.WriteLine($"메시지 받음 {(PacketID)protocolid}");
            msg.overwrite(buffer.Value, buffer.Value.Length);
            msg.SetOwner(this);
            if(msg.GetRefCount() < 0)
            {
                Console.WriteLine("레프카운트");
            }
            Program.game_server.Enqueue_Job(msg, UserToken);
        }

        void IPeer.on_removed()
        {
            Program.remove_user(this);
        }

        void IPeer.process_user_operation(CPacket msg)
        {
            
            if (msg.GetRefCount() < 0)
            {
                Console.WriteLine("레프카운트");
            }
            PacketManager.Instance.OnRecvPacket(this, msg);
            //msg.Release();

        }

       public void send(CPacket msg)
        {
            msg.is_sendPacket = true;
            msg.Retain();
            //Console.WriteLine($"보내는 패킷 아이디{msg.protocol_id}");
            UserToken.send(msg);
            
        }
        public void Enter_Room(GameRoom_Logic room)
        {
            this.room = room;
        }
        public void Change_Peer()
        {
            Console.WriteLine("피어 교체");
            User_session.CUserToken.set_peer(User_session);
        }
    }
}
