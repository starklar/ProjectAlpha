using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public partial class Main : Node
{
    private PackedScene _unitScene;

    public class SaveData{  //Fill in as I go
        public List<Godot.Collections.Dictionary<string, string>> PlayerUnits { get; set; }

    }
    
    public class ChapterData{   //Fill in as I go
        public List<Godot.Collections.Dictionary<string, string>> InitialEnemyUnits { get; set; }

    }

    public SaveData SavedData;

    public override void _Ready()
    {
        base._Ready();

        _unitScene = ResourceLoader.Load<PackedScene>("res://Unit.tscn");

        SavedData = new SaveData();

        EventBus.Instance.saveGame += SaveGame;
        EventBus.Instance.loadGame += LoadGame;
        EventBus.Instance.loadChapterData += LoadChapterData;
    }

    public void SaveGame(string fileName){
        using var saveFile = FileAccess.Open("res://Saves/" + fileName +".save", FileAccess.ModeFlags.Write);

        SavedData.PlayerUnits = new List<Godot.Collections.Dictionary<string, string>>();

        Node playerUnitsNode = GetNode("GameBoard/PlayerUnits");


        foreach(Unit player in playerUnitsNode.GetChildren()){
            SavedData.PlayerUnits.Add(player.Save());
        }

        // Store the save dictionary as a new line in the save file.
        var jsonString = JsonSerializer.Serialize(SavedData);
        saveFile.StoreLine(jsonString);
    }

    public void LoadGame(string fileName){
        if (!FileAccess.FileExists("res://Saves/" + fileName +".save"))
        {
            return; // Error! We don't have a save to load.
        }

        using var saveFile = FileAccess.Open("res://Saves/" + fileName +".save", FileAccess.ModeFlags.Read);

        Node playerUnitsNode = GetNode("GameBoard/PlayerUnits");

        //Probably use this later down the line once more savable things are set up
        //var saveNodes = GetTree().GetNodesInGroup("Persist");
        /*foreach (Node playerNode in playerUnitsNode.GetChildren())
        {
            playerNode.QueueFree();
        }*/

        while (saveFile.GetPosition() < saveFile.GetLength())
        {
            var jsonString = saveFile.GetLine();

            var json = new Json();
            var parseResult = json.Parse(jsonString);
            if (parseResult != Error.Ok)
            {
                GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
                continue;
            }
            
            SaveData data = JsonSerializer.Deserialize<SaveData>(jsonString);

            foreach (Node playerNode in playerUnitsNode.GetChildren())
            {
                playerNode.Free();
            }

            foreach(Godot.Collections.Dictionary<string, string> unitData in data.PlayerUnits){
                Unit playerUnit = _unitScene.Instantiate<Unit>();

                playerUnitsNode.AddChild(playerUnit);

                playerUnit.Load(unitData);
            }
        }
    }

    public void LoadChapterData(string chapterName){
        if (!FileAccess.FileExists("res://ChapterData/" + chapterName +".data"))
        {
            return; // Error! We don't have a save to load.
        }

        using var chapterFile = FileAccess.Open("res://ChapterData/" + chapterName +".data", FileAccess.ModeFlags.Read);

        Node enemyUnitsNode = GetNode("GameBoard/EnemyUnits");

        while (chapterFile.GetPosition() < chapterFile.GetLength())
        {
            var jsonString = chapterFile.GetLine();

            var json = new Json();
            var parseResult = json.Parse(jsonString);
            if (parseResult != Error.Ok)
            {
                GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
                continue;
            }
            
            ChapterData data = JsonSerializer.Deserialize<ChapterData>(jsonString);

            foreach (Node enemyNode in enemyUnitsNode.GetChildren())
            {
                enemyNode.Free();
            }

            foreach(Godot.Collections.Dictionary<string, string> unitData in data.InitialEnemyUnits){
                Unit enemyUnit= _unitScene.Instantiate<Unit>();

                enemyUnitsNode.AddChild(enemyUnit);

                enemyUnit.Load(unitData);
            }
        }
    }
}
