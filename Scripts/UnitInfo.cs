using Godot;
using System;
using Skirmish;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace UnitInformation
{
    public class UnitInfo
    {
        public string UnitName;

        public int MovementType;

        public int CurrentLevel;
        public int CurrentEXP;

        //Skills are skill ID numbers
        //Each array should be a max of 4
        public int[] BattleSkills;
        public int[] PassiveSkills;

        /*
        0 - Physical
        1 - Pierce
        2 - Fire
        3 - Water
        4 - Earth
        5 - Wind
        6 - Aether
        */
        public int[] ElementRes;
        /*
        0 - Physical
        1 - Pierce
        2 - Fire
        3 - Water
        4 - Earth
        5 - Wind
        6 - Aether
        7 - Heal
        8 - Support
        */
        public int[] ElementAff;
        
        /*
        0 - Max HP: Maximum HP
        1 - Max MP: Maximum MP
        2 - Movement: Number of spaces unit can move on the map at once
        3 - Strength: Increase physical attack damage and critical hit percent
        4 - Focus: Increase magic attack damage and hit percent
        5 - Speed: Must be >= 4 to follow up attack and increase evasion percent
        6 - Vitality: Reduce physical damage taken and increase critical dodge percent
        7 - Spirit: Reduce magic damage taken and increase MP regen rate
        */
        public int[] Stats;
    }

    public class PlayerUnits: Godot.Object
    {
        private List<UnitInfo> PlayerTeam = new List<UnitInfo>();

        public PlayerUnits()
        {
            //For testing purposes only
            /*int player_team_size = 2;

            for(int i = 0; i < player_team_size; i++)
            {
                UnitInfo playerUnit = new UnitInfo();

                playerUnit.UnitName = "Flynn";
                int[] statsList = { 15, 15, 5, 5, 8, 6, 4, 3 };
                int[] resList = { 1, 1, 1, 1, 1, 1, 1 };
                int[] affList = { 1, 0, 4, 4, 4, 4, 2, 3, 1 };
                BattleSkill atk = new BattleSkill("Attack", "1 hit Physical Attack", 0, 0, 2, 1, 1, 1, 80, 0, (1, 1), false, null);
                int[] bSkillList = {0, 1};
                int[] pSkillList = {};

                playerUnit.Stats = statsList;
                playerUnit.ElementRes = resList;
                playerUnit.ElementAff = affList;

                playerUnit.BattleSkills = bSkillList;
                playerUnit.PassiveSkills = pSkillList;

                PlayerTeam.Add(playerUnit);
            }

            SaveParty("Save1");*/
        }

        public int GetPartySize()
        {
            return PlayerTeam.Count;
        }

        public UnitInfo GetUnitInfo(int index)
        {
            return PlayerTeam[index];
        }

        public List<UnitInfo> GetParty()
        {
            return PlayerTeam;
        }

        public void UpdateUnit(UnitInfo new_unit, int index)
        {
            PlayerTeam[index] = new_unit;
        }

        public void UpdateParty(List<UnitInfo> new_party)
        {
            PlayerTeam = new List<UnitInfo>(new_party);
        }

        public KeyValuePair<string, List<UnitInfo>> Save()
        {   
            return new KeyValuePair<string, List<UnitInfo>> ("Player Team", PlayerTeam);
        }

        public void Load(List<UnitInfo> loading_team)
        {
            PlayerTeam = new List<UnitInfo>();

            foreach(UnitInfo unit in loading_team)
            {
                UnitInfo newUnit = new UnitInfo();

                newUnit.UnitName = unit.UnitName;
                newUnit.MovementType = unit.MovementType;
                newUnit.CurrentLevel = unit.CurrentLevel;
                newUnit.CurrentEXP = unit.CurrentEXP;
                newUnit.Stats = unit.Stats;
                newUnit.ElementRes = unit.ElementRes;
                newUnit.ElementAff = unit.ElementAff;
                newUnit.BattleSkills = unit.BattleSkills;
                newUnit.PassiveSkills = unit.PassiveSkills;
                
                PlayerTeam.Add(newUnit);
            }
        }

        //TESTING FOR SAVE
        public void SaveParty(string save_name)
        {
            var saveGame = new File();
            saveGame.Open("res://" + save_name + ".save", File.ModeFlags.Write);

            Console.Write(this.Save());

            saveGame.StoreLine(JsonConvert.SerializeObject(this.Save()));

            saveGame.Close();
        }
    }
}