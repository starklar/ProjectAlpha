using Godot;
using System;

public partial class UseSkillMenuButton : TextureButton
{
    private RichTextLabel _label;
	private int _id;

	public override void _Ready()
	{
		_label = GetNode<RichTextLabel>("VBoxContainer/Text");

		Pressed += OnUseSkillMenuButtonPressed;
	}

	public void SetText(string newText, int id){
		_label.Text = "[center]" + newText;
		_id = id;
	}

	private void OnUseSkillMenuButtonPressed(){
        EventBus.Instance.EmitSignal("useSkillMenuButtonPressed", _id);
    }
}
