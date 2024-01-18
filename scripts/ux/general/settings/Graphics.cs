using Godot;

public partial class Graphics : Control
{
    [Export]
    private OptionButton _screenOptionButton;

    private Godot.Collections.Dictionary _screenModes = new Godot.Collections.Dictionary{
        {"Windowed", ((int)DisplayServer.WindowMode.Windowed)},
        {"Fullscreen", ((int)DisplayServer.WindowMode.Fullscreen)}
    };

    [Export]
    private OptionButton _resolutionOptionButton;

    private Godot.Collections.Dictionary _resolutions = new Godot.Collections.Dictionary{
        {"1920x1080", new Vector2I(1920, 1080)},
        {"1600x900", new Vector2I(1600, 900)},
        {"1366x768", new Vector2I(1366, 768)},
        {"1280x720", new Vector2I(1280, 720)},
        {"1024x768", new Vector2I(1024, 600)},
        {"800x600", new Vector2I(800, 600)}
    };

    [Export]
    private OptionButton _qualityOptionButton;

    [Export]
    private CheckButton _vsyncCheckButton;

    public override void _Ready()
    {
        AddWindowModes();
        AddResolutions();

        _screenOptionButton.ItemSelected += OnScreenOptionButtonItemSelected;
        _resolutionOptionButton.ItemSelected += OnResolutionOptionButtonItemSelected;
        _vsyncCheckButton.Toggled += SetVsyncMode;
    }

    private void AddWindowModes()
    {
        _screenOptionButton.Clear();

        foreach (var mode in _screenModes.Keys)
        {
            _screenOptionButton.AddItem(mode.ToString());
        }
    }

    private void AddResolutions()
    {
        _resolutionOptionButton.Clear();

        foreach (var resolution in _resolutions.Keys)
        {
            _resolutionOptionButton.AddItem(resolution.ToString());
        }
    }

    private void OnScreenOptionButtonItemSelected(long index)
    {
        GetWindow().Mode = (Window.ModeEnum)((int)_screenModes[_screenOptionButton.GetItemText((int)index)]);
        CenterWindow();
    }

    private void OnResolutionOptionButtonItemSelected(long index)
    {
        Vector2I resolution = _resolutions[_resolutionOptionButton.GetItemText(((int)index))].As<Vector2I>();
        DisplayServer.WindowSetSize(resolution);
        GetTree().Root.ContentScaleSize = resolution;
        CenterWindow();
    }

    private void CenterWindow()
    {
        Vector2I screenMiddle = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
        Vector2I windowSize = GetWindow().GetSizeWithDecorations();
        GetWindow().Position = screenMiddle - windowSize / 2;
    }

    private void SetVsyncMode(bool value)
    {
        if (value)
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
        else
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
    }
}