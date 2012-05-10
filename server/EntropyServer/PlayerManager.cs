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

            Query query = new Query("select * from Player");

            foreach (DataRow player_row in query.result.Tables[0].Rows)
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
            //Update all players
            foreach (Player p in Players.Values)
            {
                p.Do_Tick();
            }

            //After all players have updated, write it to the DB
            foreach (Player p in Players.Values)
            {
                p.Save_Database_Values();
            }

            //TODO Back up Database file
        }
    }
}
