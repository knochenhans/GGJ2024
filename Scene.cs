using Godot;
using System;

public partial class Scene : Node2D
{
	public async override void _Ready()
	{
		GD.Print(Name);
		var fade = GetNode<Fade>("Fade");
		fade.FadeIn(1f);

		await ToSignal(fade.AnimationPlayerNode, "animation_finished");
		
		fade.QueueFree();
	}
}
