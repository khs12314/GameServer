using System;
using System.Collections.Generic;
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
                    allocate();
                }
                return pool.Pop();
            }
        }
        public static void push(CPacket packet)
        {
            lock (cs_buffer)
            {
                pool.Push(packet);
            }
        }
    }
}
