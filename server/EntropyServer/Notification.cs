using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntropyServer
{
    class Notification
    {
        private long ID;
        private string attacker_name;
        private string defender_name;
        private int attacker_strength;
        private int defender_strength;
        private PLAYER_ACTION action_type;
        private ACTION_RESULT action_result;
        private string attacker_ship_losses;
        private string defender_ship_losses;
        private string defender_structure_losses;

        private bool read;
    }
}
