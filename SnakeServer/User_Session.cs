using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    //접속한 유저의 세션(인 게임은 다루지 않음)
    public class User_Session
    {
        static int _UserIDGenerator = 0;
        CUserToken CUserToken;
        public int Uid { get; private set; }
       public User_Session(CUserToken token)
        {
            this.CUserToken = token;
            this.Uid = Interlocked.Increment(ref _UserIDGenerator);
        }
    }
}
