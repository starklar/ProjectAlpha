using Godot;
using System;

public partial class CombatUnit : Node2D
{
    //public int MaxHealth { get; set; }
    
    //Might want to make other animations
    public void Attack(){

    }

    public void TakeDamage(int count)
    {
        /*
        if (_state == States.Dead) {
            return;
        }

        _health -= count;
        if (_health <= 0) {
            _health = 0;
            _state = States.Dead;
            EmitSignal("Died");
        }

        var animationPlayer = (AnimationPlayer) GetNode("AnimationPlayer");
        animationPlayer.Play("take_hit");

        EmitSignal("HealthChanged", _health);
        */
    }

    public void OnAnimationPlayerAnimationFinished(string name)
    {
        /*
        if (_state != States.Dead || name != "take_hit") {
            return;
        }

        var animationPlayer = (AnimationPlayer) GetNode("AnimationPlayer");
        animationPlayer.Play("die");
        */

        /*
        if (name == "attack") {
            _state = States.Idle;
        }

        if (name == "anticipate") {
            var animationPlayer = (AnimationPlayer) GetNode("AnimationPlayer");
            animationPlayer.Play("attack");
            DamageTarget(_target, Strength);
        }
        */
    }

    public void OnTimerTimeout()
    {
        /*
        if (_target == null) {
            var timer = (Timer) GetNode("Timer");
            timer.Stop();
            return;
        }

        if (_state != States.Idle) {
            return;
        }

        _state = States.Attacking;

        var animationPlayer = (AnimationPlayer) GetNode("AnimationPlayer");
        animationPlayer.Play("anticipate");
        */
    }

    public void OnPlayerDied()
    {
    }
}