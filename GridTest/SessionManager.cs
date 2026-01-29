using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    public class SessionManager
    {

        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        int _sessionId = 0;
        Dictionary<int, User_Session> _sessions = new Dictionary<int, User_Session>();
        object _lock = new object();

        public PacketSession Generate()
        {
            lock (_lock)
            {
                int sessionId = ++_sessionId;

                User_Session session = new User_Session();
                session.Set_MyID(sessionId);
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");

                return session;
            }
        }

        public User_Session Find(int id)
        {
            lock (_lock)
            {
                User_Session session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(PacketSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.Get_MyID());
            }
        }
    }
}

