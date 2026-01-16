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
	C_EnterGame_Snake = 18,
	C_Move = 19,
	S_Move = 20,
	
}

// 공통 인터페이스
public interface IPacket
{
	short Protocol { get; }
	void Read(CPacket packet);
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.PlayerInfoReq);
        packet.push(this.playerId);packet.push(this.name);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.TestMove);
        packet.push(this.posX);packet.push(this.posY);packet.push(this.posZ);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_Login);
        
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_EnterGame);
        packet.push(this.userId);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_LoadingComplete);
        
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_HitMole);
        packet.push(this.moleIndex);packet.push(this.UserID);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_LoginOk);
        packet.push(this.ServerUserID);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_ScoreUpdate);
        packet.push(this.score);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_StartLoading);
        packet.push(this.PlayerIndex);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_Spawn_Mole);
        packet.push(this.moleIndex);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_GameStart);
        packet.push(this.duration);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_PlayerInfo);
        packet.push(this.playerIndex);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_RoomRemoved);
        
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_Login_DB);
        packet.push(this.ID);packet.push(this.PassWord);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_Login_DB);
        packet.push(this.Sucess);packet.push(this.UserID);packet.push(this.Nickname);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_SignUp_DB);
        packet.push(this.ID);packet.push(this.PassWord);packet.push(this.nickname);
        return packet;
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
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_SignUp_DB);
        packet.push(this.Sucess);packet.push(this.nickname);
        return packet;
    }
}

public class S_EnterGame_Snake : IPacket
{
    public float posX;public float posY;public int userid;

    public short Protocol { get { return (short)PacketID.S_EnterGame_Snake; } }

    public void Read(CPacket packet)
    {
        this.posX = packet.pop_float();this.posY = packet.pop_float();this.userid = packet.pop_int32();
    }

    public CPacket Write()
    {
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_EnterGame_Snake);
        packet.push(this.posX);packet.push(this.posY);packet.push(this.userid);
        return packet;
    }
}

public class C_EnterGame_Snake : IPacket
{
    public int userId;

    public short Protocol { get { return (short)PacketID.C_EnterGame_Snake; } }

    public void Read(CPacket packet)
    {
        this.userId = packet.pop_int32();
    }

    public CPacket Write()
    {
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_EnterGame_Snake);
        packet.push(this.userId);
        return packet;
    }
}

public class C_Move : IPacket
{
    public int playerId;public int direction;

    public short Protocol { get { return (short)PacketID.C_Move; } }

    public void Read(CPacket packet)
    {
        this.playerId = packet.pop_int32();this.direction = packet.pop_int32();
    }

    public CPacket Write()
    {
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_Move);
        packet.push(this.playerId);packet.push(this.direction);
        return packet;
    }
}

public class S_Move : IPacket
{
    public int playerId;public float posX;public float posY;

    public short Protocol { get { return (short)PacketID.S_Move; } }

    public void Read(CPacket packet)
    {
        this.playerId = packet.pop_int32();this.posX = packet.pop_float();this.posY = packet.pop_float();
    }

    public CPacket Write()
    {
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.S_Move);
        packet.push(this.playerId);packet.push(this.posX);packet.push(this.posY);
        return packet;
    }
}

