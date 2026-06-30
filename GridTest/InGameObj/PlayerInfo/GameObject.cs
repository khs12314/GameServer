using GridTest.Room;
using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.InGameObj.PlayerInfo
{

    public class GameObject
    {
        public int Id { get; set; } // 방 내부에서의 고유 ID (Entity ID)
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotY { get; set; }
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int MaxHp { get; set; }
        public int MaxMp { get; set; }
        public struct Pos { public float x, z; }
        public int CellX { get; set; }
        public int CellY { get; set; }

        public GameRoom_Logic Room { get; set; }
        public List<GameObject> AOIObjects { get; set; } = new List<GameObject>();
        public PlayerState State { get; protected set; }
        public GameObjectType gameObjectType { get; set; }
        // GameObject나 Monster 클래스 내부의 명시적 함수
        public void ChangeStateAndBroadcast(PlayerState newState, GameRoom_Logic room)
        {
            // [Guard Clause] 이전 상태와 똑같다면 패킷을 쏘지 않고 즉시 리턴 (대역폭 낭비 방어)
            if (State == newState)
                return;

            // 1. 상태 갱신
            State = newState;

            // 2. 패킷 조립 (Zero-Allocation을 위한 struct 또는 Pool 사용 권장)
            S_ChangeState statePacket = new S_ChangeState();
            statePacket.playerid = Id;
            statePacket.state = (byte)State;
            Console.WriteLine($"상태 변화 {State}");
            // 3. 룸을 통한 브로드캐스트
            room.Broadcast(statePacket.Write(), this);
        }
        public virtual void Initialize()
        {
            State = PlayerState.IDLE;//예외적으로 GameObject는 초기화 시 IDLE 상태로 시작하도록 설정. Player나 Monster에서 별도의 초기화 로직이 있다면 그 안에서 다시 상태를 조정할 수 있음.
        }

    }
}


