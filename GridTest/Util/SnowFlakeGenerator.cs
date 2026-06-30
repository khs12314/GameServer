using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Util
{
    public class SnowFlakeGenerator
    {
        // 1. [구조 상수] 비트 길이
        private const int WorkerIdBits = 10;
        private const int SequenceBits = 12;

        // 2. [오프셋 상수] 비트를 밀어낼 칸 수 계산
        private const int WorkerIdShift = SequenceBits; // 12칸 밂
        private const int TimestampShift = SequenceBits + WorkerIdBits; // 22칸 밂

        // 3. [마스크 상수] 최대치 제한 (비트 연산의 마법)
        // -1L << 12는 끝 12자리를 0으로 만들고, 그걸 -1과 XOR(^)하면 12자리가 전부 1이 됨 (값: 4095)
        private const long SequenceMask = -1L ^ -1L << SequenceBits;

        // 4. [커스텀 Epoch] 기준 시간 (예: 2024년 1월 1일)
        // 1970년부터의 시간을 그대로 쓰면 41비트가 금방 꽉 차므로, 기준점을 최근으로 당긴다.
        private const long Twepoch = 1704067200000L;

        // 5. [상태 변수] 런타임에 변하는 값들
        private static long _workerId = 1; // 단일 서버이므로 1로 고정
        private static long _sequence = 0;
        private static long _lastTimestamp = -1L;

        // 6. [동시성 제어]
        private static readonly object _lock = new object();

        public static long NextId()
        {
            lock (_lock)
            {
                // UTC 기준으로 현재 밀리초 획득
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (timestamp < _lastTimestamp)
                {
                    throw new Exception("서버 시계가 뒤로 돌아갔습니다. ID 생성을 거부합니다.");
                }

                if (timestamp == _lastTimestamp)
                {
                    // 같은 밀리초 내라면 시퀀스 1 증가. 
                    // 만약 4095에서 1이 더해지면 SequenceMask(4095)와의 & 연산에 의해 0이 됨.
                    _sequence = _sequence + 1 & SequenceMask;

                    if (_sequence == 0)
                    {
                        // 시퀀스가 한계치(4095)를 넘어 0이 되었다면, 다음 밀리초가 될 때까지 대기
                        timestamp = WaitNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    // 다음 밀리초로 넘어갔다면 시퀀스 0으로 초기화
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                // 최종 비트 조합: (시간차이 << 22) | (워커ID << 12) | 시퀀스
                return timestamp - Twepoch << TimestampShift |
                       _workerId << WorkerIdShift |
                       _sequence;
            }
        }

        // 다음 밀리초가 될 때까지 CPU를 헛돌게 하며 대기 (Spin-Wait)
        private static long WaitNextMillis(long lastTimestamp)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (timestamp <= lastTimestamp)
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            return timestamp;
        }

    }
}
