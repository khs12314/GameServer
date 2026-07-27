using GridTest.InGameObj.PlayerInfo;
using GridTest.Network;
using GridTest.Room;
using GridTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace GridTest.Manager
{
    public class GridManager
    {
        // _cells[row(y), column(x)]
        HashSet<GameObject>[,] _cells;
        int _cellSize;      // 한 셀의 월드 단위 길이
        int _gridSizeX;     // 칸(열) 개수
        int _gridSizeY;     // 칸(행) 개수
        readonly object _lock = new object();
        GameRoom_Logic MyRoom;
        public GridManager(int mapSize, int cellSize, GameRoom_Logic room)
        {
            MyRoom = room;
            if (cellSize <= 0) throw new ArgumentOutOfRangeException(nameof(cellSize));
            _cellSize = cellSize;

            // 전체 맵 길이를 셀 크기로 나눈 올림 -> 셀 개수
            _gridSizeX = Math.Max(1, (mapSize + cellSize - 1) / cellSize);
            _gridSizeY = Math.Max(1, (mapSize + cellSize - 1) / cellSize);

            _cells = new HashSet<GameObject>[_gridSizeY, _gridSizeX];
            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x = 0; x < _gridSizeX; x++)
                {
                    _cells[y, x] = new HashSet<GameObject>();
                }
            }
        }
        public BorderDirection CheckBorder(int cellX, int cellY)
        {
            BorderDirection dir = BorderDirection.None;

            if (cellY == 0) dir |= BorderDirection.Down;
            else if (cellY == _gridSizeY - 1) dir |= BorderDirection.Up;

            if (cellX == 0) dir |= BorderDirection.Left;
            else if (cellX == _gridSizeX - 1) dir |= BorderDirection.Right;

            return dir;
        }
        public void Add(GameObject gameobject)
        {
            if (gameobject == null) throw new ArgumentNullException(nameof(gameobject));
            Console.WriteLine($"grid에 저장 ");
            int x = (int)(gameobject.PosX / _cellSize);
            int y = (int)(gameobject.PosZ / _cellSize);

            // 범위 검사
            if (x < 0 || x >= _gridSizeX || y < 0 || y >= _gridSizeY) return;

            gameobject.CellX = x;
            gameobject.CellY = y;


            var list = _cells[y, x];
            if (list == null)
            {
                list = new HashSet<GameObject>();
                _cells[y, x] = list;
            }
            list.Add(gameobject);
            if (gameobject is Player player)
            {
                GetNeighbors(player, player.AOIObjects);
                //추후 수정 예정
                //1번 이동한 본인에게 해당 aoi에 있는 유저 및 몬스터 정보 모두 전송
                S_MMOSpawnList TargetMMOSpawn = NetworkPacketPool<S_MMOSpawnList>.Pop();
                TargetMMOSpawn.Clear();
                TargetMMOSpawn.Objects.Add(new S_MMOSpawnList.ObjectsItem
                {
                    entityid = player.Id,
                    posX = player.PosX,
                    posZ = player.PosZ,
                    posY = player.PosY,
                    objectid = player.ObjectId,
                    rotY = player.RotY,
                    state = (byte)player.State,
                    typeid = (byte)player.gameObjectType

                });
                player.Room.Broadcast(TargetMMOSpawn.Write(), player);
                S_MMOSpawnList s_MMOSpawnList = NetworkPacketPool<S_MMOSpawnList>.Pop();
                s_MMOSpawnList.Clear();
                foreach (var obj in player.AOIObjects)
                {

                    

                    s_MMOSpawnList.Objects.Add(new S_MMOSpawnList.ObjectsItem
                    {
                        entityid = obj.Id,
                        posX = obj.PosX,
                        posZ = obj.PosZ,
                        posY = obj.PosY,
                        typeid = (byte)obj.gameObjectType,
                        rotY = obj.RotY,
                        state = (byte)obj.State,
                        objectid = obj.ObjectId,
                    });
                }
                player.Session.send(s_MMOSpawnList.Write());
                NetworkPacketPool<S_MMOSpawnList>.Push(TargetMMOSpawn);
                NetworkPacketPool<S_MMOSpawnList>.Push(s_MMOSpawnList);
            }

        }

        public void Remove(GameObject gameobject)
        {
            if (gameobject == null) return;

            int x = gameobject.CellX;
            int y = gameobject.CellY;

            // 방어: 유효한 캐시 인덱스인지 확인
            if (x < 0 || x >= _gridSizeX || y < 0 || y >= _gridSizeY) return;


            _cells[y, x]?.Remove(gameobject);

        }
        HashSet<GameObject> tempOld = new HashSet<GameObject>();
        HashSet<GameObject> tempNew = new HashSet<GameObject>();
        public void Move(Player player)
        {

            if (player == null) return;

            int newX = (int)(player.PosX / _cellSize);
            int newY = (int)(player.PosZ / _cellSize);

            if (newX < 0 || newX >= _gridSizeX || newY < 0 || newY >= _gridSizeY) return;

            int oldX = player.CellX;
            int oldY = player.CellY;
            tempNew.Clear();
            tempOld.Clear();
            GetNeighbors(player, tempOld);
            if (oldX == newX && oldY == newY) return;

            // 3. Job Queue 내부에서 실행된다고 가정하므로 무거운 lock은 제거했다.

            // 4. 기존 셀에서 제거 (HashSet이므로 O(1) 속도로 즉시 삭제됨)
            if (oldX >= 0 && oldX < _gridSizeX && oldY >= 0 && oldY < _gridSizeY)
            {
                _cells[oldY, oldX].Remove(player);
            }

            // 5. 새 셀에 추가 (미리 만들어뒀으므로 null 체크나 new 할당이 아예 필요 없다!)
            Add(player);
            GetNeighbors(player, tempNew);
            S_MMODespawnList s_MyDespawnList = NetworkPacketPool<S_MMODespawnList>.Pop();
            S_MMOSpawnList s_MySpawnList = NetworkPacketPool<S_MMOSpawnList>.Pop();
            s_MyDespawnList.Objects.Clear();
            s_MySpawnList.Objects.Clear();
            s_MyDespawnList.Clear();
            s_MyDespawnList.Clear();
            // 1. 디스폰(Despawn) 처리: 내 시야에서 멀어진 유저들
            foreach (var oldobj in tempOld)
            {
                if (!tempNew.Contains(oldobj)) // 새로운 셀에 없는 경우 (멀어짐)
                {
                    // ① 내 화면에서 oldobj를 지우기 위해 리스트에 담음
                    s_MyDespawnList.Objects.Add(new S_MMODespawnList.ObjectsItem { entityid = oldobj.Id });

                    // ② 핵심: 상대방(oldobj)의 화면에서도 나(player)를 지워줘야 함!
                    if (oldobj is Player targetPlayer)
                    {
                        S_MMODespawnList s_TargetDespawn = NetworkPacketPool<S_MMODespawnList>.Pop();
                        s_MyDespawnList.Clear();
                        s_TargetDespawn.Objects.Add(new S_MMODespawnList.ObjectsItem { entityid = player.Id });
                        targetPlayer.Session.send(s_TargetDespawn.Write());
                        NetworkPacketPool<S_MMODespawnList>.Push(s_TargetDespawn);
                    }
                }
            }
            // 나에게 디스폰 패킷 쏘기 (리스트에 데이터가 있을 때만)
            if (s_MyDespawnList.Objects.Count > 0)
            {
                player.Session.send(s_MyDespawnList.Write());
                NetworkPacketPool<S_MMODespawnList>.Push(s_MyDespawnList);
            }


            // 2. 스폰(Spawn) 처리: 내 시야에 새로 들어온 유저들
            foreach (var newobj in tempNew)
            {
                // 자기 자신은 제외
                if (newobj.Id == player.Id) continue;

                if (!tempOld.Contains(newobj)) // 이전 셀에 없었던 경우 (새로 만남)
                {

                    s_MySpawnList.Objects.Add(new S_MMOSpawnList.ObjectsItem
                    {
                        entityid = newobj.Id,
                        posX = newobj.PosX,
                        posY = newobj.PosY,
                        posZ = newobj.PosZ,
                        rotY = newobj.RotY,
                        state = (byte)newobj.State
                    });


                    if (newobj is Player targetPlayer)
                    {
                        S_MMOSpawnList s_TargetSpawn = NetworkPacketPool<S_MMOSpawnList>.Pop();
                        s_MyDespawnList.Clear();
                        s_TargetSpawn.Objects.Add(new S_MMOSpawnList.ObjectsItem
                        {
                            entityid = player.Id,
                            posX = player.PosX,
                            posY = player.PosY,
                            posZ = player.PosZ,
                            rotY = player.RotY,
                            state = (byte)player.State
                        });
                        targetPlayer.Session.send(s_TargetSpawn.Write());
                        NetworkPacketPool<S_MMOSpawnList>.Push(s_TargetSpawn);
                    }
                }
            }
            // 나에게 스폰 패킷 쏘기 (리스트에 데이터가 있을 때만)
            if (s_MySpawnList.Objects.Count > 0)
            {
                player.Session.send(s_MySpawnList.Write());
                NetworkPacketPool<S_MMOSpawnList>.Push(s_MySpawnList);
            }
        }

        public void GetNeighbors(GameObject gameobject, ICollection<GameObject> outBuffer)
        {
            outBuffer.Clear();


            if (gameobject == null) return;

            int centerX = gameobject.CellX;
            int centerY = gameobject.CellY;

            // 방어: 캐시가 유효한지 확인
            if (centerX < 0 || centerX >= _gridSizeX || centerY < 0 || centerY >= _gridSizeY) return;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nextX = centerX + dx;
                    int nextY = centerY + dy;

                    if (nextX < 0 || nextX >= _gridSizeX || nextY < 0 || nextY >= _gridSizeY) continue;

                    var list = _cells[nextY, nextX];
                    if (list == null) continue;

                    foreach (GameObject obj in list)
                    {
                        outBuffer.Add(obj);
                    }
                }
            }
        }
    }
}
