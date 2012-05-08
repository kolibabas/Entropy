using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Data;


namespace EntropyServer
{
    public enum CMD_ID
    {
        CMD_CREATE,
        CMD_AUTH,
        CMD_DISCONNECT,
        CMD_REFER,
        CMD_NOTIFY_RQST,
        CMD_NOTIFY_RXD,
        CMD_ACTION,
        CMD_ACTION_CANCEL,
        CMD_MAX_CMD_ID,
    };

    public enum PLAYER_ACTION
    {
        ACTION_NO_ACTION,

        ACTION_ATTACK_COMBAT,
        ACTION_ATTACK_DIPLOMACY,
        ACTION_ATTACK_TECH,
        ACTION_ATTACK_ECON,

        ACTION_BUILD_SHIP,

        ACTION_BUILD_STRUCTURE,
        ACTION_SELL_STRUCTURE,

        ACTION_RESEARCH,

        ACTION_DIPLOMACY,

        ACTION_DIRECT,

        //Notification Actions
        ACTION_INCOMING_DETECTED,
    };

    public enum ACTION_RESULT
    {
        RESULT_SUCCESS,
        RESULT_FAILURE,
    };

    public enum PLAYER_STRAT
    {
        STRAT_PIRATE,
        STRAT_TECH,
        STRAT_ECON,
        STRAT_DIPLOMACY,
    };

    public enum PLAYER_NOTIFY
    {
        NOTIFY_ATTACK_COMPLETE,
        NOTIFY_CONSTRUCTION_COMPLETE,
        NOTIFY_RESEARCH_COMPLETE,
        NOTIFY_DIPLOMACY_COMPLETE,
        NOTIFY_ATTACKED,
        NOTIFY_SABOTAGED,
        NOTIFY_CAUGHT_SABOTEUR,
    };

    public enum STRUCTURE_TYPE
    {
        STRUCT_DEFENSE_T1,
        STRUCT_ECON_T1,
        STRUCT_TECH_T1,
        STRUCT_DIPLOMACY_T1,

        STRUCT_DEFENSE_T2,
        STRUCT_ECON_T2,
        STRUCT_TECH_T2,
        STRUCT_DIPLOMACY_T2,

        STRUCT_DEFENSE_T3,
        STRUCT_ECON_T3,
        STRUCT_TECH_T3,
        STRUCT_DIPLOMACY_T3,

        STRUCT_LAST_STRUCT,
    };

    public enum SHIP_TYPE
    {
        SHIP_PIRATE_FRIGATE,
        SHIP_PIRATE_CRUISER,
        SHIP_PIRATE_BATTLESHIP,
        SHIP_PIRATE_COVERT_OPS,
        SHIP_PIRATE_SCIENCE,
        SHIP_PIRATE_ENGINEERING,

        SHIP_TECH_FRIGATE,
        SHIP_TECH_CRUISER,
        SHIP_TECH_BATTLESHIP,
        SHIP_TECH_COVERT_OPS,
        SHIP_TECH_SCIENCE,
        SHIP_TECH_ENGINEERING,

        SHIP_ECON_FRIGATE,
        SHIP_ECON_CRUISER,
        SHIP_ECON_BATTLESHIP,
        SHIP_ECON_COVERT_OPS,
        SHIP_ECON_SCIENCE,
        SHIP_ECON_ENGINEERING,

        SHIP_DIPLOMACY_FRIGATE,
        SHIP_DIPLOMACY_CRUISER,
        SHIP_DIPLOMACY_BATTLESHIP,
        SHIP_DIPLOMACY_COVERT_OPS,
        SHIP_DIPLOMACY_SCIENCE,
        SHIP_DIPLOMACY_ENGINEERING,

        SHIP_LAST_SHIP,
    };

    public enum TECH_TYPE
    {
        TECH_SHIP_WEAPONS,
        TECH_SHIP_HP,
        TECH_SHIP_ENGINE,
        TECH_SHIP_STEALTH,
        TECH_STRUCT_WEAPONS,
        TECH_STRUCT_HP,
        TECH_STRUCT_ECON,
        TECH_STRUCT_RESEARCH,
        TECH_STRUCT_DIPLOMACY,
        TECH_CONSTRUCT_SPEED,
        TECH_BASE_SENSORS,
        TECH_BASE_FLEET_CAP,
        TECH_BASE_ASTEROID_CAP
    };

    public enum ASTEROID_SIZE
    {
        ASTEROID_SMALL,
        ASTEROID_MEDIUM,
        ASTEROID_LARGE,

        ASTEROID_LAST_ASTEROID,
    };

    static class GameManager
    {
        public static bool Game_Initialized = false;

        public static Dictionary<SHIP_TYPE, Ship> Ship_Templates = new Dictionary<SHIP_TYPE, Ship>();

        public static Dictionary<STRUCTURE_TYPE, Structure> Structure_Templates = new Dictionary<STRUCTURE_TYPE, Structure>();

        public static Dictionary<ASTEROID_SIZE, Asteroid> Asteroid_Templates = new Dictionary<ASTEROID_SIZE, Asteroid>();

        public static void Initialize()
        {
            //Build Ship Templates
            SqlCeDataAdapter adapter = new SqlCeDataAdapter("select * from Ship", ConnectionManager.SQLConnection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                Ship temp_ship = new Ship();
                temp_ship.type = (SHIP_TYPE)row["type"];
                temp_ship.name = row["name"].ToString();
                temp_ship.Defense_Mode = (int)row["combat_rating"] == 1 ? true : false;
                temp_ship.Upkeep = (int)row["upkeep"];
                temp_ship.Cost = (int)row["cost"];
                temp_ship.Build_Time = (int)row["build_time"];
                temp_ship.Speed = (int)row["speed"];
                temp_ship.HP = (int)row["hp"];

                temp_ship.Combat_Rating = (int)row["combat_rating"];
                temp_ship.Bonus_Tech = (int)row["bonus_tech"];
                temp_ship.Bonus_Econ = (int)row["bonus_econ"];
                temp_ship.Bonus_Diplomacy = (int)row["bonus_diplomacy"];

                Ship_Templates.Add((SHIP_TYPE)row["type"], temp_ship);
            }

            //Build Structure Templates
            adapter = new SqlCeDataAdapter("select * from Structure", ConnectionManager.SQLConnection);
            ds = new DataSet();
            adapter.Fill(ds);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                Structure temp_structure = new Structure();

                temp_structure.Type = (STRUCTURE_TYPE)row["type"];
                temp_structure.name = row["name"].ToString();
                temp_structure.Upkeep = (int)row["upkeep"];
                temp_structure.HP = (int)row["hp"];
                temp_structure.Cost = (int)row["cost"];
                temp_structure.Build_Time = (int)row["build_time"];

                temp_structure.Bonus_Combat = (int)row["bonus_combat"];
                temp_structure.Bonus_Tech = (int)row["bonus_tech"];
                temp_structure.Bonus_Econ = (int)row["bonus_econ"];
                temp_structure.Bonus_Diplomacy = (int)row["bonus_diplomacy"];

                Structure_Templates.Add((STRUCTURE_TYPE)row["type"], temp_structure);
            }

            Asteroid_Templates.Add(ASTEROID_SIZE.ASTEROID_SMALL, new Asteroid(ASTEROID_SIZE.ASTEROID_SMALL));
            Asteroid_Templates.Add(ASTEROID_SIZE.ASTEROID_MEDIUM, new Asteroid(ASTEROID_SIZE.ASTEROID_MEDIUM));
            Asteroid_Templates.Add(ASTEROID_SIZE.ASTEROID_LARGE, new Asteroid(ASTEROID_SIZE.ASTEROID_LARGE));

            Game_Initialized = true;
            MainForm.Log("GameManager Initialized");
        }
    }
}
