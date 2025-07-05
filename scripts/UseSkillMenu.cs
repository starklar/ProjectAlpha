using Godot;
using Godot.Collections;
using System;

public partial class UseSkillMenu : MarginContainer
{
    private PackedScene _menuButton;
    private GameBoard _gameBoard;

	private HBoxContainer _hbox;
    private VBoxContainer _skillsbox;

	public override void _Ready()
	{
        _gameBoard = GetParent() as GameBoard;
		_hbox = GetNode<HBoxContainer>("HBoxContainer");
        _skillsbox = GetNode<VBoxContainer>("HBoxContainer/SkillsBox");

		_menuButton = ResourceLoader.Load<PackedScene>("res://UseSkillMenuButton.tscn");

		Visible = false;

		EventBus.Instance.enterUseSkillMenu += Setup;
		EventBus.Instance.exitUseSkillMenu += ClearButtons;

		SetProcessInput(false);
	}


	public override void _Process(double delta)
	{
	}

	private void Setup(int basicAttackSkillIndex, Array<int> skillIndexes){
        UseSkillMenuButton previousButton = null;
		UseSkillMenuButton currentButton = null;
		string previousPath;
		string currentPath = null;
		bool isAttack = false;

		if(basicAttackSkillIndex >= 0){
			ActiveSkill skill = GlobalVariables.Instance.ActiveSkillList[basicAttackSkillIndex];
			UseSkillMenuButton button = _menuButton.Instantiate<UseSkillMenuButton>();
			_skillsbox.AddChild(button);
			button.SetText(skill.Name, -1);
			
			isAttack = true;
		}

		int i = 0;

        foreach(int id in skillIndexes){
			ActiveSkill skill = GlobalVariables.Instance.ActiveSkillList[id];
			GD.Print(skill.Name);
            if((isAttack && skill.Type < 7) || (!isAttack && skill.Type >= 7)){
                UseSkillMenuButton button = _menuButton.Instantiate<UseSkillMenuButton>();
				_skillsbox.AddChild(button);
                button.SetText(skill.Name, i);
            }
			i++;
        }

        for(int x = 1; x < _skillsbox.GetChildren().Count; x++){
			currentButton = _skillsbox.GetChild<UseSkillMenuButton>(x);
			currentPath = currentButton.GetPath();
			previousButton = _skillsbox.GetChild<UseSkillMenuButton>(x - 1);
			previousPath = previousButton.GetPath();

			previousButton.FocusNeighborBottom = currentPath;
			currentButton.FocusNeighborTop = previousPath;
		}

		if(previousButton != null){
			currentButton.FocusNeighborBottom = _skillsbox.GetChild<UseSkillMenuButton>(0).GetPath();
			_skillsbox.GetChild<UseSkillMenuButton>(0).FocusNeighborTop = currentPath;
		}

		_skillsbox.GetChild<UseSkillMenuButton>(0).GrabFocus();

		Visible = true;

		SetProcessInput(true);
	}

	private void ClearButtons(){
        Visible = false;
		SetProcessInput(false);

		foreach(Node child in _skillsbox.GetChildren()){
			_skillsbox.RemoveChild(child);
			child.QueueFree();
		}
	}
}
