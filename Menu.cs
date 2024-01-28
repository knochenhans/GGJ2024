using Godot;
using System;

public partial class Menu : Control
{
	SceneManager SceneManager;

	public override void _Ready()
	{
		SceneManager = GetNode<SceneManager>("/root/SceneManager");

		GetNode<Label>("LabelQuote").Text = "“" + MessageManager.GetRandomMessage() + "”";
	}

	public void _OnStartButtonPressed()
	{
		SceneManager.SwitchScene("Game");
	}

	public void _OnOptionsButtonPressed()
	{
		SceneManager.SwitchScene("Options");
	}

	public void _OnExitButtonPressed()
	{
		SceneManager.Quit();
	}
}
