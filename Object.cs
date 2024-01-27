using Godot;
using System;

public partial class Object : RigidBody2D
{
	[Export]
	public int Damage { get; set; } = 1;
	
	[Export]
	public int Laugh { get; set; } = 1;

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
}
