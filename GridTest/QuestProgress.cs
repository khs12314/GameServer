using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
    public class QuestProgress
    {
        public int QuestId { get; set; }

        // 이 퀘스트의 각 목표(Objective) 번호와 현재 달성한 카운트 (동적 데이터)
        // Key: 목표 인덱스(0, 1, 2...), Value: 현재 진행 카운트 (예: 3마리)
        public Dictionary<int, int> CurrentCounts { get; set; } = new Dictionary<int, int>();

        // [핵심] 캡슐화된 목표 달성 검증 메서드
        public bool IsObjectiveMet()
        {
            // 1. 서버 메모리에 올라간 마스터 데이터(법전)를 가져온다.
            QuestTemplate template = QuestManager.GetQuest(QuestId);
            if (template == null) return false;

            // 2. 마스터 데이터가 요구하는 목표들을 하나씩 순회하며 검사한다.
            for (int i = 0; i < template.Objectives.Count; i++)
            {
                var objective = template.Objectives[i];

                // 현재 유저가 달성한 카운트 꺼내기
                CurrentCounts.TryGetValue(i, out int currentCount);

                // 단 하나라도 마스터 데이터의 요구치(Count)에 미달하면 즉시 false
                if (currentCount < objective.Count)
                {
                    return false;
                }
            }

            // 3. 모든 목표 카운트를 100% 충족했음! (보상 수령 대기 상태)
            return true;
        }
    }
}
