using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Data;

namespace EntropyServer
{
    static class PlayerManager
    {
        //Holds all known players
        public static Dictionary<string, Player> Players = new Dictionary<string, Player>();


        public static void Initialize()
        {
            TimeManager.TickEvent += new TimeManager.TickHandler(On_Tick);

            SqlCeDataAdapter adapter = new SqlCeDataAdapter("select * from Player", ConnectionManager.SQLConnection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);

            foreach (DataRow player_row in ds.Tables[0].Rows)
            {
                Players.Add(player_row["user_name"].ToString(), new Player(player_row["user_name"].ToString()));
            }

            MainForm.Log("PlayerManager Initialized");
        }

        public static void Create_Player(string user_name)
        {
            Players.Add(user_name, new Player(user_name));
        }

        public static void On_Tick()
        {
            foreach (Player p in Players.Values)
            {
                p.Do_Tick();
            }

            //TODO Back up database. Write updated values to database
        }
    }
}
