﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EntropyServer
{
    public partial class MainForm : Form
    {
        private delegate void LogInvokeDelegate(string text);

        public static bool Shutdown_Requested = false;

        public MainForm()
        {
            InitializeComponent();

            Log("Welcome to Entropy. Enjoy your stay.");

            TimeManager.Initialize();
            ConnectionManager.Initialize(8080);
            PlayerManager.Initialize();

            MainForm.Log("Entropy Must Increase");
            MainForm.Log("Tick 0 Started at " + DateTime.Now.ToString());
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
                    TimeSpan t = TimeSpan.FromSeconds(TimeManager.Uptime_Secs);

                    Log("Server Uptime: " + t.ToString());
                    break;
                case "tick":
                case "ticks":
                case "t":
                    Log("Tick Info: Tick #" + TimeManager.Tick + " | Seconds Until Next Tick: " + TimeManager.Secs_Left_This_Tick);
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
                TimeManager.Shutdown();
                //TODO Write to database
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}