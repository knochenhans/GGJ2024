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

	[Export]
	public AudioStream[] OuchSounds;

	[Export]
	public AudioStream DieSound;

	AudioStreamPlayer2D SoundNode { get; set; }
	AnimatedSprite2D AnimatedSprite2D { get; set; }
	AnimationPlayer AnimationPlayer { get; set; }

	PackedScene BubbleScene = ResourceLoader.Load<PackedScene>("res://Bubble.tscn");

	Timer FlipTimer { get; set; }
	Timer BubbleTimer { get; set; }

	float minForceMultiplier = 50f;
	float maxForceMultiplier = 100f;

	Bubble Bubble;

	Array<string> messages = new();

	string currentMessage;

	[Export]
	int _life = 100;

	[Export]
	float damageFactor = 1f;

	public int Life
	{
		get => _life; set
		{
			_life = value;
			EmitSignal(SignalName.LifeChanged, _life);
		}
	}

	enum StateEnum
	{
		Idle,
		Walking,
		LyingDown,
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
					AnimatedSprite2D.Play("idle");
					break;
				case StateEnum.Walking:
					AnimatedSprite2D.Play("walk");
					break;
				case StateEnum.Dead:
					AnimatedSprite2D.Stop();
					AnimationPlayer.Play("Fall");
					ContactMonitor = false;
					Freeze = true;
					BubbleTimer.Stop();
					Life = 0;
					SoundNode.Stream = DieSound;
					SoundNode.Play();
					break;
				case StateEnum.LyingDown:
					AnimatedSprite2D.Play("lie");
					break;
				case StateEnum.Stunned:
					AnimatedSprite2D.Play("stun");
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

		FlipTimer = GetNode<Timer>("FlipTimer");
		BubbleTimer = GetNode<Timer>("BubbleTimer");

		SoundNode = GetNode<AudioStreamPlayer2D>("Sound");

		currentForceMultiplier = RNG_Manager.rng.RandfRange(minForceMultiplier, maxForceMultiplier);

		LoadMessages();
	}

	public void Init()
	{
		EmitSignal(SignalName.LifeChanged, _life);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (CurrentState != StateEnum.Dead)
		{
			AfterCollision();

			base._PhysicsProcess(delta);
			ApplyCentralForce(CurrentDirection);

			if (LinearVelocity.Length() > 0)
			{
				CurrentState = StateEnum.Walking;

				if (LinearVelocity.X > 0)
				{
					CurrentFlipState = FlipStateEnum.Right;
				}
				else
				{
					CurrentFlipState = FlipStateEnum.Left;
				}
			}
			else
			{
				CurrentState = StateEnum.Idle;
			}

			var bodies = GetCollidingBodies();

			if (bodies.Count > 0)
			{
				Collide(bodies[0]);
			}
		}
	}

	private void Collide(Node2D body)
	{
		checkCollision = true;

		Life -= (int)(LinearVelocity.Length() / 10 * damageFactor);

		if(body is Object)
		{
			var obj = body as Object;

			Life -= obj.Damage;
			EmitSignal(SignalName.LaughChanged, obj.Laugh);
		}

		if (Life <= 0)
		{
			CurrentState = StateEnum.Dead;
			// LinearVelocity = new Vector2();
			// CurrentDirection = new Vector2();
			FlipTimer.Stop();
			// EmitSignal(SignalName.TriggerLaugh);
		}
		// else
		// {
		// 	CurrentState = StateEnum.Stunned;
		// 	LinearVelocity = new Vector2();
		// 	CurrentDirection = new Vector2();
		// 	FlipTimer.Stop();
		// }
	}

	public override void _Process(double delta)
	{
		if (Bubble != null)
			if (Bubble.IsInsideTree())
				Bubble.UpdatePosition(GlobalPosition);
			else
				Bubble = null;
	}

	private void AfterCollision()
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

	public void _OnArea2DBodyShapeEntered(Rid body_rid, Node2D body, int body_shape_index, int local_shape_index)
	{
		// GD.Print(body_rid);
		// GD.Print(body);
		// GD.Print(body_shape_index);
		// GD.Print(local_shape_index);

		// if (body != null)
		// {
		// 	var tileMap = body as TileMap;

		// 	if (tileMap != null)
		// 	{
		// 		var coordinates = tileMap.GetCoordsForBodyRid(body_rid);
		// 		GD.Print(coordinates);
		// 		var worldCoordinats = tileMap.MapToLocal(coordinates);
		// 		// GD.Print(ToLocal(worldCoordinats).Normalized());
		// 		var reflect = (worldCoordinats - GlobalPosition).Normalized();

		// 		GD.Print(body);
		// 		GD.Print(body_rid);
		// 		GD.Print(reflect);
		// 	}
		// }
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
		// currentForceMultiplier = RNG_Manager.rng.RandfRange(minForceMultiplier, maxForceMultiplier);
		// CurrentDirection = new Vector2(RNG_Manager.rng.RandfRange(-1, 1), RNG_Manager.rng.RandfRange(-1, 1)) * currentForceMultiplier;
	}

	public void _OnBubbleTimerTimeout()
	{
		Bubble = BubbleScene.Instantiate<Bubble>();
		GetParent().AddChild(Bubble);
		Bubble.LifeTimer.Timeout += _OnBubbleLifeTimeout;

		Bubble.Init(GetRandomMessage(), GlobalPosition);
	}

	public void _OnBubbleLifeTimeout()
	{
		// Bubble.QueueFree();
		// RemoveChild(Bubble);
		// Task.Delay(500).ContinueWith(t => { Bubble = null; });
		// Bubble.CallDeferred("QueueFree");
		Bubble = null;
	}

	public void LoadMessages()
	{
		using var file = FileAccess.Open("res://messages.txt", FileAccess.ModeFlags.Read);

		while (!file.EofReached())
			messages.Add(file.GetLine());

		file.Close();
	}

	public string GetRandomMessage()
	{
		while (true)
		{
			var newMessage = messages[RNG_Manager.rng.RandiRange(0, messages.Count - 1)];
			if (newMessage != currentMessage)
			{
				currentMessage = newMessage;
				break;
			}
		}
		return currentMessage;
	}
}
