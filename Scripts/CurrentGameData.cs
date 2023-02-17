using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnitInformation;

namespace GameData
{
    public class CurrentGameData: Node
    {
        private PlayerUnits PlayerTeam;
        //Load unlocked skills here as well
        //Load other unlock data too?
        //Load save position too (i.e where the save took place, etc.)

        public CurrentGameData()
        {
            
        }

        public override void _Ready()
        {
            this.GetParent().Connect("LoadPlayerDataSignal", this, "Load");
        }

        public void Load(string file_name)
        {
            var saveGame = new File();
            if (!saveGame.FileExists("res://" + file_name + ".save"))
            {
                Console.Write("res://" + file_name + ".save does not exist");
                return;
            }

            PlayerTeam = new PlayerUnits();

            saveGame.Open("res://" + file_name + ".save", File.ModeFlags.Read);

            while(saveGame.GetPosition() < saveGame.GetLen())
            {
                KeyValuePair<string, List<UnitInfo>> line = JsonConvert.DeserializeObject<KeyValuePair<string, List<UnitInfo>>>(saveGame.GetLine());
                string key = line.Key.ToString();
                if (key == "Player Team")
                {
                    PlayerTeam.Load((List<UnitInfo>) line.Value);
                }
            }

        }

        public void Save(string file_name)
        {
            var saveGame = new File();
            saveGame.Open("res://" + file_name + ".save", File.ModeFlags.Write);

            //var saveNodes = GetTree().GetNodesInGroup("Persist");
            //foreach (Node saveNode in saveNodes)
            //{
            saveGame.StoreLine(JSON.Print(PlayerTeam.Save()));
            //}

            saveGame.Close();
        }

        public PlayerUnits GetPlayerUnits()
        {
            return PlayerTeam;
        }
    }
}