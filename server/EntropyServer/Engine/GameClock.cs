﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace EntropyServer.Engine
{
    public class GameClock
    {
        //Constants
        private const int DefaultTick = 5;
        private const int MillisecondsInOneSecond = 1000;

        //Events
        public delegate void TickHandler(object sender, TickEventArgs args);
        public event TickHandler TickEvent;
        public delegate void SecondElapsedHandler(object sender, SecondElapsedEventArgs args);
        public event SecondElapsedHandler SecondElapsedEvent;

        //Private data
        private int tick;
        private int runningTime;
        private Timer secondTimer;
        private Timer tickTimer;
        private int currentTickCountdown;
        private int tickDurationUsed; //tick can be changed after it is started
        private static System.Threading.Mutex mut = new System.Threading.Mutex();

        //Read/write properties
        public int TickDurationInSeconds
        {
            get;
            set;
        }

        //Read only properties
        public int CurrentTick
        {
            get
            {
                return this.tick;
            }
        }
        public int RunningTime
        {
            get
            {
                return this.runningTime;
            }
        }
        public int CurrentTickSecondsLeft
        {
            get
            {
                return this.currentTickCountdown;
            }
        }


        public GameClock(int tickDuration)
        {
            this.tick = 0;
            this.runningTime = 0;
            this.currentTickCountdown = 0;
            this.tickTimer = null;
            this.secondTimer = null;
            this.TickDurationInSeconds = tickDuration;
        }

        public GameClock() : this(DefaultTick)
        {}

        //Public functions
        public void Start()
        {
            this.tick = 0;
            this.runningTime = 0;

            // set a tick that only gets saved when started so it
            // stays the same while running
            this.tickDurationUsed = this.TickDurationInSeconds;
            this.currentTickCountdown = this.tickDurationUsed;

            this.secondTimer = new Timer(1 * MillisecondsInOneSecond);
            this.tickTimer = new Timer(this.tickDurationUsed * MillisecondsInOneSecond);

            this.tickTimer.Elapsed += new ElapsedEventHandler(OnTickEvent);
            this.secondTimer.Elapsed += new ElapsedEventHandler(OnSecondElapsedEvent);

            this.tickTimer.Enabled = true;
            this.secondTimer.Enabled = true;
            this.tickTimer.Start();
            this.secondTimer.Start();
        }

        public void Stop()
        {
            this.tickTimer.Enabled = false;
            this.secondTimer.Enabled = false;

            //reset internal data so time remaining and such is zeroed
            this.tick = 0;
            this.runningTime = 0;
            this.currentTickCountdown = 0;
        }

        public void Pause()
        {
            //just turn off the timers, leave state data
            this.tickTimer.Enabled = false;
            this.secondTimer.Enabled = false;
        }

        public void Resume()
        {
            //turn timers back on and leave state alone
            this.tickTimer.Enabled = true;
            this.secondTimer.Enabled = true;
        }

        //Private functions
        private void OnTickEvent(object sender, ElapsedEventArgs e)
        {
            //protected code while modifying object state
            mut.WaitOne();

            this.tick += 1;

            //restart the tick countdown
            this.currentTickCountdown = this.tickDurationUsed;

            mut.ReleaseMutex();


            //the user callbacks are not in protected code
            if (this.TickEvent != null)
            {
                this.TickEvent(this, new TickEventArgs());
            }
        }

        private void OnSecondElapsedEvent(object sender, ElapsedEventArgs e)
        {
            //protected code while modifying object state
            mut.WaitOne();

            this.runningTime += 1;
            this.currentTickCountdown--;

            mut.ReleaseMutex();


            //the user callbacks are not in protected code
            if (this.SecondElapsedEvent != null)
            {
                this.SecondElapsedEvent(this, new SecondElapsedEventArgs());
            }
        }

    }

    public class TickEventArgs
    {
        //TODO: add arguments for tick event.
        //ideas: tick number, time in world
    }

    public class SecondElapsedEventArgs
    {
        //TODO: add arguments for tick event
        //ideas: tick number, time left in tick, time in world
    }
}