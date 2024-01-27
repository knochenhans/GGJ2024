using Godot;
using System;

public partial class Options : Control
{
	SceneManager SceneManager;

	// GameOptionsManager GameOptionsManager { get; set; } = new GameOptionsManager();

	PackedScene OptionSliderScene = (PackedScene)ResourceLoader.Load("res://OptionSlider.tscn");

	public override void _Ready()
	{
		SceneManager = GetNode<SceneManager>("/root/SceneManager");

		// var optionsContainer = GetNode<Control>("%OptionsContainer");

		// foreach (var option in GameOptionsManager.GameOptions)
		// {
		// 	var optionSlider = OptionSliderScene.Instantiate<Container>();
		// 	optionSlider.GetNode<Label>("Label").Text = option.Value.Title;
		// 	// optionSlider.GetNode<Label>("Label").TooltipText = option.Value.OptionDescription;
		// 	optionSlider.GetNode<Slider>("Slider").TooltipText = option.Value.Description;
		// 	optionSlider.GetNode<Slider>("Slider").Value = (float)option.Value.Value;
		// 	optionSlider.GetNode<Slider>("Slider").SetMeta("optionID", option.Key);
		// 	optionSlider.GetNode<Slider>("Slider").DragEnded += SliderDragEnded;

		// 	optionsContainer.AddChild(optionSlider);
		// }
	}

	public void SliderDragEnded(bool valueChanged)
	{
		// var focusedSlider = GetViewport().GuiGetFocusOwner() as Slider;
		// var optionID = focusedSlider.GetMeta("optionID").ToString();
		// GameOptionsManager.GameOptions[optionID].Value = focusedSlider.Value;
	}

	public void _OnCloseButtonPressed()
	{
		// GameOptionsManager.SaveOptions();
		SceneManager.SwitchScene("Menu");
	}
}
