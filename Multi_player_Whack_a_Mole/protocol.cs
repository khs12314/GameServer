using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_player_Whack_a_Mole
{
    public enum PROTOCOL
    {
        BEGIN = 0,
        START_LOADING = 1,
        LOADING_COMPLETED = 2,

        GAME_START = 3,

        HIT_MOLE_REQ = 4,

        UP_MOLE = 5,

        ROOM_REMOVED= 6,

        ENTER_GAME_ROOM_REQ = 7,

        GAME_OVER = 8,

        END
    }
}
