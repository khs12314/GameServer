using GridTest.InGameObj.PlayerInfo;
using GridTest.Manager;
using GridTest.Room;
using GridTest.Util;

public class Item : GameObject
{
    public const int INVALID_SLOT = -1;
    internal bool IsDirty;
    public long DbId { get; set; }
    public int ItemID { get; set; } // 아이템 종류 식별용 ID (예: Potion=1, Sword=2 등)
    public int Count { get; set; } // 아이템 개수 (소모품인 경우 1~99, 장비는 항상 1)
    public ItemType ItemType { get; set; } // 아이템 타입 (예: Equipment, Consumables 등)
    public short SlotIndex { get; set; } = INVALID_SLOT; // 인벤토리 슬롯 인덱스 (0~19), INVALID_SLOT은 인벤토리에 없는 상태를 의미
    public int MaxStack { get; set; } // 최대 스택 수량 (소모품인 경우 99, 장비는 1 등)

    public Item()
    {
        gameObjectType = GameObjectType.Item; // Item 타입
        SlotIndex = INVALID_SLOT; // 기본값은 인벤토리에 없는 상태
        Count = 1; // 기본적으로 1개로 시작 (소모품은 나중에 합쳐질 때 Count가 증가할 수 있음)
    }
    
    public void Initialize(float x, float y, float z, GameRoom_Logic Room, int itemId, ItemType type, int maxstack)
    {
        Room.Push(() =>
        {
            PosX = x;
            PosY = y;
            PosZ = z;
            CellX = (int)(PosX * GameRoom_Logic.InvCellSize);
            CellY = (int)(PosZ * GameRoom_Logic.InvCellSize);
            this.Room = Room;
            ItemID = itemId;
            ItemType = type;
            MaxStack = maxstack;
            DbId = 0;
            Id = ObjectManager.GenerateId(GameObjectType.Temporary);
            FileLogger.Log($"아이템 생성 시도: ID={Id},종류 = {itemId}, Type={type}, MaxStack={maxstack} at ({x}, {y}, {z})");
            Room.Item(this);
        });
        Room.PushAfter(30000, () =>
        {
            // 30초 뒤 아이템 소멸
            Room.Broadcast(new S_MMODespawnItem() { entityID = this.Id }.Write(), this);
            Room.GetGameUsers().Remove(this.Id); // 룸의 오브젝트 리스트에서도 제거
        });
    }
}

