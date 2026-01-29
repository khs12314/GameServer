using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    class Define
    {
        public static readonly short HEADER_SIZE = 2;
    }
    //접속한 유저의 세션(인 게임은 다루지 않음)
    public class User_Session : PacketSession
    {
        public GameServer Loby { get; set; }
        public GameUser_Session player;
        public int Uid { get; private set; }
        public static int SendCount = 0;
        public static int RevCount = 0;
        // CUserToken은 이제 필요 없다.
        public User_Session() 
        { 
            this.Uid = 0; 
            SeteMySocket(GetMySocket());
        }

        public void SetUserID(int userid) { Uid = userid; }
        public void Enter_Loby(GameServer gameServer) { Loby = gameServer; }
        public void SetGameUser(GameUser_Session session)
        {
            player = session;
        }
        // 2. 루키스 엔진이 패킷 하나를 완성하면 호출해주는 지점
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // [알맹이 유지] 자네가 짠 로직 그대로 복사
            // 다만, buffer(ArraySegment)를 CPacket으로 옮기는 로직만 추가한다.
            //Interlocked.Increment(ref RevCount);
            CPacket msg = CPacket.create((short)0, buffer);
            msg.overwrite(buffer); // 아까 내가 알려준 이식용 메서드
            msg.SetOwner(this);

            // 3. 자네의 기존 JobQueue 시스템으로 던지기
            Program.game_server.Enqueue_Job(msg, this);
            //Console.WriteLine($"@@@@@@@@@@@@@@@@@@@@@받은 메시지 카운트 : {RevCount}");
        }
        public override int Get_MyID()
        {
            return this.Uid;
        }
        // 4. 나머지 이벤트들 매핑
        public override void OnConnected(EndPoint endPoint)
        {

        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Loby?.User_Disconnect(this);

        }
        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"온 샌드 {numOfBytes}");
        }
        public void Send(CPacket packet)
        {
            
            packet.Retain();
            //Interlocked.Increment(ref SendCount);
            //Console.WriteLine($"보낸 메시지 카운트{SendCount}");
            Send(packet.Close(), packet);
        }

        public override void OnUserOperation(CPacket packet)
        {
            PacketManager.Instance.OnRecvPacket(this, packet);
        }

        public override void Set_MyID(int myID)
        {
            this.Uid = myID;
        }

        // 기존 send 메서드는 PacketSession의 Send를 그대로 쓰면 된다.
    }
}

