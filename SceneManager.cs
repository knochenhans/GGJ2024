using System.Linq;
using Godot;

public partial class SceneManager : Node
{
	[Export]
	Godot.Collections.Dictionary<string, string> Scenes = new();

	PackedScene FadeScene = ResourceLoader.Load<PackedScene>("res://Fade.tscn");

	string CurrentSceneAlias { get; set; } = "";

	public override void _Ready()
	{
		base._Ready();

		Scenes.Add("Menu", "res://Menu.tscn");
		Scenes.Add("Options", "res://Options.tscn");
		Scenes.Add("Game", "res://Game.tscn");

		var mainScreen = ((StringName)ProjectSettings.GetSetting("application/run/main_scene"));
		var values = Scenes.Values.ToList();
		var keys = Scenes.Keys.ToList();
		CurrentSceneAlias = keys[values.IndexOf(mainScreen)];
	}

	public void AddScene(string sceneAlias, string scenePath)
	{
		Scenes.Add(sceneAlias, scenePath);
	}

	public void RemoveScene(string sceneAlias)
	{
		Scenes.Remove(sceneAlias);
	}

	public async void SwitchScene(string sceneAlias)
	{
		var fade = FadeScene.Instantiate<Fade>();
		GetTree().Root.AddChild(fade);
		fade.FadeOut(1f);

		await ToSignal(fade.AnimationPlayerNode, "animation_finished");

		fade.QueueFree();

		GetTree().ChangeSceneToFile(Scenes[sceneAlias]);
		CurrentSceneAlias = sceneAlias;

		// fade = FadeScene.Instantiate<Fade>();
		// GetTree().Root.AddChild(fade);
		// fade.FadeIn(1f);

		// await ToSignal(fade.AnimationPlayerNode, "animation_finished");

		// fade.QueueFree();
	}

	public void RestartScene()
	{
		GetTree().ReloadCurrentScene();
	}

	public void Quit()
	{
		GetTree().Quit();
	}

	public int GetSceneCount()
	{
		return Scenes.Count;
	}
}
