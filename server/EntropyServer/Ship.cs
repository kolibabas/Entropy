using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntropyServer
{
    class Ship
    {
        public SHIP_TYPE type;
        public string name;
        public bool Defense_Mode;
        public int Upkeep;
        public int Cost;
        public int Build_Time;
        public int Speed;
        public int HP;

        public int Combat_Rating;
        public int Bonus_Tech;
        public int Bonus_Econ;
        public int Bonus_Diplomacy;

        public Ship() { }

        //Copy Constructor
        public Ship(Ship template)
        {
            this.type = template.type;
            this.name = template.name;
            this.Defense_Mode = template.Defense_Mode;
            this.Upkeep = template.Upkeep;
            this.Cost = template.Cost;
            this.Build_Time = template.Build_Time;
            this.Speed = template.Speed;
            this.HP = template.HP;
            this.Combat_Rating = template.Combat_Rating;
            this.Bonus_Tech = template.Bonus_Tech;
            this.Bonus_Econ = template.Bonus_Econ;
            this.Bonus_Diplomacy = template.Bonus_Diplomacy;
        }
    }
}
