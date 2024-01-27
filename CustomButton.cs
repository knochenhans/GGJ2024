using Godot;
using System;

public partial class CustomButton : Button
{
	[Export]
	public AudioStream ButtonDownSound { get; set; }

	[Export]
	public AudioStream MouseEnteredSound { get; set; }

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

	public void _OnMouseEntered()
	{
		SoundsNode.Stream = MouseEnteredSound;
		SoundsNode.Play();
	}
}
