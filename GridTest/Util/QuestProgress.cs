using GridTest.DataManagers;
using GridTest.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Util
{
    //퀘스트 진행 상황을 나타내는 클래스 (예: "슬라임 5마리 잡기" 퀘스트의 경우, 현재 몇 마리를 잡았는지)
    //1개의 퀘스트이므로 다른 퀘스트에서 같은 몬스터가 내용에 있을 수 있음. 해당 클래스는 그 정보를 모르는 고립된 상태
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

        
        public bool AddKillCount(int targetId, int amount)
        {
            // 1. O(1) 단일 탐색 (NRE 방어)
            if (!QuestManager.TryGetQuest(QuestId, out QuestTemplate template))
                return false;

            bool isChanged = false;

            // 2. 내 퀘스트의 목표(Objectives) 배열만 순회한다 (N은 기껏해야 1~3, 매우 빠름)
            for (int i = 0; i < template.Objectives.Count; i++)
            {
                QuestObjective objective = template.Objectives[i];

                // 사냥 목표이고, 방금 죽은 몬스터 ID와 일치하는가?
                if (objective.Type == QuestObjectiveType.Kill && objective.TemplateId == targetId)
                {
                    CurrentCounts.TryGetValue(i, out int currentCount);

                    // 목표치를 이미 달성했다면 무시
                    if (currentCount >= objective.Count) continue;

                    // 3. [핵심] Clamp 방어 로직 (Max Count 초과 차단)
                    int newCount = Math.Min(currentCount + amount, objective.Count);

                    if (newCount != currentCount)
                    {
                        CurrentCounts[i] = newCount; // 카운트 갱신
                        isChanged = true;            // 상태가 변했음을 마킹
                    }
                }
            }

            return isChanged;
        }
    }
}
