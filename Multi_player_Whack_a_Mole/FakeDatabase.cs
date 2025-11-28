using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_player_Whack_a_Mole
{
    public class MockDB
    {
        // ID를 키로, 점수를 값으로 저장. (Thread-Safe한 딕셔너리 사용)
        static ConcurrentDictionary<string, int> _scores = new ConcurrentDictionary<string, int>();

        public static int GetScore(string userId)
        {
            return _scores.GetOrAdd(userId, 0);
        }

        public static void AddScore(string userId, int points)
        {
            // 원자적(Atomic) 연산으로 점수 업데이트
            _scores.AddOrUpdate(userId, points, (key, oldValue) => oldValue + points);
            Console.WriteLine($"[MockDB] User {userId} Score Updated: {_scores[userId]}");
        }
    }
}
