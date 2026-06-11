using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
    public static class QuestManager
    {
        public static IReadOnlyList<int> GetAvailableQuestsFromNpc(int npcId)
        {
            if (QuestDataManager._npcStartQuests.TryGetValue(npcId, out List<int> questList))
                return questList; // List<T>는 IReadOnlyList<T>를 상속받으므로 그대로 리턴 가능

            // 힙 할당(GC) 0, Null 체크 불필요
            return Array.Empty<int>();
        }
        public static bool TryGetQuest(int questId, out QuestTemplate template)
        {
            return QuestDataManager.QuestTemplates.TryGetValue(questId, out template);
        }
        public static QuestTemplate GetQuest(int questId)
        {
            QuestDataManager.QuestTemplates.TryGetValue(questId, out QuestTemplate template);
            return template; // null 반환 가능, 호출하는 쪽에서 null 체크 필요
        }
    }
}
