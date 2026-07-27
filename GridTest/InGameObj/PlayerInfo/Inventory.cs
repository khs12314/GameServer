using GridTest.Util;

namespace GridTest.InGameObj.PlayerInfo
{
    public class Inventory
    {
        private List<short> changeSlotindex = new List<short>(4);
        const int MaxSlots = 20;
        public Item[] Slots { get; private set; } = new Item[MaxSlots];
        public Dictionary<long, Item> Items { get; private set; } = new Dictionary<long, Item>();
        public Inventory()
        {
            Items = new Dictionary<long, Item>();
            Slots = new Item[MaxSlots];

        }
        public List<short> AddItem(Item item)
        {
            changeSlotindex.Clear();
            if (item.SlotIndex != Item.INVALID_SLOT)
            {
                return null; // 이미 어딘가에 소속된 아이템
            }

            // 아이템 타입 검사: 장비(2)가 아니면 모두 중첩 가능하다고 가정
            bool isStackable = item.ItemType != ItemType.Equipment;

            // 1단계: 소모품(Stackable)인 경우, 합칠 수 있는 기존 슬롯부터 찾는다.
            if (isStackable)
            {
                for (short i = 0; i < MaxSlots; i++)
                {
                    Item existItem = Slots[i];

                    // 빈칸이 아니고, 같은 종류의 아이템이며, 아직 덜 찬 경우
                    if (existItem != null && existItem.ObjectId == item.ObjectId && existItem.Count < existItem.MaxStack)
                    {
                        int space = existItem.MaxStack - existItem.Count;
                        int addCount = Math.Min(space, item.Count);

                        // 기존 아이템에 개수 추가
                        existItem.Count += addCount;
                        item.Count -= addCount;

                        // ★ DB 저장을 위해 기존 아이템의 상태를 Dirty로 마킹
                        existItem.IsDirty = true;

                        changeSlotindex.Add(i);
                        // 모두 합쳤으면 성공 반환
                        if (item.Count <= 0)
                        {
                            return changeSlotindex;
                        }
                    }
                }
            }

            // 2단계: 여전히 아이템이 남았거나, 처음부터 중첩 불가(장비)인 경우 빈 슬롯에 넣는다.
            // (조건: 파이프라인 단에서 이미 item.Count <= item.MaxStack 으로 맞춰져 들어왔다고 가정)
            while (item.Count > 0)
            {
                // 빈 슬롯 탐색 (별도 함수로 분리 권장)
                short emptySlot = FindEmptySlot();
                if (emptySlot == Item.INVALID_SLOT)
                {
                    // 빈칸 없음: 남은 아이템은 필드에 드랍시키거나 우편함으로 보내는 예외 처리 필요 (지금은 null 반환)
                    return null;
                }

                // 이 슬롯에 넣을 개수 계산 (남은 개수와 MaxStack 중 작은 값)
                int addAmount = Math.Min(item.Count, item.MaxStack);

                // [핵심] 파라미터로 들어온 item을 돌려쓰면 절대 안 된다!
                // DB와 독립적으로 엮일 새로운 아이템 인스턴스를 힙(Heap) 메모리에 새로 찍어내야 한다.
                Item newItem = new Item(); // (또는 ItemFactory.CreateItem(...) 사용)
                newItem.ObjectId = item.ObjectId;
                newItem.Count = addAmount;
                newItem.SlotIndex = emptySlot;

                // 이 시점에서 Snowflake를 통한 영구 ID 발급이 이루어져야 한다.
                newItem.Id = 0;
                newItem.IsDirty = true; // DB Insert 대상
                newItem.DbId = SnowFlakeGenerator.NextId();
                Console.WriteLine($"아이템 초과 분량, 새로운 아이템 생성 및 {newItem.DbId}생성 완료, 수량{item.Count}");
                // 컬렉션 등록
                Slots[emptySlot] = newItem;
                Items[newItem.Id] = newItem;

                // 파라미터 원본의 남은 수량 차감
                item.Count -= addAmount;
                changeSlotindex.Add(emptySlot);
            }

            return changeSlotindex;

        }
        short FindEmptySlot()
        {
            for (short i = 0; i < MaxSlots; i++)
            {
                if(Slots[i] == null)
                {
                    return i;
                }
            } 
            return -1;
        }
        public SwapResult SwapItem(short fromIndex, short toIndex)
        {
            if (fromIndex < 0 || fromIndex >= MaxSlots || toIndex < 0 || toIndex >= MaxSlots) return SwapResult.Failed;
            if (fromIndex == toIndex) return SwapResult.Failed;

            Item itemA = Slots[fromIndex];
            Item itemB = Slots[toIndex];

            if (itemA == null) return SwapResult.Failed; // 허공을 드래그함

            // 1. 병합(Merge) 조건 검사: B가 있고, 같은 종류이며, 스택 가능한가?
            if (itemB != null && itemA.ObjectId == itemB.ObjectId && itemA.ItemType != ItemType.Equipment)
            {
                if (itemB.Count < itemB.MaxStack)
                {
                    int space = itemB.MaxStack - itemB.Count;
                    int addCount = Math.Min(space, itemA.Count);

                    itemB.Count += addCount;
                    itemA.Count -= addCount;

                    if (itemA.Count <= 0)
                    {
                        // A가 전부 B로 흡수됨 -> A 슬롯 비우기
                        Slots[fromIndex] = null;
                        Items.Remove(itemA.Id); // 딕셔너리에서도 제거
                        return SwapResult.MergedComplete;
                    }
                    return SwapResult.MergedPartial;
                }
            }

            // 2. 단순 스왑(Swap) 연산 (빈칸으로의 이동 포함)
            Slots[fromIndex] = itemB;
            if (itemB != null) itemB.SlotIndex = fromIndex;

            Slots[toIndex] = itemA;
            itemA.SlotIndex = toIndex;

            return SwapResult.Swapped;
        }
    }
}


