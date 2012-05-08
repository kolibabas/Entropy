using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntropyServer
{
    class Structure
    {
        public STRUCTURE_TYPE Type;
        public string name;
        public int Upkeep;
        public int HP;
        public int Cost;
        public int Build_Time;

        public int Bonus_Combat;
        public int Bonus_Tech;
        public int Bonus_Econ;
        public int Bonus_Diplomacy;

        public Structure() { }

        public Structure(Structure template)
        {
            this.Type = template.Type;
            this.name = template.name;
            this.Upkeep = template.Upkeep;
            this.HP = template.HP;
            this.Cost = template.Cost;
            this.Build_Time = template.Build_Time;
            this.Bonus_Combat = template.Bonus_Combat;
            this.Bonus_Tech = template.Bonus_Tech;
            this.Bonus_Econ = template.Bonus_Econ;
            this.Bonus_Diplomacy = template.Bonus_Diplomacy;
        }
    }
}
