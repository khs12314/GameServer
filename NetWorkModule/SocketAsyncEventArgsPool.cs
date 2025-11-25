using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NetWorkModule
{
    public class ScoketAsyncEventArgsPool
    {
        // SocketAsyncEventArgs를 담는 Stack 자료구조 (LIFO)
        private Stack<SocketAsyncEventArgs> m_pool;

        // capacity만큼의 초기 크기를 지정하여 스택을 생성
        public ScoketAsyncEventArgsPool(int capacity)
        {
            m_pool = new Stack<SocketAsyncEventArgs>(capacity);
            
        }

        // 풀에 SocketAsyncEventArgs 객체를 추가 (반납)
        public void Push(SocketAsyncEventArgs item)
        {
            // null인 객체는 추가할 수 없음
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }

            // 스레드 동기화를 위해 lock 사용 (멀티스레드 환경 대비)
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        // 풀에서 SocketAsyncEventArgs 객체를 꺼내서 반환 (사용)
        public SocketAsyncEventArgs Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        // 현재 풀에 저장된 객체 개수 확인용 프로퍼티
        public int Count
        {
            get { return m_pool.Count; }
        }
    }
}
