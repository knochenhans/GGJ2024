using Godot;

public static class GlobalConstants
{

}

public enum GameStateEnum
{
    Playing,
    Paused,
    GameOver,
    Won
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
            switch (value)
            {
                case GameStateEnum.Playing:
                    break;
                case GameStateEnum.Paused:
                    break;
                case GameStateEnum.GameOver:
                    Character.Enable(false);
                    LaughFace1.ZIndex = 101;
                    LaughFace1.GameOver();
                    LaughFace1.GetNode<AnimationPlayer>("AnimationPlayer").Play("won");
                    GetNode<Label>("GameOverLabel").Visible = true;
                    PhysicsServer2D.SetActive(false);
                    break;
                case GameStateEnum.Won:
                    Character.Enable(false);
                    LaughFace2.ZIndex = 101;
                    LaughFace2.GameOver();
                    LaughFace2.GetNode<AnimationPlayer>("AnimationPlayer").Play("won");
                    GetNode<AudioStreamPlayer>("WonSound").Play();
                    GetNode<Label>("WonLabel").Visible = true;
                    PhysicsServer2D.SetActive(false);
                    break;

            }
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
                value = 0;

            _laugh = value;
            Interface.UpdateLaugh(_laugh);
        }
    }

    Godot.Collections.Dictionary<string, int> AvailableObject = new();
    PackedScene CurrentObjectScene;
    string CurrentObjectKey;

    Tilemap TileMap { get; set; }

    CursorAttachment CursorAttachment { get; set; } = null;

    LaughFace LaughFace1 { get; set; }
    LaughFace LaughFace2 { get; set; }

    [ExportCategory("Debug")]
    [Export]
    public bool PlaceHealing { get; set; } = false;

    [Export]
    public bool DontReduceLaugh { get; set; } = false;

    [Export]
    public bool WinOnWKey { get; set; } = false;

    public Game()
    {
        for (int i = 0; i < audioStreams.Length; i++)
            audioStreams[i] = (AudioStream)GD.Load($"res://sounds/laugh{i + 1}.ogg");
    }

    public override void _Ready()
    {
        AvailableObject.Add("Sign", 30);
        AvailableObject.Add("Crate", 15);
        AvailableObject.Add("Hammer", 10);
        // base._Ready();
        Interface = GetNode<Interface>("Interface");
        Interface.ObjectClicked += _OnInterfaceObjectClicked;

        Character = GetNode<Character>("Character");

        Character.TriggerLaugh += _OnCharacterTriggerLaugh;
        Character.LifeChanged += _OnCharacterLifeChanged;
        Character.LaughChanged += _OnCharacterLaughChanged;
        Character.Dead += _OnCharacterDead;

        Character.Init();

        LaughterSoundsNode = GetNode<AudioStreamPlayer>("LaughterSounds");
        LaughterSoundsDefaultVolume = LaughterSoundsNode.VolumeDb;

        foreach (var item in AvailableObject)
        {
            Interface.AddObjectPanel(item.Key, (Texture2D)GD.Load($"res://images/{item.Key}.png"));
            Interface.SetObjectCount(item.Key, item.Value);
        }

        TileMap = GetNode<Tilemap>("TileMap");
        TileMap.Clicked += _OnTilemapClicked;

        LaughFace1 = GetNode<LaughFace>("LaughFace1");
        LaughFace2 = GetNode<LaughFace>("LaughFace2");
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
        // Laugh += characterLastLife - life;
        characterLastLife = life;
    }

    public void _OnLaughterReduceTimerTimeout()
    {
        if (GameState == GameStateEnum.Playing)
            if (!DontReduceLaugh)
                Laugh -= 1;
    }

    public void _OnCharacterLaughChanged(int laugh)
    {
        Laugh += laugh;

        GD.Print($"Laugh: {laugh}");

        if (laugh == 100)
            GameState = GameStateEnum.Won;
    }

    public void _OnTilemapClicked(Vector2 position, InputEventMouseButton eventMouseButton)
    {
        if (GameState == GameStateEnum.Playing)
        {
            if (CurrentObjectScene != null)
            {
                if (eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.IsReleased())
                {
                    // var obj = CurrentObjectScene.Instantiate<Object>();
                    // obj.GlobalPosition = position;
                    // AddChild(obj);
                    PlaceCurrentObject(position);
                }
            }
            else if (PlaceHealing)
            {
                if (eventMouseButton.ButtonIndex == MouseButton.Right)
                    SpawnHealing(position);
            }
        }
    }

    public void PlaceCurrentObject(Vector2 position)
    {
        if (GameState == GameStateEnum.Playing)
        {
            if (CurrentObjectScene != null)
            {
                if (AvailableObject[CurrentObjectKey] > 0)
                {
                    if (Mathf.Abs(Character.GlobalPosition.DistanceTo(position)) < 32)
                    {
                        GetNode<AudioStreamPlayer>("CantPlaceObjectSound").Play();
                        return;
                    }
                    foreach (var item in GetTree().GetNodesInGroup("Object"))
                    {
                        if (item is Object)
                        {
                            var _obj = item as Object;
                            if (Mathf.Abs(_obj.GlobalPosition.DistanceTo(position)) < 40)
                            {
                                GetNode<AudioStreamPlayer>("CantPlaceObjectSound").Play();
                                return;
                            }
                        }
                    }

                    var obj = CurrentObjectScene.Instantiate<Object>();
                    obj.GlobalPosition = position;
                    AddChild(obj);
                    GetNode<AudioStreamPlayer>("PlaceObjectSound").Play();

                    AvailableObject[CurrentObjectKey] -= 1;
                    Interface.SetObjectCount(CurrentObjectKey, AvailableObject[CurrentObjectKey]);
                }
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (GameState == GameStateEnum.Playing)
        {
            if (@event is InputEventMouseButton eventMouseButton)
            {
                if (eventMouseButton.Pressed)
                {
                    if (eventMouseButton.ButtonIndex == MouseButton.Right)
                    {
                        RemoveCursorAttachment();
                        CurrentObjectScene = null;
                    }
                }
            }
            if (@event is InputEventKey eventKey)
            {
                if (eventKey.Pressed)
                {
                    if (eventKey.Keycode == Key.Escape)
                    {
                        if (GameState == GameStateEnum.Playing)
                            GameState = GameStateEnum.Paused;
                        else if (GameState == GameStateEnum.Paused)
                            GameState = GameStateEnum.Playing;
                    }

                    if (WinOnWKey)
                    {
                        if (eventKey.Keycode == Key.W)
                            // GameState = GameStateEnum.Won;
                            GameState = GameStateEnum.GameOver;
                    }
                }
            }
        }

        if (GameState == GameStateEnum.GameOver)
        {
            if (@event is InputEventKey eventKey)
            {
                if (eventKey.Pressed)
                {
                    if (eventKey.Keycode == Key.Escape)
                        GetTree().Quit();
                        // GetNode<SceneManager>("/root/SceneManager").SwitchScene("res://MainMenu.tscn");
                }
            }
        }
    }

    public void _OnInterfaceObjectClicked(string key)
    {
        if (GameState == GameStateEnum.Playing)
        {
            RemoveCursorAttachment();
            if (AvailableObject[key] > 0)
            {
                CurrentObjectScene = ResourceLoader.Load<PackedScene>($"res://{key}.tscn");
                CurrentObjectKey = key;

                SetCursorAttachment(key);
            }
        }
    }

    public void SetCursorAttachment(string key)
    {
        CursorAttachment = ResourceLoader.Load<PackedScene>($"res://CursorAttachment.tscn").Instantiate() as CursorAttachment;
        CursorAttachment.Texture = (Texture2D)GD.Load($"res://images/{key}_small.png");
        GetTree().Root.AddChild(CursorAttachment);
    }

    public void RemoveCursorAttachment()
    {
        if (CursorAttachment != null)
        {
            CursorAttachment.QueueFree();
            CursorAttachment = null;
            Interface.HighlightedObjectPanelReset();
        }
    }

    public void _OnCharacterDead()
    {
        GameState = GameStateEnum.GameOver;
    }

    public void SpawnHealing(Vector2 position)
    {
        if (GameState == GameStateEnum.Playing)
        {
            var healing = ResourceLoader.Load<PackedScene>("res://Healing.tscn").Instantiate<Healing>();
            healing.GlobalPosition = position;
            AddChild(healing);
        }
    }

    public void _OnHealingSpawnTimerTimeout() => SpawnHealing(TileMap.GetFreePosition());
}
