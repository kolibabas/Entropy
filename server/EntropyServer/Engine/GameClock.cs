using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace EntropyServer.Engine
{
    public class GameClock
    {
        //Constants
        private const int DefaultTickInSeconds = 5;

        //Events
        public delegate void TickHandler(object sender, TickEventArgs args);
        public event TickHandler TickEvent;
        public delegate void SecondElapsedHandler(object sender, SecondElapsedEventArgs args);
        public event SecondElapsedHandler SecondElapsedEvent;

        //Private data
        private int tick;
        private TimeSpan runningTime;
        private Timer secondTimer;
        private Timer tickTimer;
        private TimeSpan currentTickCountdown;
        private TimeSpan tickDurationUsed; //tick can be changed after it is started
        private static System.Threading.Mutex mut = new System.Threading.Mutex();

        //Read/write properties
        public TimeSpan TickDuration
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
        public TimeSpan RunningTime
        {
            get
            {
                return this.runningTime;
            }
        }
        public TimeSpan CurrentTickTimeLeft
        {
            get
            {
                return this.currentTickCountdown;
            }
        }


        public GameClock(TimeSpan tickDuration)
        {
            this.tick = 0;
            this.runningTime = TimeSpan.FromSeconds(0.0D);
            this.currentTickCountdown = TimeSpan.FromSeconds(0.0D);
            this.tickTimer = null;
            this.secondTimer = null;
            this.TickDuration = tickDuration;
        }

        public GameClock() : this(TimeSpan.FromSeconds((double)DefaultTickInSeconds))
        {}

        //Public functions
        public void Start()
        {
            this.tick = 0;
            this.runningTime = TimeSpan.FromSeconds(0.0D);

            // set a tick that only gets saved when started so it
            // stays the same while running
            this.tickDurationUsed = this.TickDuration;
            this.currentTickCountdown = this.tickDurationUsed;

            this.secondTimer = new Timer(TimeSpan.FromSeconds(1.0D).TotalMilliseconds);
            this.tickTimer = new Timer(this.tickDurationUsed.TotalMilliseconds);

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
            this.runningTime = TimeSpan.FromSeconds(0.0D);
            this.currentTickCountdown = TimeSpan.FromSeconds(0.0D);
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

            TickEventArgs eventArgs = new TickEventArgs();
            eventArgs.CurrentTick = this.CurrentTick;
            eventArgs.RunningTime = this.RunningTime;

            mut.ReleaseMutex();

            //the user callbacks are not in protected code
            if (this.TickEvent != null)
            {
                this.TickEvent(this, eventArgs);
            }
        }

        private void OnSecondElapsedEvent(object sender, ElapsedEventArgs e)
        {
            //protected code while modifying object state
            mut.WaitOne();

            this.runningTime += TimeSpan.FromMilliseconds(((Timer)sender).Interval);
            this.currentTickCountdown -= TimeSpan.FromMilliseconds(((Timer)sender).Interval);
            
            SecondElapsedEventArgs eventArgs = new SecondElapsedEventArgs();
            eventArgs.CurrentTick = this.CurrentTick;
            eventArgs.CurrentTickTimeLeft = this.CurrentTickTimeLeft;
            eventArgs.RunningTime = this.RunningTime;

            mut.ReleaseMutex();

            //the user callbacks are not in protected code
            if (this.SecondElapsedEvent != null)
            {
                this.SecondElapsedEvent(this, eventArgs);
            }
        }

    }

    public class TickEventArgs
    {
        public int CurrentTick
        { get; set; }

        public TimeSpan RunningTime
        { get; set; }
    }

    public class SecondElapsedEventArgs
    {
        public int CurrentTick
        { get; set; }

        public TimeSpan CurrentTickTimeLeft
        { get; set; }

        public TimeSpan RunningTime
        { get; set; }
    }
}
