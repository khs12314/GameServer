using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using NetWorkModule;

public enum PacketID
{
    PlayerInfoReq = 0,
	TestMove = 1,
	C_Login = 2,
	C_EnterGame = 3,
	C_LoadingComplete = 4,
	C_HitMole = 5,
	S_LoginOk = 6,
	S_ScoreUpdate = 7,
	S_StartLoading = 8,
	S_Spawn_Mole = 9,
	S_GameStart = 10,
	S_PlayerInfo = 11,
	S_RoomRemoved = 12,
	C_Login_DB = 13,
	S_Login_DB = 14,
	C_SignUp_DB = 15,
	S_SignUp_DB = 16,
	S_EnterGame_Snake = 17,
	S_Spawn = 18,
	Search_Room_id = 19,
	C_EnterGame_Snake = 20,
	C_Move = 21,
	S_Move = 22,
	S_SpawnItem = 23,
	S_PlayerLeave = 24,
	S_Die = 25,
	C_PlayerLeave = 26,
	MMO_C_Move = 27,
	MMO_S_Move = 28,
	MMO_S_Attack = 29,
	MMO_C_Attack = 30,
	MMO_S_Die = 31,
	S_ReSpawn = 32,
	S_Damage = 33,
	S_Sync = 34,
	S_MonsterSpawn = 35,
	S_ResCharacterList = 36,
	C_ResCharacterList = 37,
	C_UseSkill = 38,
	S_UseSkill = 39,
	Spawn_Object = 40,
	S_ChangeState = 41,
	C_CreateCharacter = 42,
	S_CreateCharacter = 43,
	C_DeleteCharacter = 44,
	S_MMOSpawnItem = 45,
	C_MMOGetItem = 46,
	S_MMOGetItem = 47,
	S_MMODespawnItem = 48,
	S_UpdateSlot = 49,
	C_SwapItem = 50,
	C_NpcInteract = 51,
	S_NpcInteractResult = 52,
	C_AcceptQuest = 53,
	S_AcceptQuest = 54,
	S_UpdateQuestProgress = 55,
	C_CompleteQuest = 56,
	S_CompleteQuest = 57,
	S_GetExp = 58,
	S_LevelUp = 59,
	C_UseItem = 60,
	S_PlayerStatusUpdate = 61,
	C_UnequipItem = 62,
	S_EquipUpdate = 63,
	S_MyStatUpdate = 64,
	S_MMOSpawnList = 65,
	S_MMODespawnList = 66,
	S_MMODespawn = 67,
	S_MMOSpawn = 68,
	
}

// 공통 인터페이스
public interface IPacket
{
	short Protocol { get; }
	void Read(CPacket packet);
    public void Clear();
    CPacket Write();
}


public class PlayerInfoReq : IPacket
{
    public long playerId;public string name;

    public short Protocol { get { return (short)PacketID.PlayerInfoReq; } }

    public void Read(CPacket packet)
    {
        this.playerId = packet.pop_int64();this.name = packet.pop_string();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.PlayerInfoReq);
        packet.push(this.playerId);packet.push(this.name);
        return packet;
    }

    
    public void Clear()
    {
        this.playerId = 0;
		this.name = null;
		
    }
}

public class TestMove : IPacket
{
    public float posX;public float posY;public float posZ;

    public short Protocol { get { return (short)PacketID.TestMove; } }

    public void Read(CPacket packet)
    {
        this.posX = packet.pop_float();this.posY = packet.pop_float();this.posZ = packet.pop_float();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.TestMove);
        packet.push(this.posX);packet.push(this.posY);packet.push(this.posZ);
        return packet;
    }

    
    public void Clear()
    {
        this.posX = 0f;
		this.posY = 0f;
		this.posZ = 0f;
		
    }
}

public class C_Login : IPacket
{
    

    public short Protocol { get { return (short)PacketID.C_Login; } }

    public void Read(CPacket packet)
    {
        
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_Login);
        
        return packet;
    }

    
    public void Clear()
    {
        
    }
}

public class C_EnterGame : IPacket
{
    public int userId;

    public short Protocol { get { return (short)PacketID.C_EnterGame; } }

    public void Read(CPacket packet)
    {
        this.userId = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_EnterGame);
        packet.push(this.userId);
        return packet;
    }

    
    public void Clear()
    {
        this.userId = 0;
		
    }
}

public class C_LoadingComplete : IPacket
{
    

    public short Protocol { get { return (short)PacketID.C_LoadingComplete; } }

    public void Read(CPacket packet)
    {
        
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_LoadingComplete);
        
        return packet;
    }

    
    public void Clear()
    {
        
    }
}

public class C_HitMole : IPacket
{
    public int moleIndex;public int UserID;

    public short Protocol { get { return (short)PacketID.C_HitMole; } }

    public void Read(CPacket packet)
    {
        this.moleIndex = packet.pop_int32();this.UserID = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_HitMole);
        packet.push(this.moleIndex);packet.push(this.UserID);
        return packet;
    }

    
    public void Clear()
    {
        this.moleIndex = 0;
		this.UserID = 0;
		
    }
}

public class S_LoginOk : IPacket
{
    public int ServerUserID;

    public short Protocol { get { return (short)PacketID.S_LoginOk; } }

    public void Read(CPacket packet)
    {
        this.ServerUserID = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_LoginOk);
        packet.push(this.ServerUserID);
        return packet;
    }

    
    public void Clear()
    {
        this.ServerUserID = 0;
		
    }
}

public class S_ScoreUpdate : IPacket
{
    public int score;

    public short Protocol { get { return (short)PacketID.S_ScoreUpdate; } }

    public void Read(CPacket packet)
    {
        this.score = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_ScoreUpdate);
        packet.push(this.score);
        return packet;
    }

    
    public void Clear()
    {
        this.score = 0;
		
    }
}

public class S_StartLoading : IPacket
{
    public int PlayerIndex;

    public short Protocol { get { return (short)PacketID.S_StartLoading; } }

    public void Read(CPacket packet)
    {
        this.PlayerIndex = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_StartLoading);
        packet.push(this.PlayerIndex);
        return packet;
    }

    
    public void Clear()
    {
        this.PlayerIndex = 0;
		
    }
}

public class S_Spawn_Mole : IPacket
{
    public int moleIndex;

    public short Protocol { get { return (short)PacketID.S_Spawn_Mole; } }

    public void Read(CPacket packet)
    {
        this.moleIndex = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_Spawn_Mole);
        packet.push(this.moleIndex);
        return packet;
    }

    
    public void Clear()
    {
        this.moleIndex = 0;
		
    }
}

public class S_GameStart : IPacket
{
    public short duration;

    public short Protocol { get { return (short)PacketID.S_GameStart; } }

    public void Read(CPacket packet)
    {
        this.duration = packet.pop_int16();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_GameStart);
        packet.push(this.duration);
        return packet;
    }

    
    public void Clear()
    {
        this.duration = 0;
		
    }
}

public class S_PlayerInfo : IPacket
{
    public byte playerIndex;

    public short Protocol { get { return (short)PacketID.S_PlayerInfo; } }

    public void Read(CPacket packet)
    {
        this.playerIndex = packet.pop_byte();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_PlayerInfo);
        packet.push(this.playerIndex);
        return packet;
    }

    
    public void Clear()
    {
        this.playerIndex = 0;
		
    }
}

public class S_RoomRemoved : IPacket
{
    

    public short Protocol { get { return (short)PacketID.S_RoomRemoved; } }

    public void Read(CPacket packet)
    {
        
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_RoomRemoved);
        
        return packet;
    }

    
    public void Clear()
    {
        
    }
}

public class C_Login_DB : IPacket
{
    public string ID;public string PassWord;

    public short Protocol { get { return (short)PacketID.C_Login_DB; } }

    public void Read(CPacket packet)
    {
        this.ID = packet.pop_string();this.PassWord = packet.pop_string();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_Login_DB);
        packet.push(this.ID);packet.push(this.PassWord);
        return packet;
    }

    
    public void Clear()
    {
        this.ID = null;
		this.PassWord = null;
		
    }
}

public class S_Login_DB : IPacket
{
    public bool Sucess;public int UserID;public string Nickname;

    public short Protocol { get { return (short)PacketID.S_Login_DB; } }

    public void Read(CPacket packet)
    {
        this.Sucess = packet.pop_bool();this.UserID = packet.pop_int32();this.Nickname = packet.pop_string();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_Login_DB);
        packet.push(this.Sucess);packet.push(this.UserID);packet.push(this.Nickname);
        return packet;
    }

    
    public void Clear()
    {
        this.Sucess = false;
		this.UserID = 0;
		this.Nickname = null;
		
    }
}

public class C_SignUp_DB : IPacket
{
    public string ID;public string PassWord;public string nickname;

    public short Protocol { get { return (short)PacketID.C_SignUp_DB; } }

    public void Read(CPacket packet)
    {
        this.ID = packet.pop_string();this.PassWord = packet.pop_string();this.nickname = packet.pop_string();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_SignUp_DB);
        packet.push(this.ID);packet.push(this.PassWord);packet.push(this.nickname);
        return packet;
    }

    
    public void Clear()
    {
        this.ID = null;
		this.PassWord = null;
		this.nickname = null;
		
    }
}

public class S_SignUp_DB : IPacket
{
    public bool Sucess;public string nickname;

    public short Protocol { get { return (short)PacketID.S_SignUp_DB; } }

    public void Read(CPacket packet)
    {
        this.Sucess = packet.pop_bool();this.nickname = packet.pop_string();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_SignUp_DB);
        packet.push(this.Sucess);packet.push(this.nickname);
        return packet;
    }

    
    public void Clear()
    {
        this.Sucess = false;
		this.nickname = null;
		
    }
}

public class S_EnterGame_Snake : IPacket
{
    public float mapSizeX;public float mapSizeY;public int userid;

    public short Protocol { get { return (short)PacketID.S_EnterGame_Snake; } }

    public void Read(CPacket packet)
    {
        this.mapSizeX = packet.pop_float();this.mapSizeY = packet.pop_float();this.userid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_EnterGame_Snake);
        packet.push(this.mapSizeX);packet.push(this.mapSizeY);packet.push(this.userid);
        return packet;
    }

    
    public void Clear()
    {
        this.mapSizeX = 0f;
		this.mapSizeY = 0f;
		this.userid = 0;
		
    }
}

public class S_Spawn : IPacket
{
    public float SpawnX;public float SpawnY;public float SpawnZ;public int entityID;public int typeid;public int objectid;

    public short Protocol { get { return (short)PacketID.S_Spawn; } }

    public void Read(CPacket packet)
    {
        this.SpawnX = packet.pop_float();this.SpawnY = packet.pop_float();this.SpawnZ = packet.pop_float();this.entityID = packet.pop_int32();this.typeid = packet.pop_int32();this.objectid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_Spawn);
        packet.push(this.SpawnX);packet.push(this.SpawnY);packet.push(this.SpawnZ);packet.push(this.entityID);packet.push(this.typeid);packet.push(this.objectid);
        return packet;
    }

    
    public void Clear()
    {
        this.SpawnX = 0f;
		this.SpawnY = 0f;
		this.SpawnZ = 0f;
		this.entityID = 0;
		this.typeid = 0;
		this.objectid = 0;
		
    }
}

public class Search_Room_id : IPacket
{
    public bool Room_ID;

    public short Protocol { get { return (short)PacketID.Search_Room_id; } }

    public void Read(CPacket packet)
    {
        this.Room_ID = packet.pop_bool();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.Search_Room_id);
        packet.push(this.Room_ID);
        return packet;
    }

    
    public void Clear()
    {
        this.Room_ID = false;
		
    }
}

public class C_EnterGame_Snake : IPacket
{
    public int userId;public int Room_id;

    public short Protocol { get { return (short)PacketID.C_EnterGame_Snake; } }

    public void Read(CPacket packet)
    {
        this.userId = packet.pop_int32();this.Room_id = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_EnterGame_Snake);
        packet.push(this.userId);packet.push(this.Room_id);
        return packet;
    }

    
    public void Clear()
    {
        this.userId = 0;
		this.Room_id = 0;
		
    }
}

public class C_Move : IPacket
{
    public int direction;

    public short Protocol { get { return (short)PacketID.C_Move; } }

    public void Read(CPacket packet)
    {
        this.direction = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_Move);
        packet.push(this.direction);
        return packet;
    }

    
    public void Clear()
    {
        this.direction = 0;
		
    }
}

public class S_Move : IPacket
{
    public int playerId;public float posX;public float posY;public bool IsEat;

    public short Protocol { get { return (short)PacketID.S_Move; } }

    public void Read(CPacket packet)
    {
        this.playerId = packet.pop_int32();this.posX = packet.pop_float();this.posY = packet.pop_float();this.IsEat = packet.pop_bool();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_Move);
        packet.push(this.playerId);packet.push(this.posX);packet.push(this.posY);packet.push(this.IsEat);
        return packet;
    }

    
    public void Clear()
    {
        this.playerId = 0;
		this.posX = 0f;
		this.posY = 0f;
		this.IsEat = false;
		
    }
}

public class S_SpawnItem : IPacket
{
    public int ItemID;public int posX;public int posY;

    public short Protocol { get { return (short)PacketID.S_SpawnItem; } }

    public void Read(CPacket packet)
    {
        this.ItemID = packet.pop_int32();this.posX = packet.pop_int32();this.posY = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_SpawnItem);
        packet.push(this.ItemID);packet.push(this.posX);packet.push(this.posY);
        return packet;
    }

    
    public void Clear()
    {
        this.ItemID = 0;
		this.posX = 0;
		this.posY = 0;
		
    }
}

public class S_PlayerLeave : IPacket
{
    public int userid;

    public short Protocol { get { return (short)PacketID.S_PlayerLeave; } }

    public void Read(CPacket packet)
    {
        this.userid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_PlayerLeave);
        packet.push(this.userid);
        return packet;
    }

    
    public void Clear()
    {
        this.userid = 0;
		
    }
}

public class S_Die : IPacket
{
    public int userid;

    public short Protocol { get { return (short)PacketID.S_Die; } }

    public void Read(CPacket packet)
    {
        this.userid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_Die);
        packet.push(this.userid);
        return packet;
    }

    
    public void Clear()
    {
        this.userid = 0;
		
    }
}

public class C_PlayerLeave : IPacket
{
    public int userid;

    public short Protocol { get { return (short)PacketID.C_PlayerLeave; } }

    public void Read(CPacket packet)
    {
        this.userid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_PlayerLeave);
        packet.push(this.userid);
        return packet;
    }

    
    public void Clear()
    {
        this.userid = 0;
		
    }
}

public class MMO_C_Move : IPacket
{
    public float posX;public float posY;public float posZ;public float rot;

    public short Protocol { get { return (short)PacketID.MMO_C_Move; } }

    public void Read(CPacket packet)
    {
        this.posX = packet.pop_float();this.posY = packet.pop_float();this.posZ = packet.pop_float();this.rot = packet.pop_float();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.MMO_C_Move);
        packet.push(this.posX);packet.push(this.posY);packet.push(this.posZ);packet.push(this.rot);
        return packet;
    }

    
    public void Clear()
    {
        this.posX = 0f;
		this.posY = 0f;
		this.posZ = 0f;
		this.rot = 0f;
		
    }
}

public class MMO_S_Move : IPacket
{
    public int playerid;public float posX;public float posY;public float posZ;public float rot;public byte state;

    public short Protocol { get { return (short)PacketID.MMO_S_Move; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.posX = packet.pop_float();this.posY = packet.pop_float();this.posZ = packet.pop_float();this.rot = packet.pop_float();this.state = packet.pop_byte();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.MMO_S_Move);
        packet.push(this.playerid);packet.push(this.posX);packet.push(this.posY);packet.push(this.posZ);packet.push(this.rot);packet.push(this.state);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.posX = 0f;
		this.posY = 0f;
		this.posZ = 0f;
		this.rot = 0f;
		this.state = 0;
		
    }
}

public class MMO_S_Attack : IPacket
{
    public int playerid;public int skillId;public byte state;

    public short Protocol { get { return (short)PacketID.MMO_S_Attack; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.skillId = packet.pop_int32();this.state = packet.pop_byte();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.MMO_S_Attack);
        packet.push(this.playerid);packet.push(this.skillId);packet.push(this.state);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.skillId = 0;
		this.state = 0;
		
    }
}

public class MMO_C_Attack : IPacket
{
    public int skillId;

    public short Protocol { get { return (short)PacketID.MMO_C_Attack; } }

    public void Read(CPacket packet)
    {
        this.skillId = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.MMO_C_Attack);
        packet.push(this.skillId);
        return packet;
    }

    
    public void Clear()
    {
        this.skillId = 0;
		
    }
}

public class MMO_S_Die : IPacket
{
    public int playerid;public byte state;

    public short Protocol { get { return (short)PacketID.MMO_S_Die; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.state = packet.pop_byte();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.MMO_S_Die);
        packet.push(this.playerid);packet.push(this.state);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.state = 0;
		
    }
}

public class S_ReSpawn : IPacket
{
    public int playerid;public int typeid;

    public short Protocol { get { return (short)PacketID.S_ReSpawn; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.typeid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_ReSpawn);
        packet.push(this.playerid);packet.push(this.typeid);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.typeid = 0;
		
    }
}

public class S_Damage : IPacket
{
    public int playerid;public int damage;public byte state;

    public short Protocol { get { return (short)PacketID.S_Damage; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.damage = packet.pop_int32();this.state = packet.pop_byte();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_Damage);
        packet.push(this.playerid);packet.push(this.damage);packet.push(this.state);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.damage = 0;
		this.state = 0;
		
    }
}

public class S_Sync : IPacket
{
    public int playerid;public float posX;public float posY;public float posZ;public float rotY;

    public short Protocol { get { return (short)PacketID.S_Sync; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.posX = packet.pop_float();this.posY = packet.pop_float();this.posZ = packet.pop_float();this.rotY = packet.pop_float();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_Sync);
        packet.push(this.playerid);packet.push(this.posX);packet.push(this.posY);packet.push(this.posZ);packet.push(this.rotY);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.posX = 0f;
		this.posY = 0f;
		this.posZ = 0f;
		this.rotY = 0f;
		
    }
}

public class S_MonsterSpawn : IPacket
{
    public float SpawnX;public float SpawnY;public float SpawnZ;public int entityid;public int objectid;

    public short Protocol { get { return (short)PacketID.S_MonsterSpawn; } }

    public void Read(CPacket packet)
    {
        this.SpawnX = packet.pop_float();this.SpawnY = packet.pop_float();this.SpawnZ = packet.pop_float();this.entityid = packet.pop_int32();this.objectid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_MonsterSpawn);
        packet.push(this.SpawnX);packet.push(this.SpawnY);packet.push(this.SpawnZ);packet.push(this.entityid);packet.push(this.objectid);
        return packet;
    }

    
    public void Clear()
    {
        this.SpawnX = 0f;
		this.SpawnY = 0f;
		this.SpawnZ = 0f;
		this.entityid = 0;
		this.objectid = 0;
		
    }
}

public class S_ResCharacterList : IPacket
{
    public struct CharactersItem
	{
	    public int CharacterType;
		public string Nickname;
		public int Level;
		public int CharacterId;
		
	}
	public List<CharactersItem> Characters = new List<CharactersItem>();

    public short Protocol { get { return (short)PacketID.S_ResCharacterList; } }

    public void Read(CPacket packet)
    {
        this.Characters.Clear();
		int CharactersCount = packet.pop_int32();
		for (int i = 0; i < CharactersCount; i++)
		{
		    CharactersItem item = new CharactersItem();
		    item.CharacterType = packet.pop_int32();
				item.Nickname = packet.pop_string();
				item.Level = packet.pop_int32();
				item.CharacterId = packet.pop_int32();
				
		    this.Characters.Add(item);
		}
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_ResCharacterList);
        packet.push(this.Characters.Count);
		foreach (CharactersItem item in this.Characters)
		{
		    packet.push(item.CharacterType);
				packet.push(item.Nickname);
				packet.push(item.Level);
				packet.push(item.CharacterId);
				
		}
        return packet;
    }

    
    public void Clear()
    {
        this.Characters.Clear();
		
    }
}

public class C_ResCharacterList : IPacket
{
    public struct CharactersItem
	{
	    public int CharacterType;
		public string Nickname;
		public int Level;
		public int CharacterId;
		
	}
	public List<CharactersItem> Characters = new List<CharactersItem>();

    public short Protocol { get { return (short)PacketID.C_ResCharacterList; } }

    public void Read(CPacket packet)
    {
        this.Characters.Clear();
		int CharactersCount = packet.pop_int32();
		for (int i = 0; i < CharactersCount; i++)
		{
		    CharactersItem item = new CharactersItem();
		    item.CharacterType = packet.pop_int32();
				item.Nickname = packet.pop_string();
				item.Level = packet.pop_int32();
				item.CharacterId = packet.pop_int32();
				
		    this.Characters.Add(item);
		}
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_ResCharacterList);
        packet.push(this.Characters.Count);
		foreach (CharactersItem item in this.Characters)
		{
		    packet.push(item.CharacterType);
				packet.push(item.Nickname);
				packet.push(item.Level);
				packet.push(item.CharacterId);
				
		}
        return packet;
    }

    
    public void Clear()
    {
        this.Characters.Clear();
		
    }
}

public class C_UseSkill : IPacket
{
    public float posX;public float posY;public float posZ;public float rotY;public int skillId;

    public short Protocol { get { return (short)PacketID.C_UseSkill; } }

    public void Read(CPacket packet)
    {
        this.posX = packet.pop_float();this.posY = packet.pop_float();this.posZ = packet.pop_float();this.rotY = packet.pop_float();this.skillId = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_UseSkill);
        packet.push(this.posX);packet.push(this.posY);packet.push(this.posZ);packet.push(this.rotY);packet.push(this.skillId);
        return packet;
    }

    
    public void Clear()
    {
        this.posX = 0f;
		this.posY = 0f;
		this.posZ = 0f;
		this.rotY = 0f;
		this.skillId = 0;
		
    }
}

public class S_UseSkill : IPacket
{
    public int playerid;public float posX;public float posY;public float posZ;public float rotY;public float dirx;public float dirz;public int skillId;public bool Cooldown;

    public short Protocol { get { return (short)PacketID.S_UseSkill; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.posX = packet.pop_float();this.posY = packet.pop_float();this.posZ = packet.pop_float();this.rotY = packet.pop_float();this.dirx = packet.pop_float();this.dirz = packet.pop_float();this.skillId = packet.pop_int32();this.Cooldown = packet.pop_bool();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_UseSkill);
        packet.push(this.playerid);packet.push(this.posX);packet.push(this.posY);packet.push(this.posZ);packet.push(this.rotY);packet.push(this.dirx);packet.push(this.dirz);packet.push(this.skillId);packet.push(this.Cooldown);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.posX = 0f;
		this.posY = 0f;
		this.posZ = 0f;
		this.rotY = 0f;
		this.dirx = 0f;
		this.dirz = 0f;
		this.skillId = 0;
		this.Cooldown = false;
		
    }
}

public class Spawn_Object : IPacket
{
    public struct ObjectsItem
	{
	    public float SpawnX;
		public float SpawnY;
		public float SpawnZ;
		public int userid;
		public byte typeid;
		
	}
	public List<ObjectsItem> Objects = new List<ObjectsItem>();

    public short Protocol { get { return (short)PacketID.Spawn_Object; } }

    public void Read(CPacket packet)
    {
        this.Objects.Clear();
		int ObjectsCount = packet.pop_int32();
		for (int i = 0; i < ObjectsCount; i++)
		{
		    ObjectsItem item = new ObjectsItem();
		    item.SpawnX = packet.pop_float();
				item.SpawnY = packet.pop_float();
				item.SpawnZ = packet.pop_float();
				item.userid = packet.pop_int32();
				item.typeid = packet.pop_byte();
				
		    this.Objects.Add(item);
		}
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.Spawn_Object);
        packet.push(this.Objects.Count);
		foreach (ObjectsItem item in this.Objects)
		{
		    packet.push(item.SpawnX);
				packet.push(item.SpawnY);
				packet.push(item.SpawnZ);
				packet.push(item.userid);
				packet.push(item.typeid);
				
		}
        return packet;
    }

    
    public void Clear()
    {
        this.Objects.Clear();
		
    }
}

public class S_ChangeState : IPacket
{
    public int playerid;public byte state;

    public short Protocol { get { return (short)PacketID.S_ChangeState; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.state = packet.pop_byte();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_ChangeState);
        packet.push(this.playerid);packet.push(this.state);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.state = 0;
		
    }
}

public class C_CreateCharacter : IPacket
{
    public string Nickname;public byte CharacterType;

    public short Protocol { get { return (short)PacketID.C_CreateCharacter; } }

    public void Read(CPacket packet)
    {
        this.Nickname = packet.pop_string();this.CharacterType = packet.pop_byte();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_CreateCharacter);
        packet.push(this.Nickname);packet.push(this.CharacterType);
        return packet;
    }

    
    public void Clear()
    {
        this.Nickname = null;
		this.CharacterType = 0;
		
    }
}

public class S_CreateCharacter : IPacket
{
    public bool Success;public string Nickname;public byte CharacterType;

    public short Protocol { get { return (short)PacketID.S_CreateCharacter; } }

    public void Read(CPacket packet)
    {
        this.Success = packet.pop_bool();this.Nickname = packet.pop_string();this.CharacterType = packet.pop_byte();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_CreateCharacter);
        packet.push(this.Success);packet.push(this.Nickname);packet.push(this.CharacterType);
        return packet;
    }

    
    public void Clear()
    {
        this.Success = false;
		this.Nickname = null;
		this.CharacterType = 0;
		
    }
}

public class C_DeleteCharacter : IPacket
{
    public int CharacterId;

    public short Protocol { get { return (short)PacketID.C_DeleteCharacter; } }

    public void Read(CPacket packet)
    {
        this.CharacterId = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_DeleteCharacter);
        packet.push(this.CharacterId);
        return packet;
    }

    
    public void Clear()
    {
        this.CharacterId = 0;
		
    }
}

public class S_MMOSpawnItem : IPacket
{
    public int ItemID;public int entityID;public float posX;public float posY;public float posZ;

    public short Protocol { get { return (short)PacketID.S_MMOSpawnItem; } }

    public void Read(CPacket packet)
    {
        this.ItemID = packet.pop_int32();this.entityID = packet.pop_int32();this.posX = packet.pop_float();this.posY = packet.pop_float();this.posZ = packet.pop_float();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_MMOSpawnItem);
        packet.push(this.ItemID);packet.push(this.entityID);packet.push(this.posX);packet.push(this.posY);packet.push(this.posZ);
        return packet;
    }

    
    public void Clear()
    {
        this.ItemID = 0;
		this.entityID = 0;
		this.posX = 0f;
		this.posY = 0f;
		this.posZ = 0f;
		
    }
}

public class C_MMOGetItem : IPacket
{
    public int entityid;

    public short Protocol { get { return (short)PacketID.C_MMOGetItem; } }

    public void Read(CPacket packet)
    {
        this.entityid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_MMOGetItem);
        packet.push(this.entityid);
        return packet;
    }

    
    public void Clear()
    {
        this.entityid = 0;
		
    }
}

public class S_MMOGetItem : IPacket
{
    public int entityid;public int playerid;

    public short Protocol { get { return (short)PacketID.S_MMOGetItem; } }

    public void Read(CPacket packet)
    {
        this.entityid = packet.pop_int32();this.playerid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_MMOGetItem);
        packet.push(this.entityid);packet.push(this.playerid);
        return packet;
    }

    
    public void Clear()
    {
        this.entityid = 0;
		this.playerid = 0;
		
    }
}

public class S_MMODespawnItem : IPacket
{
    public int entityID;

    public short Protocol { get { return (short)PacketID.S_MMODespawnItem; } }

    public void Read(CPacket packet)
    {
        this.entityID = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_MMODespawnItem);
        packet.push(this.entityID);
        return packet;
    }

    
    public void Clear()
    {
        this.entityID = 0;
		
    }
}

public class S_UpdateSlot : IPacket
{
    public short SlotIndex;public int entityid;public int TemplateID;public int Count;

    public short Protocol { get { return (short)PacketID.S_UpdateSlot; } }

    public void Read(CPacket packet)
    {
        this.SlotIndex = packet.pop_int16();this.entityid = packet.pop_int32();this.TemplateID = packet.pop_int32();this.Count = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_UpdateSlot);
        packet.push(this.SlotIndex);packet.push(this.entityid);packet.push(this.TemplateID);packet.push(this.Count);
        return packet;
    }

    
    public void Clear()
    {
        this.SlotIndex = 0;
		this.entityid = 0;
		this.TemplateID = 0;
		this.Count = 0;
		
    }
}

public class C_SwapItem : IPacket
{
    public short FromIndex;public short ToIndex;

    public short Protocol { get { return (short)PacketID.C_SwapItem; } }

    public void Read(CPacket packet)
    {
        this.FromIndex = packet.pop_int16();this.ToIndex = packet.pop_int16();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_SwapItem);
        packet.push(this.FromIndex);packet.push(this.ToIndex);
        return packet;
    }

    
    public void Clear()
    {
        this.FromIndex = 0;
		this.ToIndex = 0;
		
    }
}

public class C_NpcInteract : IPacket
{
    public int NpcId;

    public short Protocol { get { return (short)PacketID.C_NpcInteract; } }

    public void Read(CPacket packet)
    {
        this.NpcId = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_NpcInteract);
        packet.push(this.NpcId);
        return packet;
    }

    
    public void Clear()
    {
        this.NpcId = 0;
		
    }
}

public class S_NpcInteractResult : IPacket
{
    public int NpcId;public struct AvailableQuestsItem
	{
	    public int QuestId;
		
	}
	public List<AvailableQuestsItem> AvailableQuests = new List<AvailableQuestsItem>();public struct CompletableQuestsItem
	{
	    public int QuestId;
		
	}
	public List<CompletableQuestsItem> CompletableQuests = new List<CompletableQuestsItem>();

    public short Protocol { get { return (short)PacketID.S_NpcInteractResult; } }

    public void Read(CPacket packet)
    {
        this.NpcId = packet.pop_int32();this.AvailableQuests.Clear();
		int AvailableQuestsCount = packet.pop_int32();
		for (int i = 0; i < AvailableQuestsCount; i++)
		{
		    AvailableQuestsItem item = new AvailableQuestsItem();
		    item.QuestId = packet.pop_int32();
				
		    this.AvailableQuests.Add(item);
		}this.CompletableQuests.Clear();
		int CompletableQuestsCount = packet.pop_int32();
		for (int i = 0; i < CompletableQuestsCount; i++)
		{
		    CompletableQuestsItem item = new CompletableQuestsItem();
		    item.QuestId = packet.pop_int32();
				
		    this.CompletableQuests.Add(item);
		}
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_NpcInteractResult);
        packet.push(this.NpcId);packet.push(this.AvailableQuests.Count);
		foreach (AvailableQuestsItem item in this.AvailableQuests)
		{
		    packet.push(item.QuestId);
				
		}packet.push(this.CompletableQuests.Count);
		foreach (CompletableQuestsItem item in this.CompletableQuests)
		{
		    packet.push(item.QuestId);
				
		}
        return packet;
    }

    
    public void Clear()
    {
        this.NpcId = 0;
		this.AvailableQuests.Clear();
		this.CompletableQuests.Clear();
		
    }
}

public class C_AcceptQuest : IPacket
{
    public int NpcId;public int QuestId;

    public short Protocol { get { return (short)PacketID.C_AcceptQuest; } }

    public void Read(CPacket packet)
    {
        this.NpcId = packet.pop_int32();this.QuestId = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_AcceptQuest);
        packet.push(this.NpcId);packet.push(this.QuestId);
        return packet;
    }

    
    public void Clear()
    {
        this.NpcId = 0;
		this.QuestId = 0;
		
    }
}

public class S_AcceptQuest : IPacket
{
    public int QuestId;public int ErrorCode;

    public short Protocol { get { return (short)PacketID.S_AcceptQuest; } }

    public void Read(CPacket packet)
    {
        this.QuestId = packet.pop_int32();this.ErrorCode = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_AcceptQuest);
        packet.push(this.QuestId);packet.push(this.ErrorCode);
        return packet;
    }

    
    public void Clear()
    {
        this.QuestId = 0;
		this.ErrorCode = 0;
		
    }
}

public class S_UpdateQuestProgress : IPacket
{
    public int QuestId;public struct ObjectivesItem
	{
	    public int ObjectiveIndex;
		public int CurrentCount;
		
	}
	public List<ObjectivesItem> Objectives = new List<ObjectivesItem>();

    public short Protocol { get { return (short)PacketID.S_UpdateQuestProgress; } }

    public void Read(CPacket packet)
    {
        this.QuestId = packet.pop_int32();this.Objectives.Clear();
		int ObjectivesCount = packet.pop_int32();
		for (int i = 0; i < ObjectivesCount; i++)
		{
		    ObjectivesItem item = new ObjectivesItem();
		    item.ObjectiveIndex = packet.pop_int32();
				item.CurrentCount = packet.pop_int32();
				
		    this.Objectives.Add(item);
		}
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_UpdateQuestProgress);
        packet.push(this.QuestId);packet.push(this.Objectives.Count);
		foreach (ObjectivesItem item in this.Objectives)
		{
		    packet.push(item.ObjectiveIndex);
				packet.push(item.CurrentCount);
				
		}
        return packet;
    }

    
    public void Clear()
    {
        this.QuestId = 0;
		this.Objectives.Clear();
		
    }
}

public class C_CompleteQuest : IPacket
{
    public int NpcId;public int QuestId;

    public short Protocol { get { return (short)PacketID.C_CompleteQuest; } }

    public void Read(CPacket packet)
    {
        this.NpcId = packet.pop_int32();this.QuestId = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_CompleteQuest);
        packet.push(this.NpcId);packet.push(this.QuestId);
        return packet;
    }

    
    public void Clear()
    {
        this.NpcId = 0;
		this.QuestId = 0;
		
    }
}

public class S_CompleteQuest : IPacket
{
    public int QuestId;public int RewardItemId;public int RewardItemCount;public int ErrorCode;

    public short Protocol { get { return (short)PacketID.S_CompleteQuest; } }

    public void Read(CPacket packet)
    {
        this.QuestId = packet.pop_int32();this.RewardItemId = packet.pop_int32();this.RewardItemCount = packet.pop_int32();this.ErrorCode = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_CompleteQuest);
        packet.push(this.QuestId);packet.push(this.RewardItemId);packet.push(this.RewardItemCount);packet.push(this.ErrorCode);
        return packet;
    }

    
    public void Clear()
    {
        this.QuestId = 0;
		this.RewardItemId = 0;
		this.RewardItemCount = 0;
		this.ErrorCode = 0;
		
    }
}

public class S_GetExp : IPacket
{
    public long Exp;

    public short Protocol { get { return (short)PacketID.S_GetExp; } }

    public void Read(CPacket packet)
    {
        this.Exp = packet.pop_int64();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_GetExp);
        packet.push(this.Exp);
        return packet;
    }

    
    public void Clear()
    {
        this.Exp = 0;
		
    }
}

public class S_LevelUp : IPacket
{
    public int playerid;public int level;

    public short Protocol { get { return (short)PacketID.S_LevelUp; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.level = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_LevelUp);
        packet.push(this.playerid);packet.push(this.level);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.level = 0;
		
    }
}

public class C_UseItem : IPacket
{
    public short index;

    public short Protocol { get { return (short)PacketID.C_UseItem; } }

    public void Read(CPacket packet)
    {
        this.index = packet.pop_int16();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_UseItem);
        packet.push(this.index);
        return packet;
    }

    
    public void Clear()
    {
        this.index = 0;
		
    }
}

public class S_PlayerStatusUpdate : IPacket
{
    public int playerid;public int Hp;public int Mp;

    public short Protocol { get { return (short)PacketID.S_PlayerStatusUpdate; } }

    public void Read(CPacket packet)
    {
        this.playerid = packet.pop_int32();this.Hp = packet.pop_int32();this.Mp = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_PlayerStatusUpdate);
        packet.push(this.playerid);packet.push(this.Hp);packet.push(this.Mp);
        return packet;
    }

    
    public void Clear()
    {
        this.playerid = 0;
		this.Hp = 0;
		this.Mp = 0;
		
    }
}

public class C_UnequipItem : IPacket
{
    public short index;

    public short Protocol { get { return (short)PacketID.C_UnequipItem; } }

    public void Read(CPacket packet)
    {
        this.index = packet.pop_int16();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.C_UnequipItem);
        packet.push(this.index);
        return packet;
    }

    
    public void Clear()
    {
        this.index = 0;
		
    }
}

public class S_EquipUpdate : IPacket
{
    public short EquipType;public int TemplateID;

    public short Protocol { get { return (short)PacketID.S_EquipUpdate; } }

    public void Read(CPacket packet)
    {
        this.EquipType = packet.pop_int16();this.TemplateID = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_EquipUpdate);
        packet.push(this.EquipType);packet.push(this.TemplateID);
        return packet;
    }

    
    public void Clear()
    {
        this.EquipType = 0;
		this.TemplateID = 0;
		
    }
}

public class S_MyStatUpdate : IPacket
{
    public int Attack;public int MaxHp;

    public short Protocol { get { return (short)PacketID.S_MyStatUpdate; } }

    public void Read(CPacket packet)
    {
        this.Attack = packet.pop_int32();this.MaxHp = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_MyStatUpdate);
        packet.push(this.Attack);packet.push(this.MaxHp);
        return packet;
    }

    
    public void Clear()
    {
        this.Attack = 0;
		this.MaxHp = 0;
		
    }
}

public class S_MMOSpawnList : IPacket
{
    public struct ObjectsItem
	{
	    public int entityid;
		public float posX;
		public float posY;
		public float posZ;
		public float rotY;
		public byte state;
		public byte typeid;
		public int objectid;
		
	}
	public List<ObjectsItem> Objects = new List<ObjectsItem>();

    public short Protocol { get { return (short)PacketID.S_MMOSpawnList; } }

    public void Read(CPacket packet)
    {
        this.Objects.Clear();
		int ObjectsCount = packet.pop_int32();
		for (int i = 0; i < ObjectsCount; i++)
		{
		    ObjectsItem item = new ObjectsItem();
		    item.entityid = packet.pop_int32();
				item.posX = packet.pop_float();
				item.posY = packet.pop_float();
				item.posZ = packet.pop_float();
				item.rotY = packet.pop_float();
				item.state = packet.pop_byte();
				item.typeid = packet.pop_byte();
				item.objectid = packet.pop_int32();
				
		    this.Objects.Add(item);
		}
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_MMOSpawnList);
        packet.push(this.Objects.Count);
		foreach (ObjectsItem item in this.Objects)
		{
		    packet.push(item.entityid);
				packet.push(item.posX);
				packet.push(item.posY);
				packet.push(item.posZ);
				packet.push(item.rotY);
				packet.push(item.state);
				packet.push(item.typeid);
				packet.push(item.objectid);
				
		}
        return packet;
    }

    
    public void Clear()
    {
        this.Objects.Clear();
		
    }
}

public class S_MMODespawnList : IPacket
{
    public struct ObjectsItem
	{
	    public int entityid;
		
	}
	public List<ObjectsItem> Objects = new List<ObjectsItem>();

    public short Protocol { get { return (short)PacketID.S_MMODespawnList; } }

    public void Read(CPacket packet)
    {
        this.Objects.Clear();
		int ObjectsCount = packet.pop_int32();
		for (int i = 0; i < ObjectsCount; i++)
		{
		    ObjectsItem item = new ObjectsItem();
		    item.entityid = packet.pop_int32();
				
		    this.Objects.Add(item);
		}
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_MMODespawnList);
        packet.push(this.Objects.Count);
		foreach (ObjectsItem item in this.Objects)
		{
		    packet.push(item.entityid);
				
		}
        return packet;
    }

    
    public void Clear()
    {
        this.Objects.Clear();
		
    }
}

public class S_MMODespawn : IPacket
{
    public int entityid;

    public short Protocol { get { return (short)PacketID.S_MMODespawn; } }

    public void Read(CPacket packet)
    {
        this.entityid = packet.pop_int32();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_MMODespawn);
        packet.push(this.entityid);
        return packet;
    }

    
    public void Clear()
    {
        this.entityid = 0;
		
    }
}

public class S_MMOSpawn : IPacket
{
    public int entityid;public float posX;public float posY;public float posZ;public float rotY;public byte state;

    public short Protocol { get { return (short)PacketID.S_MMOSpawn; } }

    public void Read(CPacket packet)
    {
        this.entityid = packet.pop_int32();this.posX = packet.pop_float();this.posY = packet.pop_float();this.posZ = packet.pop_float();this.rotY = packet.pop_float();this.state = packet.pop_byte();
    }

    public CPacket Write()
    {
        CPacket packet = CPacket.create((short)PacketID.S_MMOSpawn);
        packet.push(this.entityid);packet.push(this.posX);packet.push(this.posY);packet.push(this.posZ);packet.push(this.rotY);packet.push(this.state);
        return packet;
    }

    
    public void Clear()
    {
        this.entityid = 0;
		this.posX = 0f;
		this.posY = 0f;
		this.posZ = 0f;
		this.rotY = 0f;
		this.state = 0;
		
    }
}

