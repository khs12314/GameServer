using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using NetWorkModule;

namespace CSampleServer
{

    using GameServer;

    ///<summary>
    ///하나의 session 객체를 나타낸다
    ///</summary>

    //CUserToken클래스와 CGameUser 클래스를 나눈 이유
    //CUserToken은 네트워크 모듈에 가까운 클래스로 소켓 api에 가깝운 설계이고
    //CGameUser 클래스는 애플리케이션 로직에 가까워 서버마다 각기 다른 내용으로 채워질 수 있다.
    class CGameUser : IPeer
    {
        CUserToken token;//1

        public CGameUser(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);  
        }
        
        void IPeer.on_message(Const<byte[]> buffer)//2
        {
            // ex)
            CPacket msg = new CPacket(buffer.Value, this);

            PROTOCOL protocol = (PROTOCOL)msg.pop_protocol_id();
            Console.WriteLine("---------------------------------------");
            Console.WriteLine( "protocol id " + protocol);
            switch (protocol)
            {
                case PROTOCOL.CHAT_MSG_REQ://3
                    {
                        string text = msg.pop_string();
                        Console.WriteLine(string.Format("text{0}", text));

                        CPacket response = CPacket.create((short)PROTOCOL.CHAT_MSG_ACK);
                        response.push(text);
                        send(response);
                    }
                    break;
            }
        }

        void IPeer.on_removed()
        {
            Console.WriteLine("The client disconnected.");
            Program.remove_user(this);
        }
        public void send(CPacket msg)
        {
            this.token.send(msg);
        }
        void IPeer.disconnect()//4
        {
            this.token.socket.Disconnect(false);    
        }
        void IPeer.process_user_operation(CPacket msg, short protocolID)
        {

        }
        

    }
}
