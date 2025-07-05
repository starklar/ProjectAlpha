using Godot;
using System;

public partial class UnitMenuButton : TextureButton
{
	private RichTextLabel _label;
	private string text;

	public override void _Ready()
	{
		_label = GetNode<RichTextLabel>("VBoxContainer/Text");

		Pressed += OnUnitMenuButtonPressed;
	}

	public void SetText(string newText){
		_label.Text = "[center]" + newText;
		text = newText;
	}

	private void OnUnitMenuButtonPressed(){
        EventBus.Instance.EmitSignal("unitMenuButtonPressed", text);
    }
}
