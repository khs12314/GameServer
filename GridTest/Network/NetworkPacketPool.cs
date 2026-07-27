using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Network
{
    public static class NetworkPacketPool<T> where T : IPacket, new()
    {
        private static readonly ConcurrentQueue<T> _pool = new ConcurrentQueue<T>();
        private const int MAX_POOL_SIZE = 10000;

        public static T Pop()
        {
            // 큐에서 패킷을 하나 성공적으로 꺼냈다면(Dequeue)
            if (_pool.TryDequeue(out T packet))
            {
                // [중요] 남이 쓰던 데이터를 확실하게 백지화시키고 반환한다.
                packet.Clear();
                return packet;
            }

            // 서버 웜업 단계이거나 동접자가 폭주하여 큐가 완전히 비어있을 때만
            // 불가피하게 힙(Heap)에 새로운 객체를 할당(new)한다.
            return new T();
        }

        public static void Push(T packet)
        {
            // [메모리 방어 로직] 
            // 큐의 크기가 무한정 커져서 내부 배열이 Resizing(2배 확장)되는 대참사를 막는다.
            // 임계치를 넘었다면 큐에 넣지 않고 return 해버린다.
            // 이렇게 허공에 버려진(참조를 잃은) 패킷들은 GC 0세대가 즉시, 오버헤드 없이 청소해 버린다.
            if (_pool.Count >= MAX_POOL_SIZE)
            {
                return;
            }

            // 여유가 있다면 큐의 꼬리(Tail)에 안전하게 반납한다.
            _pool.Enqueue(packet);
        }
    }
}
