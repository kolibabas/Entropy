using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntropyServer
{
    class Asteroid
    {
        public ASTEROID_SIZE Size;
        public int Slots;
        public int Build_Time;

        public Asteroid(ASTEROID_SIZE size)
        {
            this.Size = size;

            switch (size)
            {
                case ASTEROID_SIZE.ASTEROID_SMALL:
                    Slots = 2;
                    Build_Time = 4;
                    break;
                case ASTEROID_SIZE.ASTEROID_MEDIUM:
                    Slots = 4;
                    Build_Time = 6;
                    break;
                case ASTEROID_SIZE.ASTEROID_LARGE:
                    Slots = 6;
                    Build_Time = 8;
                    break;
            }
        }
    }
}
