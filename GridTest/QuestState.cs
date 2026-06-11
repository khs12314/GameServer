using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
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

    }
}
