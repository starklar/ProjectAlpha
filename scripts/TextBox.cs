using Godot;
using System;

public partial class TextBox : MarginContainer
{
	private RichTextLabel _label;

	public override void _Ready()
	{
		_label = GetNode<RichTextLabel>("VBoxContainer/Text");
	}

	public override void _Process(double delta)
	{
	}

	public void SetText(string newText){
		_label.Text = "[center]" + newText;
	}
}
