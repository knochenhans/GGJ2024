using Godot;
using System;

public partial class CursorAttachment : Sprite2D
{
	public override void _Ready()
	{
		GD.Print(Name);
		Position = new Vector2(0, 0);
	}

	public override void _PhysicsProcess(double delta)
	{
		var mousePosition = GetGlobalMousePosition();
		Position = mousePosition;
	}
}
