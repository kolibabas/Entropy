using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntropyServer
{
    static class TechManager
    {
        public const int TECH_MAX_TIER = 5;

        public static int Tech_Mod(TECH_TYPE tech, int tier)
        {
            switch (tech)
            {
                case TECH_TYPE.TECH_SHIP_WEAPONS:
                    return Tech_Mod_TECH_SHIP_WEAPONS(tier);

                case TECH_TYPE.TECH_SHIP_HP:
                    return Tech_Mod_TECH_SHIP_HP(tier);

                case TECH_TYPE.TECH_SHIP_ENGINE:
                    return Tech_Mod_TECH_SHIP_ENGINE(tier);

                case TECH_TYPE.TECH_SHIP_STEALTH:
                    return Tech_Mod_TECH_SHIP_STEALTH(tier);

                case TECH_TYPE.TECH_STRUCT_WEAPONS:
                    return Tech_Mod_TECH_STRUCT_WEAPONS(tier);

                case TECH_TYPE.TECH_STRUCT_HP:
                    return Tech_Mod_TECH_STRUCT_HP(tier);

                case TECH_TYPE.TECH_STRUCT_ECON:
                    return Tech_Mod_TECH_STRUCT_MINING(tier);

                case TECH_TYPE.TECH_STRUCT_RESEARCH:
                    return Tech_Mod_TECH_STRUCT_RESEARCH(tier);

                case TECH_TYPE.TECH_CONSTRUCT_SPEED:
                    return Tech_Mod_TECH_CONSTRUCT_SPEED(tier);

                case TECH_TYPE.TECH_BASE_SENSORS:
                    return Tech_Mod_TECH_BASE_SENSORS(tier);

                case TECH_TYPE.TECH_BASE_FLEET_CAP:
                    return Tech_Mod_TECH_BASE_FLEET_CAP(tier);

                case TECH_TYPE.TECH_BASE_ASTEROID_CAP:
                    return Tech_Mod_TECH_BASE_ASTEROID_CAP(tier);

                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_SHIP_WEAPONS(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_SHIP_HP(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_SHIP_ENGINE(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_SHIP_STEALTH(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_STRUCT_WEAPONS(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_STRUCT_HP(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_STRUCT_MINING(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_STRUCT_RESEARCH(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_CONSTRUCT_SPEED(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_BASE_SENSORS(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 1;
                case 2:
                    return 1;
                case 3:
                    return 1;
                case 4:
                    return 1;
                case 5:
                    return 1;
                default:
                    return 1;
            }
        }

        private static int Tech_Mod_TECH_BASE_FLEET_CAP(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 50;
                case 2:
                    return 80;
                case 3:
                    return 120;
                case 4:
                    return 170;
                case 5:
                    return 230;
                default:
                    return 30;
            }
        }

        private static int Tech_Mod_TECH_BASE_ASTEROID_CAP(int tier)
        {
            switch (tier)
            {
                case 1:
                    return 10;
                case 2:
                    return 15;
                case 3:
                    return 20;
                case 4:
                    return 25;
                case 5:
                    return 30;
                default:
                    return 5;
            }
        }


    }
}
