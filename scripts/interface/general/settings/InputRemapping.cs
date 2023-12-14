using Godot;

public partial class InputRemapping : Control
{
    [Export]
    private string _action;

    [Export]
    private Label _label;

    [Export]
    private Button _button;

    public override void _Ready()
    {
        UpdateText();

        SetProcessUnhandledInput(false);

        _button.Pressed += () => SetProcessUnhandledInput(true);
        _button.Pressed += () => _button.Disabled = true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsPressed())
            return;

        InputMap.ActionEraseEvents(_action);
        InputMap.ActionAddEvent(_action, @event);

        SetProcessUnhandledInput(false);
        _button.Disabled = false;
        UpdateText();
    }

    private void UpdateText()
    {
        _label.Text = _action;
        _button.Text = InputMap.ActionGetEvents(_action)[0].AsText();
    }

    public void SetAction(string action)
    {
        _action = action;
        UpdateText();
    }
}