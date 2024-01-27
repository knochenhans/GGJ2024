using Godot;
using System;

public partial class Object : RigidBody2D
{
	[Export]
	public int Damage { get; set; } = 1;

	[Export]
	public int Laugh { get; set; } = 1;

	Timer SpawningTimer;
	AnimationPlayer AnimationPlayer { get; set; }
	CollisionShape2D CollisionShape { get; set; }

	enum StateEnum
	{
		Spawning,
		Idle,
		FadeOut
	}

	StateEnum _state;
	private StateEnum State
	{
		get => _state;
		set
		{
			switch (value)
			{
				case StateEnum.Spawning:
					{
						AnimationPlayer.Play("Spawning");
						SpawningTimer.Start();
						CollisionShape.Disabled = true;
						break;
					}
				case StateEnum.Idle:
					{
						AnimationPlayer.Stop();
						CollisionShape.Disabled = false;
						break;
					}
				case StateEnum.FadeOut:
					{
						AnimationPlayer.Play("FadeOut");
						break;
					}
			}

			_state = value;
		}
	}

	public override void _Ready()
	{
		SpawningTimer = GetNode<Timer>("SpawningTimer");
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		State = StateEnum.Spawning;
	}

	public override void _PhysicsProcess(double delta)
	{
		var bodies = GetCollidingBodies();

		if (bodies.Count > 0)
		{
			var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			animationPlayer.Play("FadeOut");
		}
	}

	public void _OnAnimationPlayerAnimationFinished(string animName)
	{
		if (animName == "FadeOut")
		{
			QueueFree();
		}
	}

	public void _OnSpawningTimerTimeout()
	{
		State = StateEnum.Idle;
	}

	public void _OnLifeTimerTimeout()
	{
		State = StateEnum.FadeOut;
	}
}
