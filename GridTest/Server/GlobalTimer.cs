using GridTest.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Server
{
    public static class GlobalTimer
    {
        public static void Start()
        {
            Thread timerThread = new Thread(TimerLoop);
            timerThread.IsBackground = true;
            timerThread.Start();
        }

        private static void TimerLoop()
        {
            
            int tickMs = 50; // 20 TPS
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (true)
            {
                long start = sw.ElapsedMilliseconds;

                // 1. O(1) Zero-Allocation Push (방 단위 완벽한 병렬 처리)
                foreach (var channel in ChannelManager.GetAllChannels())
                {
                    foreach (var room in channel.room.Values)
                    {
                        // 람다 절대 금지. 미리 캐싱된 UpdateAction 사용
                        room.Push(room.UpdateAction);
                    }
                }

                // 2. 실용주의적 Tick 대기 (CPU 점유율 최적화)
                long elapsed = sw.ElapsedMilliseconds - start;
                int sleepTime = tickMs - (int)elapsed;

                if (sleepTime > 0)
                {
                    // OS 스케줄러에 자원을 양보함. (약간의 Tick 밀림은 현재 단계에서 감수)
                    Thread.Sleep(sleepTime);
                }
            }
        }
    }
}
