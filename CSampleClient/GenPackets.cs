using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using NetWorkModule;

public enum PacketID
{
    PlayerInfoReq = 0,
	TestMove = 1,
	C_EnterGame = 2,
	C_LoadingComplete = 3,
	C_HitMole = 4,
	S_ScoreUpdate = 5,
	
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

public class C_EnterGame : IPacket
{
    public int tempId;

    public short Protocol { get { return (short)PacketID.C_EnterGame; } }

    public void Read(CPacket packet)
    {
        this.tempId = packet.pop_int32();
    }

    public CPacket Write()
    {
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_EnterGame);
        packet.push(this.tempId);
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
    public int gameRoomId;public int moleIndex;

    public short Protocol { get { return (short)PacketID.C_HitMole; } }

    public void Read(CPacket packet)
    {
        this.gameRoomId = packet.pop_int32();this.moleIndex = packet.pop_int32();
    }

    public CPacket Write()
    {
        // 자네가 만든 CPacket.create 사용
        CPacket packet = CPacket.create((short)PacketID.C_HitMole);
        packet.push(this.gameRoomId);packet.push(this.moleIndex);
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

