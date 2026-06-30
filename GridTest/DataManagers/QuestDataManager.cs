using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.DataManagers
{
    public struct QuestObjective
    {
        public QuestObjectiveType Type;
        public int TemplateId;
        public int Count;
    }
    public enum QuestObjectiveType : byte
    {
        Kill = 1,
        Collect = 2,
        Talk = 3
    }
    public class QuestTemplate
    {
        public int QuestId;
        public string Name;
        public int StartNpcId;
        public int CompleteNpcId;
        public int ReqLevel;
        public int PreQuestId;
        // 파일에는 컬럼이 평탄화되어 있지만, 메모리에서는 리스트로 묶어서 관리한다.
        // 최대 3개이므로 Capacity를 3으로 줘서 내부 배열 재할당을 막는다.
        public List<QuestObjective> Objectives = new List<QuestObjective>(3);
        public int RewardExp;
        public int RewardGold;
        public int RewardItemId;
        public int RewardItemCount;
    }
    public static class QuestDataManager
    {
        public static Dictionary<int, List<int>> _monsterToQuests = new Dictionary<int, List<int>>();
        public static Dictionary<int, QuestTemplate> QuestTemplates = new Dictionary<int, QuestTemplate>();
        public static Dictionary<int, List<int>> _npcStartQuests = new Dictionary<int, List<int>>(); // NPC ID -> 시작 퀘스트 ID 리스트
        public static void LoadQuestData(string filePath, string QuestDatacsv)
        {
            filePath = Path.Combine(filePath, QuestDatacsv);
            // 1. 파일 전체를 단 하나의 거대한 string으로 딱 한 번만 읽는다. (유일한 대규모 할당)
            if (!File.Exists(filePath))
            {
                FileLogger.Log($"[데이터 로드 실패] 퀘스트 데이터 파일이 존재하지 않습니다: {filePath}");
                return;
            }
            string fileContent = File.ReadAllText(filePath);

            // 2. 이 거대한 텍스트를 Span(메모리 참조)으로 변환
            ReadOnlySpan<char> span = fileContent.AsSpan();
            // 3. 헤더(첫 줄) 스킵 로직
            int headerIndex = span.IndexOf('\n');
            if (headerIndex != -1)
            {
                // 헤더 다음 글자부터 끝까지로 시야(Span)를 좁힘
                span = span.Slice(headerIndex + 1);
            }

            while (!span.IsEmpty)
            {
                // 한 줄(Line) 범위 찾기
                int lineEnd = span.IndexOf('\n');
                ReadOnlySpan<char> lineSpan = lineEnd != -1 ? span.Slice(0, lineEnd) : span;

                // 다음 줄을 위해 원본 span을 전진시킴
                span = lineEnd != -1 ? span.Slice(lineEnd + 1) : ReadOnlySpan<char>.Empty;


                // 캐리지 리턴(\r) 제거
                if (lineSpan.Length > 0 && lineSpan[^1] == '\r')
                {
                    lineSpan = lineSpan.Slice(0, lineSpan.Length - 1);
                }

                if (lineSpan.IsEmpty) continue;

                // --- 이 아래부터가 핵심 데이터 파싱이다 ---
                QuestTemplate quest = new QuestTemplate();

                // Col 0: QuestId (할당 없는 int 변환)
                quest.QuestId = CsvParser.ParseNextInt(ref lineSpan);

                // Col 1: Name (문자열은 객체에 저장해야 하므로 ToString() 호출)
                quest.Name = CsvParser.ParseNextString(ref lineSpan);

                // Col 2, 3: NPC ID
                quest.StartNpcId = CsvParser.ParseNextInt(ref lineSpan);
                quest.CompleteNpcId = CsvParser.ParseNextInt(ref lineSpan);

                // Col 4, 5: 조건
                quest.ReqLevel = CsvParser.ParseNextInt(ref lineSpan);
                quest.PreQuestId = CsvParser.ParseNextInt(ref lineSpan);

                // Col 6~14: 다중 목표 파싱 (반복문 또는 수동 Unrolling)
                // 목표 1
                AddObjectiveIfValid(quest, (QuestObjectiveType)CsvParser.ParseNextByte(ref lineSpan), CsvParser.ParseNextInt(ref lineSpan), CsvParser.ParseNextInt(ref lineSpan));
                // 목표 2
                AddObjectiveIfValid(quest, (QuestObjectiveType)CsvParser.ParseNextByte(ref lineSpan), CsvParser.ParseNextInt(ref lineSpan), CsvParser.ParseNextInt(ref lineSpan));
                // 목표 3
                AddObjectiveIfValid(quest, (QuestObjectiveType)CsvParser.ParseNextByte(ref lineSpan), CsvParser.ParseNextInt(ref lineSpan), CsvParser.ParseNextInt(ref lineSpan));

                // Col 15~18: 보상
                quest.RewardExp = CsvParser.ParseNextInt(ref lineSpan);
                // 나머지 보상들도 동일하게 ParseNextInt로 호출...

                // 딕셔너리에 적재
                QuestTemplates.Add(quest.QuestId, quest);
                // NPC 시작 퀘스트 맵에 등록
                if (_npcStartQuests.TryGetValue(quest.StartNpcId, out var questList))
                {
                    questList.Add(quest.QuestId);
                }
                foreach(var objective in quest.Objectives)
                {
                    if(objective.Type == QuestObjectiveType.Kill)
                    {
                        if (!_monsterToQuests.ContainsKey(objective.TemplateId))
                        {
                            _monsterToQuests[objective.TemplateId] = new List<int>();
                            _monsterToQuests[objective.TemplateId].Add(quest.QuestId);
                        }
                    }
                }
                _npcStartQuests[quest.StartNpcId] = new List<int> { quest.QuestId };

            }
            Console.WriteLine($"[데이터 로드] 퀘스트 데이터 {QuestTemplates.Count}개 로드 완료");
            FileLogger.Log($"[데이터 로드] 퀘스트 데이터 {QuestTemplates.Count}개 Span 로드 완료.");
        }
        // [Span 헬퍼 함수 1] 다음 쉼표 전까지 잘라서 int로 파싱하고, 원본 Span을 전진시킨다.
        // [헬퍼 함수 3] 목표값이 0이 아니면 리스트에 추가
        private static void AddObjectiveIfValid(QuestTemplate quest, QuestObjectiveType type, int id, int count)
        {
            if (type != 0 && id != 0)
            {
                quest.Objectives.Add(new QuestObjective { Type = type, TemplateId = id, Count = count });
            }
        }


    }
}
