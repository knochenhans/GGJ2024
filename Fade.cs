using Godot;
using System;

public partial class Fade : ColorRect
{
	public AnimationPlayer AnimationPlayerNode { get; set; }

	public override void _Ready()
	{
		base._Ready();

		AnimationPlayerNode = GetNode<AnimationPlayer>("AnimationPlayer");

		var _interface = GetParent().GetNodeOrNull<Interface>("Game/Interface");

		if (_interface != null)
		{
			Position = GetViewportRect().Position - _interface.Position;
			Size = GetViewportRect().Size;
		}
	}

	public void FadeIn(float duration)
	{
		AnimationPlayerNode.Play("FadeIn");
		AnimationPlayerNode.Seek(0, true);
		AnimationPlayerNode.SpeedScale = 1 / duration;
	}

	public void FadeOut(float duration)
	{
		AnimationPlayerNode.Play("FadeOut");
		AnimationPlayerNode.Seek(0, true);
		AnimationPlayerNode.SpeedScale = 1 / duration;
	}
}
