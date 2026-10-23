using GridTest.DataManagers;
using GridTest.Network;
using GridTest.Session;
using NetWorkModule;

namespace GridTest.Server
{
    
    internal class Program
    {
        public static Dictionary<int, IPeer> userlist = new Dictionary<int, IPeer>();
        public static GameServer game_server = new GameServer();
        
        static void Main(string[] args)
        {
            FileLogger.Start();
            FileLogger.Log("서버 시작");    
            
            if (PacketHandler.USE_DB)
            {
                RedisManager.Init();
                DataManager.LoadAllData();
                Console.WriteLine("DB, redis 사용 모드로 실행합니다.");
            }
            else
            {
                DataManager.LoadAllData();
                Console.WriteLine("DB, redis 미사용 모드로 실행합니다.");
            }
            CPacketBufferManger.initialize(1000000);//임시
            ThreadPool.SetMinThreads(100, 100);
            userlist = new Dictionary<int, IPeer>();
            CNetworkService service = new CNetworkService();
            service.session_created_callback += on_session_created;

            service.initialize();
            service.listen("127.0.0.1",  8000, 5000);
            Console.WriteLine("Listening....");
            //Thread timerThread = new Thread(GlobalTimerLoop);
            //timerThread.IsBackground = true;
            //timerThread.Name = "GlobalTimerThread";
            //timerThread.Start();
            Test_InventorySpillover();
            while (true)
            {
                Thread.Sleep(10);
            }

        }
        public static void Test_InventorySpillover()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine(" [System] 아이템 분할 및 Snowflake DbId 발급 테스트 시작");
            Console.WriteLine("==================================================");
            CUserToken cUserToken = new CUserToken();
            User_Session user_Session = new User_Session(cUserToken);
            Channel channel = new Channel(100);
            GameUser_Session gameUser_Session = new GameUser_Session(cUserToken,1,user_Session, channel, null);

            Player player = new Player(1, gameUser_Session,channel);
            // 1. 기획자가 우편함으로 102번 물약 200개를 꽂았다고 가정 (더미 객체)
            Item dummyReward = new Item();
            dummyReward.ObjectId = 102;  // 빨간 물약
            dummyReward.Count = 200;       // 지급 수량 (200개)
            dummyReward.MaxStack = 99;     // 최대 중첩 한도

            // 2. 인벤토리 적재 로직 격발
            List<short> changedSlots =player.Inventory.AddItem(dummyReward);

            // 3. 결과 검증 (Log 출력)
            if (changedSlots != null && changedSlots.Count > 0)
            {
                foreach (short slot in changedSlots)
                {
                    Item addedItem =player.Inventory.Slots[slot];
                    Console.WriteLine($"> [성공] 슬롯 번호: {slot:D2} | TemplateID: {addedItem.ObjectId} | 수량: {addedItem.Count:D2} | DbId: {addedItem.DbId}");
                }
            }
            else
            {
                Console.WriteLine("> [실패] 인벤토리가 가득 찼거나 에러가 발생했습니다.");
            }
            Console.WriteLine("==================================================");
        }
        //static void GlobalTimerLoop()
        //{
        //    while (true)
        //    {
        //        long startTick = Environment.TickCount64;

        //        foreach (Channel channel in game_server.ChannelManager.Values)
        //        {
        //            foreach (GameRoom_Logic room in channel.room.Values)
        //            {
        //                // 각 룸의 Job Queue에 시간 정보와 함께 Update 작업을 꽂아 넣는다
        //                room.Push(() =>
        //                {
        //                    room.Update();
        //                });
        //            }
        //        }

        //        // [핵심 튜닝] Tick Drift(시간 밀림) 현상 방어
        //        long elapsed = Environment.TickCount64 - startTick;
        //        int sleepTime = 50 - (int)elapsed;

        //        // Push 작업에 시간이 소요되었다면, 그만큼 덜 쉬어야 서버의 틱(20 TPS)이 일정하게 유지된다.
        //        if (sleepTime > 0)
        //        {
        //            System.Threading.Thread.Sleep(sleepTime);
        //        }
        //    }
        //}
        static void on_session_created(CUserToken token)
        {
            Console.WriteLine("connect");
            User_Session user = new User_Session(token);
            lock (userlist)
            {
                if (!userlist.ContainsKey(user.Uid))
                {
                    userlist.Add(user.Uid, user);
                }
            }
        }
        public static void remove_user(IPeer user)
        {
            lock (userlist)
            {
                userlist.Remove(user.Get_MyID());
                game_server.User_Disconnect(user);
                if (user is GameUser_Session gameUser_Session)
                {
                    gameUser_Session.room.LeavGame(gameUser_Session, gameUser_Session.User_session.Uid);
                }

            }
        }
    }
}
