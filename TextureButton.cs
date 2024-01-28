using Godot;
using System;

public partial class TextureButton : Godot.TextureButton
{
	[Export]
	public AudioStream ButtonDownSound { get; set; }

	public AudioStreamPlayer SoundsNode { get; set; }

	public override void _Ready()
	{
		SoundsNode = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
	}

	public void _OnButtonDown()
	{
		SoundsNode.Stream = ButtonDownSound;
		SoundsNode.Play();
	}
}
