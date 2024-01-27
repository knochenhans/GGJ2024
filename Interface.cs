using Godot;
using System;

public partial class Interface : Control
{
	public void UpdateLife(int life)
	{
		var lifeProgress = GetNode<ProgressBar>("%LifeBar");
		lifeProgress.Value = life;
	}

	public void UpdateLaugh(int laugh)
	{
		var laughProgress = GetNode<ProgressBar>("%LaughBar");
		laughProgress.Value = laugh;
	}
}
