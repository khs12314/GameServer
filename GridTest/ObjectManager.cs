using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest
{
  
    public static class ObjectManager
    {
        private static int _playerCounter = 0;
        private static int _monsterCounter = 0;
        private static int _projectileCounter = 0;
        private static int _itemCounter = 0;
        public static int GenerateId(GameObjectType type)
        {
            int counter = 0;
            switch (type)
            {
                case GameObjectType.Player:
                    counter = Interlocked.Increment(ref _playerCounter);
                    break;
                case GameObjectType.Monster:
                    counter = Interlocked.Increment(ref _monsterCounter);
                    break;
                case GameObjectType.Projectile:
                    counter = Interlocked.Increment(ref _projectileCounter); // Using monster counter for projectiles as well
                    break;
                case GameObjectType.Item:
                    counter = Interlocked.Increment(ref _itemCounter);
                    break;
            }

            Console.WriteLine($"발급 id {((int)type << 24) | (counter & 0x00FFFFFF)}");
            return ((int)type << 24) | (counter & 0x00FFFFFF);
        }
        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }
    }
}
