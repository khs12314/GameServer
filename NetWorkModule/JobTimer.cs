using System;
using System.Collections.Generic;
using System.Text;

namespace NetWorkModule
{
    public class JobTimer
    {
        PriorityQueue<Action, long> _pq = new PriorityQueue<Action, long>();
        object _lock = new object();

        public void PushAfter(int tickAfter, Action action)
        {
            // Environment.TickCount64는 int(약 24일 뒤 오버플로우)의 한계를 극복한 long 타입이다.
            long execTick = Environment.TickCount64 + tickAfter;

            lock (_lock)
            {
                // 데이터(Action)와 우선순위(execTick)를 함께 넣는다.
                _pq.Enqueue(action, execTick);
            }
        }

        public void Flush()
        {
            while (true)
            {
                long now = Environment.TickCount64;
                Action action = null;

                lock (_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    // 1. 맨 앞의 예약 시간을 슬쩍 본다 (Peek)
                    _pq.TryPeek(out action, out long execTick);

                    // 2. 아직 실행할 시간이 안 됐으면 바로 루프 탈출
                    if (execTick > now)
                        break;

                    // 3. 시간이 됐으면 진짜로 큐에서 뺀다 (Dequeue)
                    _pq.Dequeue();
                }

                // 뽑아낸 작업을 실행 (보통 GameRoom의 JobQueue에 Push하는 역할)
                action?.Invoke();
            }
        }
    }
}
