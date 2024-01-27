using Godot;
using System;

public partial class Interface : Control
{
	Godot.Collections.Dictionary<string, PanelContainer> ObjectPanels = new();
	HBoxContainer objectPanelContainer;

	public override void _Ready()
	{
		objectPanelContainer = GetNode<HBoxContainer>("%ObjectPanelContainer");
	}

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

	public void AddObjectPanel(string key, Texture2D texture)
	{
		var objectPanel = ResourceLoader.Load<PackedScene>("res://ObjectPanel.tscn").Instantiate();
		objectPanel.GetNode<TextureButton>("%TextureButton").TextureNormal = texture;
		objectPanelContainer.AddChild(objectPanel);
		ObjectPanels.Add(key, (PanelContainer)objectPanel);
	}

	public void SetObjectCount(string key, int count)
	{
		var objectPanel = ObjectPanels[key];
		objectPanel.GetNode<Label>("%CountLabel").Text = count.ToString();
	}
}
