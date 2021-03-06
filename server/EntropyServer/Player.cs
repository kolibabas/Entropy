﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Data;
using EntropyServer.Engine;

namespace EntropyServer
{
    class Player
    {
        #region Default Starting Player

        private const int       STARTING_CREDITS = 100;
        private const string    STARTING_ASTEROIDS = "000000000000";
        private const string    STARTING_STRUCTURES = "000000000000000000000000000000000000000000000000000000000000";
        private const string    STARTING_SHIPS = "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
        private const string    STARTING_ACTION = "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
        private const int       STARTING_TECH = 0;

        public static object[] Default_Player(int id, string user_name, PLAYER_STRAT strategy)
        {
            return new object[]
            { 
            id,                     //ID
            user_name,              //user_name
            (int)strategy,          //strategy
            STARTING_CREDITS,       //credits_current
            STARTING_ASTEROIDS,     //asteroids
            STARTING_STRUCTURES,    //structures
            STARTING_ACTION,        //action_attack
            STARTING_ACTION,        //action_build_ship
            STARTING_ACTION,        //action_build_struct
            STARTING_ACTION,        //action_research
            STARTING_ACTION,        //action_diplomacy
            STARTING_ACTION,        //action_direct
            STARTING_TECH,          //tech_ship_weapons;
            STARTING_TECH,          //tech_ship_hp;
            STARTING_TECH,          //tech_ship_engine;
            STARTING_TECH,          //tech_ship_stealth;
            STARTING_TECH,          //tech_struct_weapons;
            STARTING_TECH,          //tech_struct_hp;
            STARTING_TECH,          //tech_struct_mining;
            STARTING_TECH,          //tech_struct_research;
            STARTING_TECH,          //tech_construct_speed;
            STARTING_TECH,          //tech_base_sensors;
            STARTING_TECH,          //tech_base_fleet_cap;
            STARTING_TECH,          //tech_base_asteroid_cap;
            STARTING_SHIPS,         //ships_attacking
            STARTING_SHIPS,         //ships_defending
            };
        }

        #endregion
        
        #region Properties

        //Schema values
        private int ID; //C# and SQL Max is 2147483647
        private string user_name; //SQL 100 chars
        private PLAYER_STRAT strategy; //SQL int

        private int credits_current; //C# and SQL Max is 2147483647

        private Dictionary<ASTEROID_SIZE, int> Asteroids = new Dictionary<ASTEROID_SIZE, int>(); //SQL Max is (#asteroid types) * 9999 = 4 digits per asteroid type

        private Dictionary<STRUCTURE_TYPE, int> Structures = new Dictionary<STRUCTURE_TYPE, int>(); //SQL Max is (#struct types) * 99999 (5 digits) = 5 digits per struct type

        //Actions SQL Type (2 digits), Start tick (7 digits), End tick (7 digits), Player Name (up to 100 chars) = 116 digits per action
        private PlayerAction action_attack;
        private PlayerAction action_build_ship;
        private PlayerAction action_build_struct;
        private PlayerAction action_research;
        private PlayerAction action_diplomacy;
        private PlayerAction action_direct;

        //Techs tier 0-5, 10, 100, 1000, 10000, 100000 RP
        private int tech_ship_weapons;
        private int tech_ship_hp;
        private int tech_ship_engine;
        private int tech_ship_stealth;
        private int tech_struct_weapons;
        private int tech_struct_hp;
        private int tech_struct_econ;
        private int tech_struct_research;
        private int tech_struct_diplomacy;
        private int tech_construct_speed;
        private int tech_base_sensors;
        private int tech_base_fleet_cap;
        private int tech_base_asteroid_cap;

        private Dictionary<SHIP_TYPE, int> Ships_Attacking = new Dictionary<SHIP_TYPE, int>(); //SQL Max is (#ship types) * 99999 (5 digits) = 5 digits per ship type

        private Dictionary<SHIP_TYPE, int> Ships_Defending = new Dictionary<SHIP_TYPE, int>(); //SQL Max is (#ship types, 2 digit) * 999999 (6 digits) = 8 digits per ship type

        //Computed / Static values
        private int tier; //TODO Compute Tier

        private const int base_econ = 1;
        private const int base_research = 1;
        private const int base_combat = 1;
        private const int base_diplomacy = 1;
        private const int base_build_slots = 10;

        private int Score_Overall;
        private int Score_Combat;
        private int Score_Diplomacy;
        private int Score_Econ;
        private int Score_Tech;

        private int credits_per_tick_net;
        private int credits_per_tick_gross;
        private int research_per_tick;

        private int ship_pop_used;
        private int ship_pop_cap;

        private int build_slots_cap;
        private int build_slots_used;
        private int asteroids_used;
        private int asteroids_cap;

        private List<Notification> Unread_Notifications; //TODO Get notifications from database
        public bool fleet_at_home;

#endregion

        #region Local Constants

        private const int SELL_DIVISOR = 2;
        private const int TRADE_DIVISOR = 10;
        private const int DIRECT_DIVISOR = 50;
        private const int PIRATE_DIVISOR = 2;
        private const int STRUCT_DESTROYED_CHANCE = 20;
        private const int STRUCT_DESTROYED_CHANCE_TARGET = 33;

        #endregion

        #region Constructor
        public Player(string user_name, GameClock gameClock)
        {
            this.user_name = user_name;

            //Populate all player data

            Load_Database_Values();
            Compute_Credits(gameClock);
            Compute_Research(gameClock);
            Compute_Ship_Pop(gameClock);
            Compute_Scores(gameClock);
        }

        private void Load_Database_Values()
        {
            Query query = new Query("select * from Player where user_name=\'" + this.user_name + "\'");

            if (query.result.Tables[0].Rows.Count != 0)
            {
                this.ID = (int)query.result.Tables[0].Rows[0]["id"];
                this.strategy = (PLAYER_STRAT)(int)query.result.Tables[0].Rows[0]["strategy"];
                this.credits_current = (int)query.result.Tables[0].Rows[0]["credits_current"];
                this.Asteroids = Parse_Asteroids(query.result.Tables[0].Rows[0]["asteroids"].ToString());
                this.Structures = Parse_Structures(query.result.Tables[0].Rows[0]["structures"].ToString());
                this.Ships_Attacking = Parse_Ships(query.result.Tables[0].Rows[0]["ships_attacking"].ToString());
                this.Ships_Defending = Parse_Ships(query.result.Tables[0].Rows[0]["ships_defending"].ToString());

                this.action_attack = Parse_Action(query.result.Tables[0].Rows[0]["action_attack"].ToString());
                this.action_build_ship = Parse_Action(query.result.Tables[0].Rows[0]["action_build_ship"].ToString());
                this.action_build_struct = Parse_Action(query.result.Tables[0].Rows[0]["action_build_struct"].ToString());
                this.action_research = Parse_Action(query.result.Tables[0].Rows[0]["action_research"].ToString());
                this.action_diplomacy = Parse_Action(query.result.Tables[0].Rows[0]["action_diplomacy"].ToString());
                this.action_direct = Parse_Action(query.result.Tables[0].Rows[0]["action_direct"].ToString());

                this.tech_ship_weapons = (int)query.result.Tables[0].Rows[0]["tech_ship_weapons"];
                this.tech_ship_hp = (int)query.result.Tables[0].Rows[0]["tech_ship_hp"];
                this.tech_ship_engine = (int)query.result.Tables[0].Rows[0]["tech_ship_engine"];
                this.tech_ship_stealth = (int)query.result.Tables[0].Rows[0]["tech_ship_stealth"];
                this.tech_struct_weapons = (int)query.result.Tables[0].Rows[0]["tech_struct_weapons"];
                this.tech_struct_hp = (int)query.result.Tables[0].Rows[0]["tech_struct_hp"];
                this.tech_struct_econ = (int)query.result.Tables[0].Rows[0]["tech_struct_mining"];
                this.tech_struct_research = (int)query.result.Tables[0].Rows[0]["tech_struct_research"];
                this.tech_struct_diplomacy = (int)query.result.Tables[0].Rows[0]["tech_struct_diplomacy"];
                this.tech_construct_speed = (int)query.result.Tables[0].Rows[0]["tech_construct_speed"];
                this.tech_base_sensors = (int)query.result.Tables[0].Rows[0]["tech_base_sensors"];
                this.tech_base_fleet_cap = (int)query.result.Tables[0].Rows[0]["tech_base_fleet_cap"];
                this.tech_base_asteroid_cap = (int)query.result.Tables[0].Rows[0]["tech_base_asteroid_cap"];

                if (this.action_attack.Type == PLAYER_ACTION.ACTION_NO_ACTION)
                {
                    fleet_at_home = true;
                }
                else
                {
                    fleet_at_home = false;
                }
            }
            else
            {
                MainForm.Log("Error! A player got past authentication without existing! Username: " + this.user_name);
            }
        }

        private Dictionary<ASTEROID_SIZE, int> Parse_Asteroids(string text)
        {
            Dictionary<ASTEROID_SIZE, int> temp_list = new Dictionary<ASTEROID_SIZE, int>();

            //4 digits per asteroid type
            int small_asteroid_count = int.Parse(text.Substring(0, 4));
            int med_asteroid_count = int.Parse(text.Substring(4, 4));
            int large_asteroid_count = int.Parse(text.Substring(8, 4));

            temp_list.Add(ASTEROID_SIZE.ASTEROID_SMALL, small_asteroid_count);
            temp_list.Add(ASTEROID_SIZE.ASTEROID_MEDIUM, med_asteroid_count);
            temp_list.Add(ASTEROID_SIZE.ASTEROID_LARGE, large_asteroid_count);

            return temp_list;
        }

        private Dictionary<STRUCTURE_TYPE, int> Parse_Structures(string text)
        {
            Dictionary<STRUCTURE_TYPE, int> temp_list = new Dictionary<STRUCTURE_TYPE, int>();

            for (int structure_type = 0; structure_type < (int)STRUCTURE_TYPE.STRUCT_LAST_STRUCT; structure_type++)
            {
                //5 digits per structure type
                int count = int.Parse(text.Substring(structure_type * 5, 5));

                temp_list.Add((STRUCTURE_TYPE)structure_type, count);
            }

            return temp_list;
        }

        private Dictionary<SHIP_TYPE, int> Parse_Ships(string text)
        {
            Dictionary<SHIP_TYPE, int> temp_list = new Dictionary<SHIP_TYPE, int>();

            for (int ship_type = 0; ship_type < (int)SHIP_TYPE.SHIP_LAST_SHIP; ship_type++)
            {
                //5 digits per ship type
                int count = int.Parse(text.Substring(ship_type * 5, 5));

                temp_list.Add((SHIP_TYPE)ship_type, count);
            }

            return temp_list;
        }

        private PlayerAction Parse_Action(string text)
        {
            PlayerAction temp_action = new PlayerAction();

            //Actions SQL Type (2 digits), Start tick (7 digits), End tick (7 digits), Param (2 digits), Player Name (up to 100 chars) = 118 digits per action
            temp_action.Type = (PLAYER_ACTION)int.Parse(text.Substring(0, 2));
            temp_action.Start_Tick = int.Parse(text.Substring(2, 7));
            temp_action.End_Tick = int.Parse(text.Substring(9, 7));
            temp_action.Param = int.Parse(text.Substring(16, 2));
            temp_action.Target_Player_Name = text.Substring(18, 100).Trim();

            return temp_action;
        }

        private void Compute_Credits(GameClock gameClock)
        {
            credits_per_tick_gross = base_econ;

            //Structure Income
            for (int structure_type = 0; structure_type < (int)STRUCTURE_TYPE.STRUCT_LAST_STRUCT; structure_type++)
            {
                credits_per_tick_gross += Structures[(STRUCTURE_TYPE)structure_type] * (GameManager.Structure_Templates[(STRUCTURE_TYPE)structure_type].Bonus_Econ * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_ECON, tech_struct_econ));
            }

            //Ship Income
            for (int ship_type = 0; ship_type < (int)SHIP_TYPE.SHIP_LAST_SHIP; ship_type++)
            {
                credits_per_tick_gross += Ships_Attacking[(SHIP_TYPE)ship_type] * (GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Bonus_Econ);
            }

            credits_per_tick_net = credits_per_tick_gross;

            //Structure Upkeep
            for (int structure_type = 0; structure_type < (int)STRUCTURE_TYPE.STRUCT_LAST_STRUCT; structure_type++)
            {
                credits_per_tick_net -= Structures[(STRUCTURE_TYPE)structure_type] * (GameManager.Structure_Templates[(STRUCTURE_TYPE)structure_type].Upkeep);
            }

            //Ship Upkeep
            for (int ship_type = 0; ship_type < (int)SHIP_TYPE.SHIP_LAST_SHIP; ship_type++)
            {
                credits_per_tick_net -= Ships_Attacking[(SHIP_TYPE)ship_type] * (GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Upkeep);
            }
        }

        private void Compute_Research(GameClock gameClock)
        {
            research_per_tick = base_research;

            //Structure Research
            for (int structure_type = 0; structure_type < (int)STRUCTURE_TYPE.STRUCT_LAST_STRUCT; structure_type++)
            {
                research_per_tick += Structures[(STRUCTURE_TYPE)structure_type] * (GameManager.Structure_Templates[(STRUCTURE_TYPE)structure_type].Bonus_Tech * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_RESEARCH, tech_struct_research));
            }

            //Ship Research
            for (int ship_type = 0; ship_type < (int)SHIP_TYPE.SHIP_LAST_SHIP; ship_type++)
            {
                research_per_tick += Ships_Attacking[(SHIP_TYPE)ship_type] * (GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Bonus_Tech);
            }
        }

        private void Compute_Ship_Pop(GameClock gameClock)
        {
            ship_pop_cap = TechManager.Tech_Mod(TECH_TYPE.TECH_BASE_FLEET_CAP, tech_base_fleet_cap);

            //Ship Count
            for (int ship_type = 0; ship_type < (int)SHIP_TYPE.SHIP_LAST_SHIP; ship_type++)
            {
                ship_pop_used += Ships_Attacking[(SHIP_TYPE)ship_type];
                ship_pop_used += Ships_Defending[(SHIP_TYPE)ship_type];
            }

            if (ship_pop_used > ship_pop_cap)
            {
                MainForm.Log("Player " + user_name + " has " + ship_pop_used + " ships when their cap is " + ship_pop_cap);
            }
        }

        private void Compute_Build_Slots(GameClock gameClock)
        {
            build_slots_cap = base_build_slots;
            asteroids_cap = TechManager.Tech_Mod(TECH_TYPE.TECH_BASE_ASTEROID_CAP, tech_base_asteroid_cap);

            //Asteroid slots
            for (int asteroid_type = 0; asteroid_type < (int)SHIP_TYPE.SHIP_LAST_SHIP; asteroid_type++)
            {
                build_slots_cap += Asteroids[(ASTEROID_SIZE)asteroid_type] * GameManager.Asteroid_Templates[(ASTEROID_SIZE)asteroid_type].Slots;
                asteroids_used += Asteroids[(ASTEROID_SIZE)asteroid_type];
            }

            //Structures used
            for (int structure_type = 0; structure_type < (int)STRUCTURE_TYPE.STRUCT_LAST_STRUCT; structure_type++)
            {
                build_slots_used += Structures[(STRUCTURE_TYPE)structure_type];
            }

            if (build_slots_used > build_slots_cap)
            {
                MainForm.Log("Player " + user_name + " has " + build_slots_used + " structures when their cap is " + build_slots_cap);
            }

            if (asteroids_used > asteroids_cap)
            {
                MainForm.Log("Player " + user_name + " has " + asteroids_used + " asteroids when their cap is " + asteroids_cap);
            }
            
        }

        public void Compute_Scores(GameClock gameClock)
        {
            Score_Overall = 0;
            Score_Combat = 0;
            Score_Diplomacy = 0;
            Score_Econ = 0;
            Score_Tech = 0;

            //Ship Scores
            for (int ship_type = 0; ship_type < (int)SHIP_TYPE.SHIP_LAST_SHIP; ship_type++)
            {
                Score_Combat += Ships_Attacking[(SHIP_TYPE)ship_type] * ((GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Combat_Rating) * TechManager.Tech_Mod(TECH_TYPE.TECH_SHIP_WEAPONS, tech_ship_weapons));
                Score_Combat += Ships_Defending[(SHIP_TYPE)ship_type] * ((GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Combat_Rating) * TechManager.Tech_Mod(TECH_TYPE.TECH_SHIP_WEAPONS, tech_ship_weapons));

                Score_Diplomacy += Ships_Attacking[(SHIP_TYPE)ship_type] * (GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Bonus_Diplomacy);
                Score_Diplomacy += Ships_Defending[(SHIP_TYPE)ship_type] * (GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Bonus_Diplomacy);

                Score_Econ += Ships_Attacking[(SHIP_TYPE)ship_type] * (GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Bonus_Econ);
                Score_Econ += Ships_Defending[(SHIP_TYPE)ship_type] * (GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Bonus_Econ);

                Score_Tech += Ships_Attacking[(SHIP_TYPE)ship_type] * (GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Bonus_Tech);
                Score_Tech += Ships_Defending[(SHIP_TYPE)ship_type] * (GameManager.Ship_Templates[(SHIP_TYPE)ship_type].Bonus_Tech);
            }

            //Structure Scres
            for (int structure_type = 0; structure_type < (int)STRUCTURE_TYPE.STRUCT_LAST_STRUCT; structure_type++)
            {
                Score_Combat += Structures[(STRUCTURE_TYPE)structure_type] * ((GameManager.Structure_Templates[(STRUCTURE_TYPE)structure_type].Bonus_Combat) * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_WEAPONS, tech_struct_weapons));

                Score_Diplomacy += Structures[(STRUCTURE_TYPE)structure_type] * ((GameManager.Structure_Templates[(STRUCTURE_TYPE)structure_type].Bonus_Diplomacy) * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_DIPLOMACY, tech_struct_diplomacy));

                Score_Econ += Structures[(STRUCTURE_TYPE)structure_type] * ((GameManager.Structure_Templates[(STRUCTURE_TYPE)structure_type].Bonus_Econ) * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_ECON, tech_struct_econ));

                Score_Tech += Structures[(STRUCTURE_TYPE)structure_type] * ((GameManager.Structure_Templates[(STRUCTURE_TYPE)structure_type].Bonus_Tech) * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_RESEARCH, tech_struct_research));
            }

            Score_Overall = Score_Combat + Score_Diplomacy + Score_Econ + Score_Tech;
        }

        #endregion

        #region Serializer

        public void Save_Database_Values()
        {
            Query query = new Query("select * from Player where user_name=\'" + this.user_name + "\'");

            if (query.result.Tables[0].Rows.Count != 0)
            {
                query.result.Tables[0].Rows[0]["credits_current"] = this.credits_current;

                query.result.Tables[0].Rows[0]["asteroids"] = Serialize_Asteroids(this.Asteroids);

                query.result.Tables[0].Rows[0]["structures"] = Serialize_Structures(this.Structures);

                //Store Ships
                query.result.Tables[0].Rows[0]["ships_attacking"] = Serialize_Ships(this.Ships_Attacking);
                query.result.Tables[0].Rows[0]["ships_defending"] = Serialize_Ships(this.Ships_Defending);

                //Store Actions
                query.result.Tables[0].Rows[0]["action_attack"] = Serialize_Action(this.action_attack);
                query.result.Tables[0].Rows[0]["action_build_ship"] = Serialize_Action(this.action_build_ship);
                query.result.Tables[0].Rows[0]["action_build_struct"] = Serialize_Action(this.action_build_struct);
                query.result.Tables[0].Rows[0]["action_research"] = Serialize_Action(this.action_research);
                query.result.Tables[0].Rows[0]["action_diplomacy"] = Serialize_Action(this.action_diplomacy);
                query.result.Tables[0].Rows[0]["action_direct"] = Serialize_Action(this.action_direct);

                //Store Tech Levels
                query.result.Tables[0].Rows[0]["tech_ship_weapons"] = this.tech_ship_weapons;
                query.result.Tables[0].Rows[0]["tech_ship_hp"] = this.tech_ship_hp;
                query.result.Tables[0].Rows[0]["tech_ship_engine"] = this.tech_ship_engine;
                query.result.Tables[0].Rows[0]["tech_ship_stealth"] = this.tech_ship_stealth;
                query.result.Tables[0].Rows[0]["tech_struct_weapons"] = this.tech_struct_weapons;
                query.result.Tables[0].Rows[0]["tech_struct_hp"] = this.tech_struct_hp;
                query.result.Tables[0].Rows[0]["tech_struct_mining"] = this.tech_struct_econ;
                query.result.Tables[0].Rows[0]["tech_struct_research"] = this.tech_struct_research;
                query.result.Tables[0].Rows[0]["tech_struct_diplomacy"] = this.tech_struct_diplomacy;
                query.result.Tables[0].Rows[0]["tech_construct_speed"] = this.tech_construct_speed;
                query.result.Tables[0].Rows[0]["tech_base_sensors"] = this.tech_base_sensors;
                query.result.Tables[0].Rows[0]["tech_base_fleet_cap"] = this.tech_base_fleet_cap;
                query.result.Tables[0].Rows[0]["tech_base_asteroid_cap"] = this.tech_base_asteroid_cap;

                query.Update();
            }
        }

        private string Serialize_Ships(Dictionary<SHIP_TYPE, int> ships)
        {
            string result = "";

            foreach (SHIP_TYPE ship_type in GameManager.Ship_Templates.Keys)
            {
                //5 digits per ship type
                result += ships[ship_type].ToString().PadLeft(5, '0');
            }

            return result;
        }

        private string Serialize_Structures(Dictionary<STRUCTURE_TYPE, int> structures)
        {
            string result = "";

            foreach (STRUCTURE_TYPE structure_type in GameManager.Structure_Templates.Keys)
            {
                //5 digits per structure type
                result += structures[structure_type].ToString().PadLeft(5, '0');
            }

            return result;
        }

        private string Serialize_Asteroids(Dictionary<ASTEROID_SIZE, int> asteroids)
        {
            string result = "";

            //4 digits per asteroid type
            result += asteroids[ASTEROID_SIZE.ASTEROID_SMALL].ToString().PadLeft(4, '0');
            result += asteroids[ASTEROID_SIZE.ASTEROID_MEDIUM].ToString().PadLeft(4, '0');
            result += asteroids[ASTEROID_SIZE.ASTEROID_LARGE].ToString().PadLeft(4, '0');

            return result;
        }

        private string Serialize_Action(PlayerAction action)
        {
            string result = "";

            //Actions SQL Type (2 digits), Start tick (7 digits), End tick (7 digits), Param (2 digits), Player Name (up to 100 chars) = 118 digits per action
            result += ((int)action.Type).ToString().PadLeft(2, '0');
            result += ((int)action.Start_Tick).ToString().PadLeft(7, '0');
            result += ((int)action.End_Tick).ToString().PadLeft(7, '0');
            result += ((int)action.Param).ToString().PadLeft(2, '0');
            result += action.Target_Player_Name.PadLeft(100, '0');

            return result;
        }

#endregion

        #region Tick Processing

        public void Do_Tick(GameClock gameClock)
        {
            Process_Actions(gameClock);
            Compute_Credits(gameClock);
            Compute_Research(gameClock);
            Compute_Ship_Pop(gameClock);
            
            credits_current += credits_per_tick_net;

            Compute_Scores(gameClock);
        }

        //TODO Notifications for completed actions
        private void Process_Actions(GameClock gameClock)
        {
            //Attack Slot
            if (gameClock.CurrentTick == action_attack.End_Tick)
            {
                Do_Combat(action_attack);
            }

            //Ship Slot
            if (gameClock.CurrentTick == action_build_ship.End_Tick)
            {
                SHIP_TYPE ship_type = (SHIP_TYPE)action_build_ship.Param;

                this.Ships_Defending[ship_type]++;
            }

            //Structure Slot
            if (gameClock.CurrentTick == action_build_struct.End_Tick)
            {
                if (this.action_build_struct.Type == PLAYER_ACTION.ACTION_BUILD_STRUCTURE)
                {
                    STRUCTURE_TYPE structure_type = (STRUCTURE_TYPE)action_build_ship.Param;

                    this.Structures[structure_type]++;
                }
                else if (this.action_build_struct.Type == PLAYER_ACTION.ACTION_SELL_STRUCTURE)
                {
                    STRUCTURE_TYPE structure_type = (STRUCTURE_TYPE)action_build_ship.Param;

                    this.Structures[structure_type]--;

                    //Refund some cash on completed sell
                    credits_current += GameManager.Structure_Templates[structure_type].Cost / SELL_DIVISOR;
                }

            }

            //Research Slot
            if (gameClock.CurrentTick == action_research.End_Tick)
            {
                TECH_TYPE tech_type = (TECH_TYPE)action_research.Param;
                //TODO Cap at max tier
                switch (tech_type)
                {
                    case TECH_TYPE.TECH_SHIP_WEAPONS:
                        tech_ship_weapons++;
                        break;

                    case TECH_TYPE.TECH_SHIP_HP:
                        tech_ship_hp++;
                        break;

                    case TECH_TYPE.TECH_SHIP_ENGINE:
                        tech_ship_engine++;
                        break;

                    case TECH_TYPE.TECH_SHIP_STEALTH:
                        tech_ship_stealth++;
                        break;

                    case TECH_TYPE.TECH_STRUCT_WEAPONS:
                        tech_struct_weapons++;
                        break;

                    case TECH_TYPE.TECH_STRUCT_HP:
                        tech_struct_hp++;
                        break;

                    case TECH_TYPE.TECH_STRUCT_ECON:
                        tech_struct_econ++;
                        break;

                    case TECH_TYPE.TECH_STRUCT_RESEARCH:
                        tech_struct_research++;
                        break;

                    case TECH_TYPE.TECH_CONSTRUCT_SPEED:
                        tech_construct_speed++;
                        break;

                    case TECH_TYPE.TECH_BASE_SENSORS:
                        tech_base_sensors++;
                        break;

                    case TECH_TYPE.TECH_BASE_FLEET_CAP:
                        tech_base_fleet_cap++;
                        break;

                    case TECH_TYPE.TECH_BASE_ASTEROID_CAP:
                        tech_base_asteroid_cap++;
                        break;

                    default:
                        MainForm.Log("Error! Player " + user_name + " researched an impossible tech: " + tech_type);
                        break;
                }
            }

            //Diplomacy Slot
            if (gameClock.CurrentTick == action_diplomacy.End_Tick)
            {
                //Diplomacy on self = "Optimize Planetary Trade"
                if (action_diplomacy.Target_Player_Name == user_name)
                {
                    credits_current += Score_Diplomacy / TRADE_DIVISOR;
                }
                else
                {
                    //TODO Do Sabotage!
                }
            }

            //Direct Slot
            if (gameClock.CurrentTick == action_direct.End_Tick)
            {
                credits_current += Score_Overall / DIRECT_DIVISOR;
            }
        }

        #endregion

        #region Combat

        private void Do_Combat(PlayerAction action)
        {

            //Get target player
            Player target_player = PlayerManager.Players[action.Target_Player_Name];

            //Compute both player's relevant powers
            int attacker_power = Combat_Compute_Attacker_Power(action);
            int defender_power = Combat_Compute_Defender_Power(action, target_player);

            //Compute power difference and victor
            float differential = attacker_power - defender_power;
            bool attacker_victory = differential > 0 ? true : false; //GAVIN'S TERNARY STATEMENT

            //Apply damage to attacker
            Combat_Compute_Attacker_losses(differential, attacker_power, attacker_victory, target_player);

            //Apply damage to defender
            Combat_Compute_Defender_losses(differential, defender_power, attacker_victory, target_player, action);

            //Shift credits
            if (attacker_victory)
            {
                Combat_Compute_Credit_Transfer(differential, target_player);
            }

        }

        private int Combat_Compute_Attacker_Power(PlayerAction action)
        {
            //Compute total attacking power
            int attacker_power = 0;
            foreach (KeyValuePair<SHIP_TYPE, int> pair in this.Ships_Attacking)
            {
                attacker_power += GameManager.Ship_Templates[pair.Key].Combat_Rating * pair.Value * TechManager.Tech_Mod(TECH_TYPE.TECH_SHIP_WEAPONS, this.tech_ship_weapons);
            }

            //Determine which attacking and defending specialty ships (Covert Ops, et al) to add to the scores
            //Using the same mechanism as above based on attack type
            switch (action.Type)
            {
                case PLAYER_ACTION.ACTION_ATTACK_COMBAT:
                    break;
                case PLAYER_ACTION.ACTION_ATTACK_DIPLOMACY:
                    foreach (KeyValuePair<SHIP_TYPE, int> pair in this.Ships_Attacking)
                    {
                        attacker_power += GameManager.Ship_Templates[pair.Key].Bonus_Diplomacy * pair.Value;
                    }
                    break;
                case PLAYER_ACTION.ACTION_ATTACK_ECON:
                    foreach (KeyValuePair<SHIP_TYPE, int> pair in this.Ships_Attacking)
                    {
                        attacker_power += GameManager.Ship_Templates[pair.Key].Bonus_Econ * pair.Value;
                    }
                    break;
                case PLAYER_ACTION.ACTION_ATTACK_TECH:
                    foreach (KeyValuePair<SHIP_TYPE, int> pair in this.Ships_Attacking)
                    {
                        attacker_power += GameManager.Ship_Templates[pair.Key].Bonus_Tech * pair.Value;
                    }
                    break;
                default:
                    break;
            }

            return attacker_power;
        }

        private int Combat_Compute_Defender_Power(PlayerAction action, Player target_player)
        {
            //Compute Defender combat power
            //Add raw power from ships defending
            int defender_power = 0;
            foreach (KeyValuePair<SHIP_TYPE, int> pair in target_player.Ships_Defending)
            {
                defender_power += GameManager.Ship_Templates[pair.Key].Combat_Rating * pair.Value * TechManager.Tech_Mod(TECH_TYPE.TECH_SHIP_WEAPONS, target_player.tech_ship_weapons);
            }

            //If the defending fleet is at home, it adds its raw combat power to the defense
            if (target_player.fleet_at_home)
            {
                foreach (KeyValuePair<SHIP_TYPE, int> pair in target_player.Ships_Attacking)
                {
                    defender_power += GameManager.Ship_Templates[pair.Key].Combat_Rating * pair.Value * TechManager.Tech_Mod(TECH_TYPE.TECH_SHIP_WEAPONS, target_player.tech_ship_weapons);
                }
            }

            //Add powers from all structures with combat power
            foreach (KeyValuePair<STRUCTURE_TYPE, int> pair in target_player.Structures)
            {
                defender_power += GameManager.Structure_Templates[pair.Key].Bonus_Combat * pair.Value * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_WEAPONS, target_player.tech_struct_weapons);
            }

            //Determine which attacking and defending specialty ships (Covert Ops, et al) to add to the scores
            //Using the same mechanism as above based on attack type
            switch (action.Type)
            {
                case PLAYER_ACTION.ACTION_ATTACK_COMBAT:
                    break;
                case PLAYER_ACTION.ACTION_ATTACK_DIPLOMACY:
                    foreach (KeyValuePair<SHIP_TYPE, int> pair in target_player.Ships_Defending)
                    {
                        defender_power += GameManager.Ship_Templates[pair.Key].Bonus_Diplomacy * pair.Value;
                    }

                    if (target_player.fleet_at_home)
                    {
                        foreach (KeyValuePair<SHIP_TYPE, int> pair in target_player.Ships_Attacking)
                        {
                            defender_power += GameManager.Ship_Templates[pair.Key].Bonus_Diplomacy * pair.Value;
                        }
                    }
                    break;
                case PLAYER_ACTION.ACTION_ATTACK_ECON:
                    foreach (KeyValuePair<SHIP_TYPE, int> pair in target_player.Ships_Defending)
                    {
                        defender_power += GameManager.Ship_Templates[pair.Key].Bonus_Econ * pair.Value;
                    }

                    if (target_player.fleet_at_home)
                    {
                        foreach (KeyValuePair<SHIP_TYPE, int> pair in target_player.Ships_Attacking)
                        {
                            defender_power += GameManager.Ship_Templates[pair.Key].Bonus_Econ * pair.Value;
                        }
                    }
                    break;
                case PLAYER_ACTION.ACTION_ATTACK_TECH:
                    foreach (KeyValuePair<SHIP_TYPE, int> pair in target_player.Ships_Defending)
                    {
                        defender_power += GameManager.Ship_Templates[pair.Key].Bonus_Tech * pair.Value;
                    }

                    if (target_player.fleet_at_home)
                    {
                        foreach (KeyValuePair<SHIP_TYPE, int> pair in target_player.Ships_Attacking)
                        {
                            defender_power += GameManager.Ship_Templates[pair.Key].Bonus_Tech * pair.Value;
                        }
                    }
                    break;
                default:
                    break;
            }

            return defender_power;
        }

        private void Combat_Compute_Attacker_losses(float differential, int attacker_power, bool attacker_victory, Player target_player)
        {
            //Attacker can only lose HP from ships that attacked
            //If they won, limit losses to 20% of differential
            int attacker_hp_loss = 0;
            int total_attack_hp = 0;
            foreach (KeyValuePair<SHIP_TYPE, int> pair in this.Ships_Attacking)
            {
                total_attack_hp += GameManager.Ship_Templates[pair.Key].HP * pair.Value * TechManager.Tech_Mod(TECH_TYPE.TECH_SHIP_HP, target_player.tech_ship_hp); ;
            }
            float attacker_loss_mod = attacker_victory ? 0.2f : 0.8f;
            attacker_hp_loss = (int)((differential / (float)attacker_power) * total_attack_hp * attacker_loss_mod);

            //Select destroyed ships and structures
            Random rand = new Random();
            int offense_combat_rolls = 0;
            int max_offense_combat_rolls = attacker_hp_loss;
            while (attacker_hp_loss > 0)
            {
                SHIP_TYPE destroyed_ship = (SHIP_TYPE)rand.Next((int)SHIP_TYPE.SHIP_LAST_SHIP);

                //If there is an attacking ship of this type, destroy it, reduce HP loss by its HP
                if (this.Ships_Attacking[destroyed_ship] > 0)
                {
                    this.Ships_Attacking[destroyed_ship]--;
                    attacker_hp_loss -= GameManager.Ship_Templates[destroyed_ship].HP * TechManager.Tech_Mod(TECH_TYPE.TECH_SHIP_HP, this.tech_ship_hp);
                }

                //Terminate combat eventually if not losing anything
                offense_combat_rolls++;
                if (offense_combat_rolls > max_offense_combat_rolls)
                {
                    break;
                }
            }
        }

        private void Combat_Compute_Defender_losses(float differential, int defender_power, bool attacker_victory, Player target_player, PlayerAction action)
        {

            //Defender can lose any ship present and any structure that was targeted
            int defender_hp_loss = 0;
            int total_defend_hp = 0;
            foreach (KeyValuePair<SHIP_TYPE, int> pair in this.Ships_Defending)
            {
                total_defend_hp += GameManager.Ship_Templates[pair.Key].HP * pair.Value;
            }

            if (target_player.fleet_at_home)
            {
                foreach (KeyValuePair<SHIP_TYPE, int> pair in target_player.Ships_Attacking)
                {
                    total_defend_hp += GameManager.Ship_Templates[pair.Key].Bonus_Tech * pair.Value;
                }
            }

            //Add only structures that have a stat that was targeted by the attack action. ACTION_ATTACK_COMBAT targets all.
            foreach (KeyValuePair<STRUCTURE_TYPE, int> pair in target_player.Structures)
            {
                bool add = false;
                if (action.Type == PLAYER_ACTION.ACTION_ATTACK_COMBAT)
                {
                    add = true;
                }
                else if (action.Type == PLAYER_ACTION.ACTION_ATTACK_DIPLOMACY && GameManager.Structure_Templates[pair.Key].Bonus_Diplomacy > 0)
                {
                    add = true;
                }
                else if (action.Type == PLAYER_ACTION.ACTION_ATTACK_ECON && GameManager.Structure_Templates[pair.Key].Bonus_Econ > 0)
                {
                    add = true;
                }
                else if (action.Type == PLAYER_ACTION.ACTION_ATTACK_TECH && GameManager.Structure_Templates[pair.Key].Bonus_Tech > 0)
                {
                    add = true;
                }

                if (add)
                {
                    total_defend_hp += GameManager.Structure_Templates[pair.Key].HP * pair.Value * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_HP, target_player.tech_struct_hp);
                }
            }

            //If defender won, limit losses to 20%
            float defender_loss_mod = attacker_victory ? 0.8f : 0.2f;
            defender_hp_loss = (int)((differential / (float)defender_power) * total_defend_hp * defender_loss_mod);

            //Defender losses, decide between ships and structures
            int defense_combat_rolls = 0;
            int max_defense_combat_rolls = defender_hp_loss;
            while (defender_hp_loss > 0)
            {
                Random rand = new Random();
                int target_roll = rand.Next(100);
                if (target_roll < STRUCT_DESTROYED_CHANCE && action.Type == PLAYER_ACTION.ACTION_ATTACK_COMBAT)
                {
                    STRUCTURE_TYPE destroyed_struct = (STRUCTURE_TYPE)rand.Next((int)STRUCTURE_TYPE.STRUCT_LAST_STRUCT);

                    //If the selected structure type exists, destroy it
                    if (target_player.Structures[destroyed_struct] > 0)
                    {
                        target_player.Structures[destroyed_struct]--;
                        defender_hp_loss -= GameManager.Structure_Templates[destroyed_struct].HP * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_HP, target_player.tech_struct_hp);
                    }
                }
                else if (target_roll < STRUCT_DESTROYED_CHANCE_TARGET && action.Type != PLAYER_ACTION.ACTION_ATTACK_COMBAT)
                {
                    List<STRUCTURE_TYPE> target_types = new List<STRUCTURE_TYPE>();
                    switch (action.Type)
                    {
                        case PLAYER_ACTION.ACTION_ATTACK_DIPLOMACY:
                            foreach (KeyValuePair<STRUCTURE_TYPE, Structure> struct_type in GameManager.Structure_Templates)
                            {
                                if (struct_type.Value.Bonus_Diplomacy > 0)
                                {
                                    target_types.Add(struct_type.Key);
                                }
                            }
                            break;
                        case PLAYER_ACTION.ACTION_ATTACK_ECON:
                            foreach (KeyValuePair<STRUCTURE_TYPE, Structure> struct_type in GameManager.Structure_Templates)
                            {
                                if (struct_type.Value.Bonus_Econ > 0)
                                {
                                    target_types.Add(struct_type.Key);
                                }
                            }
                            break;
                        case PLAYER_ACTION.ACTION_ATTACK_TECH:
                            foreach (KeyValuePair<STRUCTURE_TYPE, Structure> struct_type in GameManager.Structure_Templates)
                            {
                                if (struct_type.Value.Bonus_Tech > 0)
                                {
                                    target_types.Add(struct_type.Key);
                                }
                            }
                            break;
                    }

                    //If the target has one of the valid structures, destroy it
                    //If not, the loop continues to find a ship
                    foreach (STRUCTURE_TYPE destroyed_struct in target_types)
                    {
                        if (target_player.Structures[destroyed_struct] > 0)
                        {
                            target_player.Structures[destroyed_struct]--;
                            defender_hp_loss -= GameManager.Structure_Templates[destroyed_struct].HP * TechManager.Tech_Mod(TECH_TYPE.TECH_STRUCT_HP, target_player.tech_struct_hp);
                            break;
                        }
                    }

                }
                else
                {
                    SHIP_TYPE destroyed_ship = (SHIP_TYPE)rand.Next((int)SHIP_TYPE.SHIP_LAST_SHIP);

                    //If there is an defending ship of this type, destroy it, reduce HP loss by its HP
                    if (target_player.Ships_Defending[destroyed_ship] > 0)
                    {
                        target_player.Ships_Defending[destroyed_ship]--;
                        defender_hp_loss -= GameManager.Ship_Templates[destroyed_ship].HP * TechManager.Tech_Mod(TECH_TYPE.TECH_SHIP_HP, target_player.tech_ship_hp);
                    }
                    //Else if target player has a ship marked for attack that is at home, destroy it
                    else if (target_player.Ships_Attacking[destroyed_ship] > 0 && target_player.fleet_at_home)
                    {
                        target_player.Ships_Defending[destroyed_ship]--;
                        defender_hp_loss -= GameManager.Ship_Templates[destroyed_ship].HP * TechManager.Tech_Mod(TECH_TYPE.TECH_SHIP_HP, target_player.tech_ship_hp);
                    }
                }

                //Terminate combat eventually if not losing anything
                defense_combat_rolls++;
                if (defense_combat_rolls > max_defense_combat_rolls)
                {
                    break;
                }
            }
        }

        private void Combat_Compute_Credit_Transfer(float differential, Player target_player)
        {
            //Compute Credit Transfer
            if (target_player.credits_current > differential)
            {
                target_player.credits_current -= (int)differential;

                if (this.strategy == PLAYER_STRAT.STRAT_PIRATE)
                {
                    this.credits_current += (int)differential / PIRATE_DIVISOR;
                }
                else
                {
                    this.credits_current += (int)differential;
                }
            }
            //Else the target was massively outclassed, no plundering the weak
        }

        #endregion


    }
}
