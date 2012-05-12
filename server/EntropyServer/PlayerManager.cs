using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Data;
using EntropyServer.Engine;

namespace EntropyServer
{
    static class PlayerManager
    {
        //Holds all known players
        public static Dictionary<string, Player> Players = new Dictionary<string, Player>();

        private static GameClock gameClock;

        public static void Initialize(GameClock gameClock)
        {
            gameClock.TickEvent += new GameClock.TickHandler(On_Tick);
            PlayerManager.gameClock = gameClock;

            Query query = new Query("select * from Player");

            foreach (DataRow player_row in query.result.Tables[0].Rows)
            {
                Players.Add(player_row["user_name"].ToString(), new Player(player_row["user_name"].ToString(), PlayerManager.gameClock));
            }

            MainForm.Log("PlayerManager Initialized");
        }

        public static void Create_Player(string user_name)
        {
            Players.Add(user_name, new Player(user_name, PlayerManager.gameClock));
        }

        private static void On_Tick(object sender, TickEventArgs args)
        {
            GameClock gameClock = null;
            if (sender is GameClock)
                gameClock = (GameClock)sender;

            if (gameClock != null)
            {

                //Update all players
                foreach (Player p in Players.Values)
                {
                    p.Do_Tick(gameClock);
                }

                //After all players have updated, write it to the DB
                foreach (Player p in Players.Values)
                {
                    p.Save_Database_Values();
                }
            }
            else
            {
                //todo: handle error here, something invoked the tick that was not a gameclock
            }
            //TODO Back up Database file
        }
    }
}
