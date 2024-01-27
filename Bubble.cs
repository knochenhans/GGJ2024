using Godot;
using System;

public partial class Bubble : PanelContainer
{
	[Export]
	public float lifeTime = 1f;

	[Export]
	public Vector2 Distance = new Vector2(0, -20);

	public Timer LifeTimer { get; set; }

	public override void _Ready()
	{
		LifeTimer = GetNode<Timer>("LifeTimer");
		LifeTimer.WaitTime = lifeTime;
	}

	public void Init(string message, Vector2 position)
	{
		UpdatePosition(position);
		var label = GetNode<Label>("%Label");
		label.Text = message;
	}

    public void _OnLifeTimerTimeout()
    {
    	QueueFree();
    }

    // public override void _ExitTree()
    // {
    //     QueueFree();
    // }

    public void UpdatePosition(Vector2 position)
	{
		var widthOffset = Size.X / 2;
		Position = position + Distance - new Vector2(widthOffset, 0);
	}
}
