using Godot;
using System;

public partial class Healing : Area2D
{
	[Export]
	public float LifeTime { get; set; } = 0f;

	[Export]
	public int Life { get; set; } = 10;

	AudioStreamPlayer2D TouchSoundNode { get; set; }

	public override void _Ready()
	{
		TouchSoundNode = GetNode<AudioStreamPlayer2D>("TouchSound");
	}

	public void _OnLifeTimerTimeout() => QueueFree();

	public async void Touch()
	{
		TouchSoundNode.Play();
		Visible = false;

		await ToSignal(TouchSoundNode, "finished");

		QueueFree();
	}
}
