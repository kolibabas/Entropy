using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EntropyServer.Engine;

namespace EntropyServer
{
    public partial class MainForm : Form
    {
        private delegate void LogInvokeDelegate(string text);

        private GameClock gameClock;

        public static bool Shutdown_Requested = false;

        public MainForm()
        {
            InitializeComponent();

            Log("Welcome to Entropy. Enjoy your stay.");

            this.gameClock = new GameClock(TimeSpan.FromSeconds(5.0D));
            this.gameClock.TickEvent += new GameClock.TickHandler(gameClock_TickEvent);
            this.gameClock.Start();
            ConnectionManager.Initialize( this.gameClock, 8080 );
            PlayerManager.Initialize(this.gameClock);

            MainForm.Log("Entropy Must Increase");
            outputTickStamp(0);
        }

        void gameClock_TickEvent(object sender, TickEventArgs args)
        {
            outputTickStamp(args.CurrentTick);
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (inputTextBox.Text.Length != 0)
                {
                    process_command_text(inputTextBox.Text);
                }

                inputTextBox.Text = "";               
            }
        }

        private void process_command_text(string text)
        {
            string command = text.ToLower();

            //TODO Accept params

            switch (command)
            {
                case "help":
                case "h":
                    Log("Available Commands (Not case sensitive):" + Environment.NewLine +
                                    "\t(H) Help\t- Shows this message" + Environment.NewLine +
                                    "\t(U) Uptime - Shows current server uptime in seconds" + Environment.NewLine +
                                    "\t(T) Tick\t- Shows Tick Information" + Environment.NewLine +
                                    "\t(C) Client\t- Shows Client Information"
                                );
                    break;
                case "uptime":
                case "u":
                    TimeSpan t = this.gameClock.RunningTime;

                    Log("Server Uptime: " + t.ToString());
                    break;
                case "tick":
                case "ticks":
                case "t":
                    Log("Tick Info: Tick #" + this.gameClock.CurrentTick + " | Seconds Until Next Tick: " + this.gameClock.CurrentTickTimeLeft);
                    break;
                case "client":
                case "clients":
                case "c":
                    Log("Clients Connected: " + ConnectionManager.Connections.Count);
                    break;
                default:
                    Log("Unsupported command: \"" + text + "\", type help for available commands.");
                    break;
            }
        }

        public static void Log(string text)
        {
            if (outputTextBox.InvokeRequired)
            {
                outputTextBox.BeginInvoke(new LogInvokeDelegate(Log), new object[] { text });
            }
            else
            {
                outputTextBox.Text += text + Environment.NewLine;
                outputTextBox.SelectionStart = outputTextBox.Text.Length;
                outputTextBox.ScrollToCaret();
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            inputTextBox.Focus();
        }

        private void outputTextBox_Enter(object sender, EventArgs e)
        {
            inputTextBox.Focus();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to shut down the server?", "Shutdown Requested", MessageBoxButtons.YesNo);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Log("Server Shutting Down!");
                Shutdown_Requested = true;
                System.Threading.Thread.Sleep(2000);
                ConnectionManager.Shutdown();
                gameClock.Stop();
                //TODO Write to database
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void outputTickStamp(int tickNumber)
        {
            MainForm.Log(String.Format("Tick {0:d} started at {1:s}", tickNumber, DateTime.Now.ToString()));
        }
    }
}
