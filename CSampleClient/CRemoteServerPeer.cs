using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CSampleClient
{
    using GameServer;
    class CRemoteServerPeer : IPeer
    {
        public CUserToken token { get; private set; }
        int myID;
        static Random _rand = new Random();
        public CRemoteServerPeer(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
            myID = 0;
        }
        void IPeer.on_message(Const<byte[]> buufer)
        {
            CPacket msg = new CPacket(buufer.Value, this);
            PROTOCOL protocol_id = (PROTOCOL)msg.pop_protocol_id();
            Console.WriteLine($"[Client] Received from server{protocol_id}");
            switch (protocol_id) 
            { 
                case PROTOCOL.ENTER_GAME_ROOM_REQ:
                    {
                        
                        myID = msg.pop_int32();
                        
                        Console.WriteLine(string.Format("Received from server: {0}", myID));
                    }
                    break;
                case PROTOCOL.SPAWN_MOLE:
                    Int16 index = msg.pop_int16();
                    Console.WriteLine($"[Client] Mole appeared - SpawnIndex{index}");
                    Task.Run(async () =>
                    {
                        // 50ms ~ 500ms 랜덤 지연 (사람 반응 속도 + 핑 시뮬레이션)
                        int delay = _rand.Next(50, 250);

                        Console.WriteLine($"[Client] Saw mole at {index}. Reacting in {delay}ms...");
                        await Task.Delay(delay);

                        SendHitPacket(index);
                    });
                    Console.WriteLine($"[Client] Hit REQ{index}");
                    break;
                case PROTOCOL.LOGIN_OK:
                    myID = msg.pop_int32();
                    Console.WriteLine($"myid : {myID}");
                    break;
            }
        }
        void SetMyId(int ID)
        {
            myID = ID;
            Console.WriteLine($"MyID : {myID}");
        }
        void SendHitPacket(int index)
        {
            CPacket hitMsg = CPacket.create((short)PROTOCOL.HIT_MOLE_REQ);
            hitMsg.push(index);
            hitMsg.push(myID);
            Console.WriteLine($"push ID{myID}");
            this.token.send(hitMsg);
        }
        void IPeer.on_removed()
        {
            Console.WriteLine("Server removed");
        }
        void IPeer.send(CPacket msg)
        {
            this.token.send(msg);
        }
        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);    
        }
        void IPeer.process_user_operation(CPacket msg, short protocolID = 0)
        {

        }


    }
}
