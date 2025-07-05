using Godot;
using System;
using System.Threading.Tasks;

public partial class Bar : HBoxContainer
{
    private TextureProgressBar _progressBar;
    private RichTextLabel _label;
    private Tween _tween;
    private static readonly double _barSpeed = 0.75;

    private string _barText;
    
    public int Value {
        get {
            return (int) _progressBar.Value;
        }
    }

    public void Setup(string barText, int maxValue, int currentValue){
        _progressBar = GetNode<TextureProgressBar>("ProgressBar");
        _label = GetNode<RichTextLabel>("ValueBox/Text");

        _barText = barText;
        _label.Text = _barText + currentValue;
        _progressBar.MaxValue = maxValue;
        _progressBar.Value = currentValue;
    }

    public async Task ChangeValueAsync(int newValue){
        if(newValue < 0){
            newValue = 0;
        }
        else if(newValue > _progressBar.MaxValue){
            newValue = (int) _progressBar.MaxValue;
        }

        //GD.Print(_progressBar.Value + " " + newValue);
        _tween = GetTree().CreateTween();
        _tween.TweenProperty(_progressBar, "value", newValue, _barSpeed);
        _label.Text = _barText + newValue;

        await ToSignal(_tween, "finished");


    }
}
