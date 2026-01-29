using CSampleClient;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSampleClient
{
    public class SessionManager
    {

        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        int _sessionId = 0;
        Dictionary<int, CRemoteServerPeer> _sessions = new Dictionary<int, CRemoteServerPeer>();
        object _lock = new object();

        public PacketSession Generate()
        {
            lock (_lock)
            {
                int sessionId = ++_sessionId;

                CRemoteServerPeer session = new CRemoteServerPeer();
                session.Set_MyID(sessionId);
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");

                return session;
            }
        }

        public void SendLogin()
		{
			lock (_lock)
			{
				foreach (var session in _sessions.Values)
				{
                    C_Login_DB loginPacket = new C_Login_DB();
                    loginPacket.ID = $"Dummy_{session.Get_MyID()}"; // 약속된 형식
                    loginPacket.PassWord = "1234";
                    Console.WriteLine($"로그인{session.Get_MyID()}");
                    session.Send(loginPacket.Write());
                }
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

