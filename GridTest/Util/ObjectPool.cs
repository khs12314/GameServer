using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Util
{
    public interface IPoolable
    {
        void Reset();
    }
    public class ObjectPool<T> where T : class, IPoolable, new()
    {
       private static readonly ConcurrentBag<T> _pool = new ConcurrentBag<T>();
        public static T Pop()
        {
            if (_pool.TryTake(out T item))
            {
                return item;
            }
            else
            {
                return new T();
            }
        }
        public static void Push(T item)
        {
            item.Reset();
            _pool.Add(item);
        }
    }
}
