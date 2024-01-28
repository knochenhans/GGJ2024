using Godot;
using System;

public partial class LaughFace : Node2D
{
	Timer Timer { get; set; }

	AnimatedSprite2D AnimatedSprite2D { get; set; }

	public override void _Ready()
	{
		Timer = GetNode<Timer>("Timer");
		RandomizeTimer();
		Timer.Start();
		AnimatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public void _OnTimerTimeout()
	{
		AnimatedSprite2D.Play("blink");
		RandomizeTimer();
	}

	public void RandomizeTimer()
	{
		Timer.WaitTime = RNG_Manager.rng.RandfRange(1f, 5f);
	}

	internal void GameOver()
	{
		Timer.Stop();
		AnimatedSprite2D.Play("game_over");
	}
}
