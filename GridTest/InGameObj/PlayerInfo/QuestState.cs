using GridTest.DataManagers;
using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.InGameObj.PlayerInfo
{
    public class QuestState
    {
        HashSet<int> CompleteQuests = new HashSet<int>();
        public Dictionary<int, QuestProgress> ActiveQuests = new Dictionary<int, QuestProgress>();


        public bool IsComplete(int questId)
        {
            return CompleteQuests.Contains(questId);
        }
        public bool IsActive(int questId)
        {
            return ActiveQuests.ContainsKey(questId);
        }
        public IEnumerable<QuestProgress> GetActiveQuests()
        {
            return ActiveQuests.Values;
        }
        public void UpdateObjectiveEvent(Player player, QuestObjectiveType type, int targetId, int amount)
        {
            if (type != QuestObjectiveType.Kill) { return; }

            if (QuestDataManager._monsterToQuests.TryGetValue(targetId, out List<int> relatedQuests))
            {
                // 2. 이 몬스터와 연관된 퀘스트 번호들을 하나씩 순회
                foreach (int questId in relatedQuests)
                {
                    // 3. 유저가 이 퀘스트를 진행 중인지 확인
                    if (ActiveQuests.TryGetValue(questId, out QuestProgress progress))
                    {
                        // 4. [핵심] 진행 객체에게 카운트를 올리라고 명령. (true면 진짜로 값이 변한 것)
                        if (progress.AddKillCount(targetId, amount))
                        {
                            // isChanged 변수로 밖에서 관리할 필요 없이, 변했을 때 즉시 패킷을 조립한다.
                            S_UpdateQuestProgress res = new S_UpdateQuestProgress();
                            res.QuestId = questId;

                            // 5. '어떤 인덱스가 변했는지' 복잡하게 찾지 말고, 현재 퀘스트의 모든 진행도를 다 때려 넣는다. (State Sync)
                            // (딕셔너리 순회는 C# 구조체 Enumerator를 사용하므로 GC가 발생하지 않는 Zero-Allocation이다)
                            foreach (var kvp in progress.CurrentCounts)
                            {
                                res.Objectives.Add(new S_UpdateQuestProgress.ObjectivesItem
                                {
                                    ObjectiveIndex = kvp.Key,     // 마스터 데이터의 목표 인덱스 (0, 1, 2...)
                                    CurrentCount = kvp.Value      // 현재 유저가 달성한 마릿수
                                });
                            }

                            // 6. 클라이언트로 패킷 발송
                            player.Session.send(res.Write());
                        }
                    }
                }
            }
        }
        public bool SetCompeleteQuest(int questid)
        {
            if(!ActiveQuests.ContainsKey(questid))
            {
                return false;
            }
            CompleteQuests.Add(questid);
            ActiveQuests.Remove(questid);
            return true;
        }
    }
}
