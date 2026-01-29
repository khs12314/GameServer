using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkModule
{
    public class CPacketBufferManger
    {
        static object cs_buffer = new object();//오브젝트로 선언된걸 봐선 lock을 위한 객체
        static Stack<CPacket> pool;//여러 스레드에서 접근할 수 있다.
        static int pool_capacity;
        static int popcount;
        static int pushCount;
        static CPacketBufferManger()
        {
            pool = new Stack<CPacket>();
            // 서버 시작 시 1000개 미리 생산 (이때만 할당 발생)
            //for (int i = 0; i < 100000; i++)
            //    pool.Push(new CPacket());
            popcount= 0;
            pushCount= 0;
        }
        public static void get_pool_count()
        {
            Console.WriteLine($"Pool Count : {pool.Count}");
        }
        public static void initialize(int capacity)
        {
            pool = new Stack<CPacket>();
            pool_capacity = capacity;
            allocate();
        }
        static void allocate()
        {
            for (int i = 0; i < pool_capacity; i++)
            {
                pool.Push(new CPacket());
            }
        }
        public static CPacket pop()
        {
            //여러 스레드에서 접근할 수 있으니 lock처리
            lock (cs_buffer)
            {
                if (pool.Count == 0)
                {
                    Console.WriteLine("Pool Count : 0");
                    allocate();
                }
                Interlocked.Increment(ref popcount);
                
                return pool.Pop();
            }
        }
        public static void push(CPacket packet)
        {
            string killerTrace = Environment.StackTrace;

            // 현재 스레드 ID까지 찍어라. (같은 놈이 두 번 죽이는지, 다른 놈이 죽이는지)
            int threadId = Thread.CurrentThread.ManagedThreadId;
            lock (cs_buffer)
            {
                
                pool.Push(packet);
                Interlocked.Increment(ref pushCount);
                //Console.WriteLine($"풀카운트{pool.Count}, 팝카운트{popcount}, 푸시카운트 {pushCount}");
                //Console.WriteLine($"[push DETECTED] Thread:{threadId}, \n{killerTrace}");
                if(pool.Count > pool_capacity)
                {
                    //Console.WriteLine("죽음");
                    //Console.WriteLine($"[push DETECTED] Thread:{threadId}, \n{killerTrace}");
                }
            }
        }
    }
}
