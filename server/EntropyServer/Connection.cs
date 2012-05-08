using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Data.SqlServerCe;
using System.Data;

namespace EntropyServer
{
    class Connection
    {
        public static int CONNECTION_TIMEOUT_SECS = 600;

        public TcpClient Client { get; set; }
        public int conn_num { get; set; }
        public string connected_player_name = "";

        private DateTime lastMessageTime;
        private bool authenticated = true;

        public Connection(TcpClient client, int conn_num)
        {
            this.Client = client;
            this.conn_num = conn_num;
            this.lastMessageTime = DateTime.Now;
        }

        public bool Is_Timed_Out()
        {
            TimeSpan span = DateTime.Now.Subtract(lastMessageTime);

            if (span.TotalSeconds > CONNECTION_TIMEOUT_SECS)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Parse_Input(string input)
        {
            //CMD_ID length is 3
            //PARAM length is 252
            if (input.Length == 255)
            {
                try
                {
                    string cmd_id_str = input.Substring(0, 3);
                    CMD_ID cmd_id = (CMD_ID)int.Parse(cmd_id_str);

                    string param_str = input.Substring(3);

                    Process_Input(cmd_id, param_str);
                }
                catch (Exception)
                {
                    MainForm.Log("Invalid Msg | Client " + conn_num + ": " + input);
                }
            }
            else
            {
                MainForm.Log("Invalid Msg Len | Client " + conn_num + ": " + input);
            }
        }

        private void Process_Input(CMD_ID cmd, string param)
        {
            lastMessageTime = DateTime.Now;

            if (authenticated == true || cmd == CMD_ID.CMD_AUTH)
            {
                switch (cmd)
                {
                    case CMD_ID.CMD_CREATE:
                        Create_Player(param);
                        break;
                    case CMD_ID.CMD_AUTH:
                        authenticated = false;
                        Authenticate(param);
                        break;
                    default:
                        MainForm.Log("Invalid Command from Client " + conn_num + ": " + (int)cmd);
                        break;
                }
            }
        }

        private void Create_Player(string player_str)
        {
            try
            {
                PLAYER_STRAT strategy = (PLAYER_STRAT)int.Parse(player_str.Substring(0, 2));
                string user_name = player_str.Substring(2, 100).Trim();
                string password = player_str.Substring(100, 152).Trim();

                Query query = new Query("select * from Player where user_name=\'" + user_name + "\'");
                if (query.result.Tables[0].Rows.Count != 0)
                {
                    //TODO Send username taken packet
                }
                else
                {
                    query = new Query("select * from Player");
                    query.result.Tables[0].Rows.Add(Player.Default_Player(query.result.Tables[0].Rows.Count, user_name, strategy));
                    query.Update();

                    PlayerManager.Create_Player(user_name);
                }
            }
            catch (Exception)
            {
                MainForm.Log("Error creating player with string: " + player_str);
            }
        }

        private void Authenticate(string auth_str)
        {
            string user_name = auth_str.Substring(0, 100).Trim();
            string password = auth_str.Substring(100, 152).Trim();

            //TODO Sanitize inputs!

            Query query = new Query("select * from Player where user_name=\'" + user_name + "\'");

            if (query.result.Tables[0].Rows.Count != 0 && query.result.Tables[0].Rows[0]["password_hash"].ToString() == password)
            {
                authenticated = true;
                connected_player_name = user_name;
            }
            else
            {
                connected_player_name = "";
                authenticated = false;
                MainForm.Log("Username " + user_name + " failed to authenticate");

                //TODO Send invalid authentication packet
            }
        }
    }
}
