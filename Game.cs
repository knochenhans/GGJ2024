using Godot;

public static class RNG_Manager
{
    public static RandomNumberGenerator rng = null;
}

public static class GlobalConstants
{

}

public enum GameStateEnum
{
    Playing,
    Paused,
    GameOver
}

public partial class Game : Scene
{
    AudioStream[] audioStreams = new AudioStream[6];

    Character Character { get; set; }

    Interface Interface { get; set; }

    int characterLastLife = 0;

    PackedScene SignScene = ResourceLoader.Load<PackedScene>("res://Sign.tscn");

    GameStateEnum _gameState = GameStateEnum.Playing;
    public GameStateEnum GameState
    {
        get => _gameState;
        set
        {
            _gameState = value;
        }
    }

    AudioStreamPlayer LaughterSoundsNode { get; set; }
    float LaughterSoundsDefaultVolume;

    int _laugh = 0;
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
        Character.LaughChanged += _OnCharacterLaughChanged;

        Character.Init();

        LaughterSoundsNode = GetNode<AudioStreamPlayer>("LaughterSounds");
        LaughterSoundsDefaultVolume = LaughterSoundsNode.VolumeDb;
    }

    public void _OnCharacterTriggerLaugh()
    {
        LaughterSoundsNode.VolumeDb = LaughterSoundsDefaultVolume + (3.0f / 100 * Laugh);
        if (!LaughterSoundsNode.Playing)
        {
            var index = RNG_Manager.rng.RandiRange(0, audioStreams.Length - 1);
            LaughterSoundsNode.Stream = audioStreams[index];
            LaughterSoundsNode.Play();
        }
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

    public void _OnCharacterLaughChanged(int laugh)
    {
        Laugh += laugh;
    }

    public override void _Input(InputEvent @event)
    {
        if (GameState == GameStateEnum.Playing)
        {
            if (@event is InputEventMouseButton eventMouseButton)
            {
                if (eventMouseButton.Pressed)
                {
                    GD.Print("Mouse Click/Unclick at: ", eventMouseButton.Position);
                    var sign = SignScene.Instantiate<Object>();
                    sign.GlobalPosition = GetLocalMousePosition();
                    AddChild(sign);
                }
            }
        }
    }
}
