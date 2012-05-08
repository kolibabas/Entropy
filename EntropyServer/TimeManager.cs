using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EntropyServer
{
    static class TimeManager
    {
        public const int TICK_LEN_SECS = 5;

        public static int Tick { get; set; }
        public static long Uptime_Secs { get; set; }
        public static int Secs_Left_This_Tick { get; set; }

        private static int lastSecond;
        private static Thread clockThread;

        public delegate void TickHandler();
        public static event TickHandler TickEvent;

        static public void OnTickEvent()
        {
            if (TickEvent != null)
            {
                TickEvent();
            }
        }

        public delegate void SecondChangeHandler();
        public static event SecondChangeHandler SecondChangeEvent;

        static public void OnSecondChangeEvent()
        {
            if (SecondChangeEvent != null)
            {
                SecondChangeEvent();
            }
        }

        public static void Initialize()
        {
            lastSecond = 0;
            Tick = 0;
            Uptime_Secs = 0;
            Secs_Left_This_Tick = TICK_LEN_SECS;

            clockThread = new Thread(Run);
            clockThread.Start();

            MainForm.Log("TimeManager Initialized");
    
        }

        public static void Shutdown()
        {
            clockThread.Abort();
        }

        public static void Run()
        {
            while(!MainForm.Shutdown_Requested)
            {
                // Sleep 1 Second
                Thread.Sleep(1000);

                // Get the current time
                System.DateTime dt = System.DateTime.Now;

                //If the second changed
                if (dt.Second != lastSecond)
                {
                    OnSecondChangeEvent();

                    Uptime_Secs++;
                    Secs_Left_This_Tick--;

                    if (Secs_Left_This_Tick <= 0)
                    {
                        //TODO Lock TickComputing
                        OnTickEvent();
                        //TODO Unlock TickComputing
                        
                        //TODO Do Tick
                        Tick++;
                        Secs_Left_This_Tick = TICK_LEN_SECS;
                        
                        MainForm.Log("Tick " + Tick + " Started at " + DateTime.Now.ToString());
                    }

                }

                lastSecond = dt.Second;
            }
        }
    }
}
