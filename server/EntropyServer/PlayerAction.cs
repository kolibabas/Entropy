using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntropyServer
{
    class PlayerAction
    {
        public PLAYER_ACTION Type;
        public int Start_Tick;
        public int End_Tick;
        public int Param;   //Used to hold which ship is being built, structure constructed, research, etc
        public string Target_Player_Name;
    }
}
