using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitleScreen
{
    public class TitleScreenScene: Node
    {
        [Signal]
        delegate void LoadPlayerDataSignal(string file_name);

        [Signal]
        delegate void StartSkirmishSignal(string map_name, int cursor_start_x, int cursor_start_y, int map_size_x, int map_size_y);


        public TitleScreenScene()
        {

        }

        public override void _Ready()
        {
            //GetNode("PlayerData").Connect("LoadPlayerDataSignal", this, "Load");
            //LoadGame("Save1");
        }

        public void LoadGame(string file_name)
        {
            this.SetProcessInput(false);

            EmitSignal("LoadPlayerDataSignal", file_name);

            Node PlayerData = (Node) GetNode("PlayerData");
            

            PlayerData.GetParent().RemoveChild(PlayerData);

            Node nextNode = ((PackedScene)ResourceLoader.Load("res://BattleScenes/Ch1SkirmishScene.tscn")).Instance();

            nextNode.AddChild(PlayerData);

            GetTree().Root.AddChild(nextNode);

            Connect("StartSkirmishSignal", nextNode, "StartSkirmish");
            EmitSignal("StartSkirmishSignal", "Ch1Skirmish", 0, 0, 22, 13);

            GetTree().Root.RemoveChild(this);
            this.QueueFree();
        }

        public override void _Input(InputEvent inputEvent)
        {
            LoadGame("Save1");
        }
    }
}