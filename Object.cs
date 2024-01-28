using Godot;
using System;

public partial class Object : RigidBody2D
{
	[Export]
	public int Damage { get; set; } = 1;

	[Export]
	public int Laugh { get; set; } = 1;

	[Export]
	public float StunTime { get; set; } = 0f;

	[Export]
	public float LifeTime { get; set; } = 0f;

	[Export]
	public float SpawningTime { get; set; } = 0f;


	Timer SpawningTimer;
	Timer LifeTimer;
	AnimationPlayer AnimationPlayerNode { get; set; }
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
						AnimationPlayerNode.Play("Spawning");
						SpawningTimer.Start();
						CollisionShape.Disabled = true;
						break;
					}
				case StateEnum.Idle:
					{
						AnimationPlayerNode.Play("RESET");
						CollisionShape.Disabled = false;
						break;
					}
				case StateEnum.FadeOut:
					{
						AnimationPlayerNode.Play("FadeOut");
						break;
					}
			}

			_state = value;
		}
	}

	public override void _Ready()
	{
		SpawningTimer = GetNode<Timer>("SpawningTimer");
		SpawningTimer.WaitTime = SpawningTime;
		LifeTimer = GetNode<Timer>("LifeTimer");
		LifeTimer.WaitTime = LifeTime;
		AnimationPlayerNode = GetNode<AnimationPlayer>("AnimationPlayer");
		CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		State = StateEnum.Spawning;
	}

	public override void _PhysicsProcess(double delta)
	{
		var bodies = GetCollidingBodies();

		if (bodies.Count > 0)
		{
			Touched();
		}
	}

	public void _OnAnimationPlayerAnimationFinished(string animName)
	{
		if (animName == "FadeOut")
			QueueFree();
	}

	public void _OnSpawningTimerTimeout() => State = StateEnum.Idle;

	public void _OnLifeTimerTimeout() => State = StateEnum.FadeOut;

	public async void Touched()
	{
		if (State == StateEnum.Idle)
		{
			SetPhysicsProcess(false);
			ContactMonitor = false;
			Freeze = true;
			
			GetNode<AudioStreamPlayer2D>("TouchSound").Play();
			AnimationPlayerNode.Play("Touch");

			await ToSignal(AnimationPlayerNode, "animation_finished");

			AnimationPlayerNode.Play("FadeOut");
		}
	}
}
