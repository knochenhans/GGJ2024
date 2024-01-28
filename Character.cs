using Godot;
using Godot.Collections;

public partial class Character : RigidBody2D
{
	[Signal]
	public delegate void TriggerLaughEventHandler();

	[Signal]
	public delegate void LifeChangedEventHandler(int life);

	[Signal]
	public delegate void LaughChangedEventHandler(int laugh);

	[Signal]
	public delegate void DeadEventHandler();

	[Signal]
	public delegate void StunnedEventHandler();

	AudioStreamPlayer2D SoundNode { get; set; }
	AnimatedSprite2D AnimatedSprite2D { get; set; }
	AnimationPlayer AnimationPlayer { get; set; }

	PackedScene BubbleScene = ResourceLoader.Load<PackedScene>("res://Bubble.tscn");

	Timer FlipTimer { get; set; }
	Timer BubbleTimer { get; set; }
	Timer DecisionTimer { get; set; }
	Timer StunnedTimer { get; set; }

	float minForceMultiplier = 50f;
	float maxForceMultiplier = 100f;

	Bubble Bubble;

	Array<string> messages = new();

	string currentMessage;

	[Export]
	int _life = 100;

	public int Life
	{
		get => _life; set
		{
			_life = Mathf.Clamp(value, 0, 100);
			EmitSignal(SignalName.LifeChanged, _life);
		}
	}

	[Export]
	float damageFactor = 1f;

	[ExportCategory("Debug")]
	[Export]
	public bool DecisionsEnabled { get; set; } = true;

	[ExportCategory("Sounds")]
	[Export]
	public AudioStream[] OuchSounds;

	[Export]
	public AudioStream DieSound;

	[Export]
	public AudioStream StunnedSound;

	enum StateEnum
	{
		Idle,
		Walking,
		Stunned,
		Dead
	}

	enum FlipStateEnum
	{
		Left,
		Right
	}

	float currentForceMultiplier;

	bool checkCollision = false;

	AnimatedSprite2D Stars { get; set; }

	Vector2 CurrentDirection { get; set; } = new Vector2();
	private StateEnum _currentState = StateEnum.Idle;
	private StateEnum CurrentState
	{
		get => _currentState;
		set
		{

			switch (value)
			{
				case StateEnum.Idle:
					Enable(true);
					if (_currentState == StateEnum.Stunned)
					{
						ContactMonitor = true;
						Freeze = false;
						BubbleTimer.Start();
						AnimationPlayer.Play("RESET");
						Stars.Visible = false;
						Stars.Stop();
					}
					AnimatedSprite2D.Play("idle");
					break;
				case StateEnum.Walking:
					AnimatedSprite2D.Play("walk");
					break;
				case StateEnum.Dead:
					// AnimatedSprite2D.Stop();
					AnimationPlayer.Play("Fall");
					// ContactMonitor = false;
					// Freeze = true;
					// BubbleTimer.Stop();
					Life = 0;
					Enable(false);
					SoundNode.Stream = DieSound;
					SoundNode.Play();
					EmitSignal(SignalName.Dead);
					break;
				case StateEnum.Stunned:
					StunnedTimer.Start();
					// AnimatedSprite2D.Stop();
					AnimationPlayer.Play("Fall");
					// ContactMonitor = false;
					// Freeze = true;
					// BubbleTimer.Stop();
					Enable(false);
					SoundNode.Stream = StunnedSound;
					SoundNode.Play();
					Stars.Visible = true;
					Stars.Play("stunned");
					EmitSignal(SignalName.Stunned);
					break;
				default:
					AnimatedSprite2D.Play("idle");
					break;
			}
			_currentState = value;
		}
	}

	private FlipStateEnum _currentFlipState = FlipStateEnum.Right;
	private FlipStateEnum CurrentFlipState
	{
		get => _currentFlipState;
		set
		{
			switch (value)
			{
				case FlipStateEnum.Left:
					// AnimatedSprite2D.FlipH = true;
					AnimatedSprite2D.Scale = new Vector2(-1, 1);
					break;
				case FlipStateEnum.Right:
					// AnimatedSprite2D.FlipH = false;
					AnimatedSprite2D.Scale = new Vector2(1, 1);
					break;
					// default:
					// 	AnimatedSprite2D.FlipH = false;
					// 	break;
			}
			_currentFlipState = value;
		}
	}

	public override void _Ready()
	{
		AnimatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		AnimatedSprite2D.Play("idle");

		AnimationPlayer = AnimatedSprite2D.GetNode<AnimationPlayer>("AnimationPlayer");

		currentForceMultiplier = 50f;

		CurrentDirection = new Vector2(-1, 0) * currentForceMultiplier;

		Stars = GetNode<AnimatedSprite2D>("Stars");

		FlipTimer = GetNode<Timer>("FlipTimer");
		BubbleTimer = GetNode<Timer>("BubbleTimer");
		DecisionTimer = GetNode<Timer>("DecisionTimer");
		StunnedTimer = GetNode<Timer>("StunnedTimer");

		SoundNode = GetNode<AudioStreamPlayer2D>("Sound");

		currentForceMultiplier = RNG_Manager.rng.RandfRange(minForceMultiplier, maxForceMultiplier);
	}

	public void Enable(bool enable)
	{
		DecisionsEnabled = enable;

		AnimatedSprite2D.Stop();
		ContactMonitor = enable;
		Freeze = !enable;
		SetPhysicsProcess(enable);

		if (enable)
		{
			DecisionTimer.Start();
			BubbleTimer.Start();
		}
		else
		{
			DecisionTimer.Stop();
			BubbleTimer.Stop();
		}
	}

	public void Init() => EmitSignal(SignalName.LifeChanged, _life);

	public override void _PhysicsProcess(double delta)
	{
		if (CurrentState != StateEnum.Dead && CurrentState != StateEnum.Stunned)
		{
			AfterInternalCollision();

			ApplyCentralForce(CurrentDirection);

			if (LinearVelocity.Length() > 0)
			{
				CurrentState = StateEnum.Walking;

				if (LinearVelocity.X > 0)
					CurrentFlipState = FlipStateEnum.Right;
				else
					CurrentFlipState = FlipStateEnum.Left;
			}
			else
				CurrentState = StateEnum.Idle;

			var bodies = GetCollidingBodies();

			if (bodies.Count > 0)
				Collide(bodies[0]);
		}
	}

	public override void _Process(double delta)
	{
		if (Bubble != null)
			if (Bubble.IsInsideTree())
				Bubble.UpdatePosition(GlobalPosition);
			else
				Bubble = null;
	}

	private void Collide(Node2D body)
	{
		GD.Print("Collide");

		checkCollision = true;

		var damage = (int)(LinearVelocity.Length() / 10 * damageFactor);

		Life -= damage;

		if (body is Object)
		{
			var obj = body as Object;

			Life -= obj.Damage;

			if (obj.StunTime > 0 && Life > 0)
			{
				StunnedTimer.WaitTime = obj.StunTime;
				CurrentState = StateEnum.Stunned;
			}

			EmitSignal(SignalName.LaughChanged, obj.Laugh);
		}
		else if (body is TileMap)
			EmitSignal(SignalName.LaughChanged, damage);

		if (Life <= 0)
		{
			CurrentState = StateEnum.Dead;
			// LinearVelocity = new Vector2();
			// CurrentDirection = new Vector2();
			FlipTimer.Stop();
			// EmitSignal(SignalName.TriggerLaugh);
		}
	}

	private void AfterInternalCollision()
	{
		if (checkCollision)
		{
			checkCollision = false;
			CurrentDirection = LinearVelocity.Normalized() * currentForceMultiplier;

			SoundNode.Stream = OuchSounds[RNG_Manager.rng.RandiRange(0, OuchSounds.Length - 1)];
			SoundNode.Play();

			EmitSignal(SignalName.TriggerLaugh);
		}
	}

	public void _OnArea2DAreaEntered(Area2D area)
	{
		if (area is Healing)
		{
			var healing = area as Healing;

			Life += healing.Life;
			healing.Touch();
		}
	}

	public void Flip()
	{
		if (FlipTimer.IsStopped())
		{
			CurrentFlipState = CurrentFlipState == FlipStateEnum.Left ? FlipStateEnum.Right : FlipStateEnum.Left;
			FlipTimer.Start();
		}
	}

	public void _OnDecisionTimerTimeout()
	{
		if (DecisionsEnabled)
		{
			currentForceMultiplier = RNG_Manager.rng.RandfRange(minForceMultiplier, maxForceMultiplier);
			CurrentDirection = new Vector2(RNG_Manager.rng.RandfRange(-1, 1), RNG_Manager.rng.RandfRange(-1, 1)) * currentForceMultiplier;
		}
	}

	public void _OnBubbleTimerTimeout()
	{
		Bubble = BubbleScene.Instantiate<Bubble>();
		GetParent().AddChild(Bubble);
		Bubble.LifeTimer.Timeout += _OnBubbleLifeTimeout;

		Bubble.Init(MessageManager.GetRandomMessage(), GlobalPosition);
	}

	public void _OnBubbleLifeTimeout() => Bubble = null;

	public void _OnDecisionChangeTimerTimeout() => DecisionTimer.WaitTime = RNG_Manager.rng.RandfRange(1f, 5f);

	public void _OnStunnedTimerTimeout() => CurrentState = StateEnum.Idle;

	public void _OnHealingTimerTimeout() => Life += 1;
}
