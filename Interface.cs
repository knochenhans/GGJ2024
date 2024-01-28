using Godot;
using System;

public partial class Interface : Control
{
	Godot.Collections.Dictionary<string, PanelContainer> ObjectPanels = new();
	HBoxContainer ObjectPanelContainer { get; set; } = null;
	PanelContainer HighlightedObjectPanel { get; set; } = null;

	[Signal]
	public delegate void ObjectClickedEventHandler(string key);

	public override void _Ready()
	{
		ObjectPanelContainer = GetNode<HBoxContainer>("%ObjectPanelContainer");
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

		var textureButton = objectPanel.GetNode<TextureButton>("%TextureButton");
		textureButton.TextureNormal = texture;
		textureButton.ButtonDown += () => _OnObjectButtonPressed(key);
		ObjectPanelContainer.AddChild(objectPanel);
		ObjectPanels.Add(key, (PanelContainer)objectPanel);
	}

	public void SetObjectCount(string key, int count)
	{
		var objectPanel = ObjectPanels[key];
		objectPanel.GetNode<Label>("%CountLabel").Text = count.ToString();
	}

	public void _OnObjectButtonPressed(string key)
	{
		foreach (var item in ObjectPanels)
			HighlightObject(item.Key, false);

		HighlightObject(key, true);
		EmitSignal(SignalName.ObjectClicked, key);
	}

	public void HighlightObject(string key, bool highlight)
	{
		var objectPanel = ObjectPanels[key];
		objectPanel.GetNode<PanelContainer>("%PanelContainer").AddThemeStyleboxOverride("panel", highlight ? ResourceLoader.Load<StyleBox>("res://ObjectPanelHighlight.tres") : ResourceLoader.Load<StyleBox>("res://ObjectPanelNormal.tres"));

		if (highlight)
			HighlightedObjectPanel = objectPanel;
		else if (HighlightedObjectPanel == objectPanel)
			HighlightedObjectPanel = null;
	}

	public void HighlightedObjectPanelReset()
	{
		if (HighlightedObjectPanel != null)
			HighlightedObjectPanel.GetNode<PanelContainer>("%PanelContainer").AddThemeStyleboxOverride("panel", ResourceLoader.Load<StyleBox>("res://ObjectPanelNormal.tres"));
	}
}
