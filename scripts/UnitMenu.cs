using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class UnitMenu : MarginContainer
{
    private PackedScene _menuButton;

	private VBoxContainer _vbox;

	public override void _Ready()
	{
		_vbox = GetNode<VBoxContainer>("VBoxContainer");

		_menuButton = ResourceLoader.Load<PackedScene>("res://UnitMenuButton.tscn");

		Visible = false;

		EventBus.Instance.enterUnitMenu += Setup;
		EventBus.Instance.exitUnitMenu += ClearButtons;

		SetProcessInput(false);
	}


	public override void _Process(double delta)
	{
	}

	private void Setup(Array<string> possibleCommands){
		UnitMenuButton previousButton = null;
		UnitMenuButton currentButton = null;
		string previousPath = null;
		string currentPath = null;

		//Special Command
		if(possibleCommands.Contains("Special")){
			UnitMenuButton specialButton = _menuButton.Instantiate<UnitMenuButton>();
			specialButton.SetText("SPECIAL"); //Placeholder for now
			_vbox.AddChild(specialButton);
		}

		//Attack
		if(possibleCommands.Contains("Attack")){
			UnitMenuButton attackButton = _menuButton.Instantiate<UnitMenuButton>();
			_vbox.AddChild(attackButton);
			attackButton.SetText("Attack");
		}

		//Support
		if(possibleCommands.Contains("Support")){
			UnitMenuButton supportButton = _menuButton.Instantiate<UnitMenuButton>();
			_vbox.AddChild(supportButton);
			supportButton.SetText("Support");
		}

		//Set, NOTE: Should be allowed to select this anywhere
		UnitMenuButton setButton = _menuButton.Instantiate<UnitMenuButton>();
		_vbox.AddChild(setButton);
		setButton.SetText("Set");

		//Wait
		if(possibleCommands.Contains("Wait")){
			UnitMenuButton waitButton = _menuButton.Instantiate<UnitMenuButton>();
			_vbox.AddChild(waitButton);
			waitButton.SetText("Wait");
		}

		for(int x = 1; x < _vbox.GetChildren().Count; x++){
			currentButton = _vbox.GetChild<UnitMenuButton>(x);
			currentPath = currentButton.GetPath();
			previousButton = _vbox.GetChild<UnitMenuButton>(x - 1);
			previousPath = previousButton.GetPath();

			previousButton.FocusNeighborBottom = currentPath;
			currentButton.FocusNeighborTop = previousPath;
		}

		if(previousButton != null){
			currentButton.FocusNeighborBottom = _vbox.GetChild<UnitMenuButton>(0).GetPath();
			_vbox.GetChild<UnitMenuButton>(0).FocusNeighborTop = currentPath;
		}

		_vbox.GetChild<UnitMenuButton>(0).GrabFocus();

		Visible = true;

		SetProcessInput(true);
	}

	private void ClearButtons(){
		foreach(Node child in _vbox.GetChildren()){
			_vbox.RemoveChild(child);
			child.QueueFree();
		}
	}
}
