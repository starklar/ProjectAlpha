using Godot;
using System.Collections.Generic;
using UnitInformation;
using Main;

namespace Skirmish
{
    public class AIUnitScene: UnitScene
    {
        private static string MapAIPattern;
        private static string BattleAIPattern;

        public void Create(int start_x, int start_y, string new_name, int team, int movement_type, int level, int exp, int[] stats, BattleSkill standard_attack, ActiveSkill[] active_skills, PassiveSkill[] passive_skills, string[] elemental_res, int[] elemental_aff, string map_ai_pattern, string battle_ai_pattern)
        {
            MapAIPattern = map_ai_pattern;
            BattleAIPattern = battle_ai_pattern;

            CurrX = start_x;
            CurrY = start_y;
            PrevX = start_x;
            PrevY = start_y;
            Position = new Vector2(CurrX * Global.MAP_SCALE + Global.MAP_SCALE / 2, CurrY * Global.MAP_SCALE + Global.MAP_SCALE / 2);

            Show();

            UnitName = new_name;
            Team = team;
            MovementType = movement_type;

            Level = level;
            EXP = exp;

            Stats = new List<int>(stats);
            int[] statModsList = { 0, 0, 0, 0, 0, 0 };
            StatMods = new List<int>(statModsList);
            StandardAttack = standard_attack;
            ActiveSkills = new List<ActiveSkill>(active_skills);
            PassiveSkills = new List<PassiveSkill>(passive_skills);
            ElementAff = new List<int>(elemental_aff);
            ElementRes = new List<string>(elemental_res);

            CurrHP = Stats[0];
            CurrMP = Stats[1];

            AniPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            AniPlayer.Play("Idle");

            HasMoved = false;
        }

        public string GetMapAIPattern()
        {
            return MapAIPattern;
        }

        public string GetBattleAIPattern()
        {
            return BattleAIPattern;
        }
    }
}