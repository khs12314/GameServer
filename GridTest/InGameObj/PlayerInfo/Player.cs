using GridTest.DataManagers;
using GridTest.InGameObj.PlayerInfo;
using GridTest.Manager;
using GridTest.Server;
using GridTest.Session;
using GridTest.Util;
using NetWorkModule;
public struct PlayerSkillInfo
{
    public int SkillID;
    public long Cooldown;
}
public class Player : GameObject
{
    int Level = 1;
    long CurrentExp;
    public bool IsGhost = false;
    public int attack { get; private set; }
    public Channel MyChannel;
    public Dictionary<int, PlayerSkillInfo> PlayerSkillDic = new Dictionary<int, PlayerSkillInfo>();
    public int PlayerID { get; set; }//플레이어 전용 고유 아이디 db에서 가져올 내용(계정 생성 순서에 따라 저정된 db의 pk값)
    public Inventory Inventory { get; set; } = new Inventory();
    public QuestState QuestState { get; set; }
    public Equipment Equipment { get; set; }
    public GameUser_Session Session { get; set; }
    private const int LOOT_COOLDOWN_MS = 200;
    private long _lastLootTick = 0;
    public Player()
    {
        gameObjectType = GameObjectType.Player; // Player 타입
        this.QuestState = new QuestState();
        //추후 db에서 가져온 값으로 초기화
        Equipment = new Equipment();

        this.RefreshStatus();
    }
    public Player(int playerID, GameUser_Session session, Channel channel)
    {

        this.MyChannel = channel;
        gameObjectType = GameObjectType.Player; // Player 타입
        PlayerID = playerID;//db에서 발급받은 고유id
        Session = session;
        this.Id = ObjectManager.GenerateId(GameObjectType.Player);//방 접속 시 사용할 고유 id
        this.QuestState = new QuestState();
        foreach(var skillData in SkillDataManager.SkillDataDic.Values)
        {
            PlayerSkillInfo skillInfo = new PlayerSkillInfo
            {
                SkillID = skillData.SkillId,
                Cooldown = 0
            };
            PlayerSkillDic[skillData.SkillId] = skillInfo;
        }
        //추후 db에서 가져온 값으로 초기화
        Equipment = new Equipment();
        this.RefreshStatus();
    }
    public void RedisSave()
    {
        if (this.Room == null || !Session.UserToken.socket.Connected /* 혹은 this.Session.IsConnected == false 등 자네의 종료 조건 */)
        {
            Console.WriteLine($"[Redis 틱 종료] {this.Id}번 유저 퇴장. 주기적 저장 루프 정지.");
            return; // 여기서 return 해버리면 더 이상 PushAfter가 안 돌고 소멸한다.
        }
        RedisManager.SavePlayerState(this.Id, this.Hp, this.PosX, this.PosY, this.PosZ);
        this.Room.PushAfter(1000, RedisSave); // 1초마다 DB에 저장 예약
    }
    public bool CanLoot()
    {
        long now = Environment.TickCount64;
        if (now - _lastLootTick < LOOT_COOLDOWN_MS)
        {
            return false; // 아직 쿨타임이 안 지났음
        }
        _lastLootTick = now;
        return true;
    }
    public void AddExp(long expToAdd)
    {
        // 방어 로직: 경험치가 0 이하이거나, 이미 만렙이면 연산 자체를 드랍한다.
        if (expToAdd <= 0 || this.Level >= LevelDataManager.MaxLevel) return;

        this.CurrentExp += expToAdd;
        bool isLevelUp = false;

        // 다중 레벨업 처리를 위한 while 루프
        while (true)
        {
            // 1. 현재 레벨이 만렙에 도달했는지 확인 (Clamp 방어)
            if (this.Level >= LevelDataManager.MaxLevel)
            {
                this.CurrentExp = 0; // 만렙 달성 시 초과 경험치 증발
                break;
            }

            // 2. 현재 내 레벨의 마스터 데이터를 가져온다.
            LevelData currentLevelData = LevelManager.GetLevelData(this.Level);
            if (currentLevelData == null) break;

            // 3. 내 현재 경험치가 레벨업 요구치를 넘겼는가?
            if (this.CurrentExp < currentLevelData.RequiredExp)
            {
                break; // 아직 못 채웠으면 루프 정상 탈출
            }

            // --- 레벨업 발생 ---

            // 4. 요구치만큼만 경험치를 차감하고, 레벨을 1 올린다.
            this.CurrentExp -= currentLevelData.RequiredExp;
            this.Level++;
            isLevelUp = true;

            // 5. 오른 레벨의 스탯으로 갱신하고 체력을 꽉 채운다.
            LevelData nextLevelData = LevelManager.GetLevelData(this.Level);
            if (nextLevelData != null)
            {
                this.MaxHp = nextLevelData.MaxHp;
                this.MaxMp = nextLevelData.MaxMp;
                this.Hp = this.MaxHp;
                RefreshStatus();
            }
        }

        // 6. 한 번이라도 레벨업을 했다면, 루프를 다 빠져나온 뒤에 [단 한 번만] 브로드캐스트
        if (isLevelUp)
        {
            S_LevelUp s_LevelUp = new S_LevelUp();
            s_LevelUp.playerid = this.Id;
            s_LevelUp.level = this.Level; // 몇 레벨이 되었는지도 알려줘야 클라이언트가 UI를 갱신함

            // 이미 Room 안에서 돌고 있으므로 Push 없이 즉시 브로드캐스트
            this.Room.Broadcast(s_LevelUp.Write());
        }
    }
    public void OnKillMonster(Monster monster)
    {
        int monsterid = monster.MonsterID;
        QuestState.UpdateObjectiveEvent(this, QuestObjectiveType.Kill, monsterid, 1);
        AddExp(monster.Exp);
    }
    public void UseItem(C_UseItem packet)
    {
        short index = packet.index;

        // 1. 완벽한 범위 검증 (해킹 및 배열 초과 방어)
        if (index < 0 || index >= Inventory.Slots.Length)
            return;

        Item item = Inventory.Slots[index];
        if (item == null) return;
        switch (item.ItemType)
        {
            case ItemType.Equipment:
                SwapEquipment(item, index);
                break;
            case ItemType.Consumables:
                ConsumeItem(item, index);
                break;
            default:
                return;
        }
    }
    public void SwapEquipment(Item item, short index)
    {
        WeaponData weaponData = WeaponManager.GetWeaponData(item.ItemID);
        if (weaponData == null)
        {
            // 무기 데이터 없음
            return;
        }
        if (weaponData.ReqLevel > this.Level)
        {
            // 레벨 부족
            return;
        }
        if (Equipment.GetWeapon() == null)
        {
            Equipment.SetWeapon(item);
            Inventory.Slots[index] = null; // 인벤토리 슬롯 비우기

        }
        else
        {
            // 기존 장착 무기와 교체
            Item oldWeapon = Equipment.GetWeapon();
            Inventory.Slots[index] = oldWeapon; // 인벤토리 슬롯에 기존 무기 넣기
            Equipment.SetWeapon(item); // 새 무기 장착
        }
        this.RefreshStatus();
        Room.UpdateSlot(this, index); // 인벤토리 슬롯 업데이트
        Room.UpdateEquipment(this, item); // 장비 상태 업데이트
        Room.MyStatusUpdate(this); // 공격력 등 상태 업데이트
    }
    public void RefreshStatus()
    {
        Item weapon = Equipment.GetWeapon();
        this.attack = Level * 2;
        if (weapon != null)
        {
            this.attack += WeaponManager.GetWeaponData(weapon.ItemID).AttackPower;
        }
    }
    public void UnEquipWeapon(C_UnequipItem c_UnequipItem)
    {
        short index = c_UnequipItem.index;
        Item weapon = Equipment.GetWeapon();
        if (weapon == null)
            return;
        if (index != -1 && (index < 0 || index >= Inventory.Slots.Length))
            return;
        if (index != -1)
        {
            if (Inventory.Slots[index] != null) return;

            // 포인터 맞교환 및 해제 완료
            Inventory.Slots[index] = weapon;
            Equipment.SetWeapon(null);
        }
        else
        {
            short freeSlotIndex = -1;
            for (short i = 0; i < Inventory.Slots.Length; i++)
            {
                if (Inventory.Slots[i] == null)
                {
                    freeSlotIndex = i;
                    break;
                }
            }
            if (freeSlotIndex == -1) return; // 인벤토리 꽉참
            Inventory.Slots[freeSlotIndex] = weapon;
            Equipment.SetWeapon(null);
            index = freeSlotIndex;
        }
        this.RefreshStatus();
        Room.UpdateSlot(this, index);
        Room.UpdateEquipment(this);
        Room.MyStatusUpdate(this);
    }
    public void UseSkill(C_UseSkill packet)
    {
        int skillId = packet.skillId;
        if (!PlayerSkillDic.TryGetValue(skillId, out PlayerSkillInfo skillinfo))
            return;
        long currentTime = Environment.TickCount64;
        if (currentTime < skillinfo.Cooldown)
            return; // 스킬 쿨타임 중
        // 스킬 사용 로직 수행 및 브로드 캐스트
        SkillData? skillData = SkillDataManager.GetSkillData(packet.skillId);
        if(skillData == null)
            return;
        float angleRad = (float)(this.RotY * Math.PI / 180f);
        float dirX = (float)Math.Sin(angleRad); // X가 Sin
        float dirZ = (float)Math.Cos(angleRad); // Z가 Cos
        ProJectile proJectile = this.Room.Get_Projectile();
        this.Room.Add_Projectile(skillData, dirX, dirZ, proJectile,this);
        // 쿨타임 갱신
        skillinfo.Cooldown = currentTime + skillData.Cooldown;
        PlayerSkillDic[skillId] = skillinfo;
        

    }
    public void ConsumeItem(Item item, short index)
    {

        // 2. 널 체크 및 타입/수량 검증
        if (item == null || item.ItemType != ItemType.Consumables || item.Count <= 0)
            return;

        // 3. O(1) 데이터 룩업 (Data-Driven)
        ConsumeItemData data = ConsumeItemDataManager.GetConsumeItemData(item.ItemID);
        if (data == null)
            return;

        // 4. 상태 전이 및 빈 슬롯 정리 (Zero-Allocation 로직)
        item.Count--;
        if (item.Count <= 0)
        {
            Inventory.Slots[index] = null; // 유령 아이템 제거
        }

        // 5. 스탯 갱신
        if (data.ConsumeItemType == ConsumeItemType.Hp || data.ConsumeItemType == ConsumeItemType.Both)
            this.Hp = Math.Min(this.Hp + data.Healamount, this.MaxHp);

        if (data.ConsumeItemType == ConsumeItemType.Mp || data.ConsumeItemType == ConsumeItemType.Both)
            this.Mp = Math.Min(this.Mp + data.Healamount, this.MaxMp);

        // 6. 패킷 브로드캐스팅 (내부적으로 객체 풀링/Zero-Alloc 적용 필)
        Room.UpdateSlot(this, index);
        Room.PlayerStatusUpdate(this);
    }
}

