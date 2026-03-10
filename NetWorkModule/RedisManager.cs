using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkModule
{
    public static class RedisManager
    {
        static ConnectionMultiplexer _redis;
        static IDatabase _db;
        public static void Init(string connectionString = "127.0.0.1:6379")
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(connectionString);
                _db = _redis.GetDatabase();

                TimeSpan ping = _db.Ping();
                Console.WriteLine($"[Cache] Redis ConnectionMultiplexer 초기화 및 연결");
                Console.WriteLine($"{ping}");
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cache] Redis ConnectionMultiplexer 초기화 실패: {ex.Message}");
                Console.WriteLine($"상세 에러 {ex.Message}");
                Environment.Exit(1);
            }
        }
        public static void SavePlayerState(int accountId, int hp, float x, float y, float z)
        {
            // Fail-Fast: 연결이 안 되어 있다면 로직을 타지 않는다.
            if (_db == null) return;

            string key = $"user:{accountId}:info";

            // Redis의 Hash 구조에 넣을 데이터 묶음 (Zero-Allocation 관점에서는 배열 할당이 아쉽지만, 
            // 인프라 I/O 계층에서는 이 정도 객체 풀링/할당은 Trade-off로 허용한다.)
            HashEntry[] entries = new HashEntry[]
            {
        new HashEntry("Hp", hp),
        new HashEntry("PosX", x),
        new HashEntry("PosY", y),
        new HashEntry("PosZ", z)
            };

            // [CS 핵심] 메인 쓰레드 블로킹 없음, await에 의한 ThreadPool 컨텍스트 스위칭 없음.
            // 그저 네트워크 소켓 버퍼에 "이거 저장해!" 라고 던지고(Fire) 잊어버린다(Forget).
            _db.HashSet(key, entries, CommandFlags.FireAndForget);
        }
        public static void SavePlayerPosition(int accountId, float x, float y, float z)
        {
            if (_db == null) return;
            string key = $"player:{accountId}:info";
            HashEntry[] entries = {
                new HashEntry("PosX", x),
                new HashEntry("PosY", y),
                new HashEntry("PosZ", z)
            };
            _db.HashSet(key, entries, CommandFlags.FireAndForget);
        }
        public static IDatabase Db { get { return _db; } }

        public static void SavePlayerHp(int accountId, int hp)
        {
            if (_db == null)
            {
                Console.WriteLine("null reids 저장 안됨");
                return; 
            }
            Console.WriteLine($"redis 저장 {accountId}, {hp}");
            string key = $"player:{accountId}:info";

            HashEntry[] entries = { new HashEntry("Hp", hp) };

            _db.HashSet(key, entries, CommandFlags.FireAndForget);
        }
    }
}
