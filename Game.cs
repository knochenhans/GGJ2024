using Godot;

public static class RNG_Manager
{
    public static RandomNumberGenerator rng = null;
}

public static class GlobalConstants
{

}

public partial class Game : Scene
{
    AudioStream[] audioStreams = new AudioStream[6];

    Character Character { get; set; }

    Interface Interface { get; set; }

    int characterLastLife = 0;

    public int Laugh
    {
        get => _laugh; set
        {
            if (value < 0)
            {
                value = 0;
            }

            _laugh = value;
            Interface.UpdateLaugh(_laugh);
        }
    }

    int _laugh = 0;

    public Game()
    {
        RNG_Manager.rng = new RandomNumberGenerator();

        for (int i = 0; i < audioStreams.Length; i++)
        {
            audioStreams[i] = (AudioStream)GD.Load($"res://sounds/laugh{i + 1}.ogg");
        }
    }

    public override void _Ready()
    {
        // base._Ready();
        Interface = GetNode<Interface>("Interface");

        Character = GetNode<Character>("Character");

        Character.TriggerLaugh += _OnCharacterTriggerLaugh;
        Character.LifeChanged += _OnCharacterLifeChanged;
        Character.Init();
    }

    public void _OnCharacterTriggerLaugh()
    {
        var index = RNG_Manager.rng.RandiRange(0, audioStreams.Length - 1);
        var audioStreamPlayer = GetNode<AudioStreamPlayer>("LaughterSounds");
        audioStreamPlayer.Stream = audioStreams[index];
        audioStreamPlayer.Play();
    }

    public void _OnCharacterLifeChanged(int life)
    {
        Interface.UpdateLife(life);
        Laugh += characterLastLife - life;
        characterLastLife = life;
    }

    public void _OnLaughterReduceTimerTimeout()
    {
        Laugh -= 1;
    }
}
