using Godot;
using System;

public partial class LaughFace : Node2D
{
	Timer Timer { get; set; }

	public override void _Ready()
	{
		Timer = GetNode<Timer>("Timer");
		RandomizeTimer();
		Timer.Start();
	}

	public void _OnTimerTimeout()
	{
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("blink");
		RandomizeTimer();
	}

	public void RandomizeTimer()
	{
		Timer.WaitTime = RNG_Manager.rng.RandfRange(1f, 5f);
	}
}
