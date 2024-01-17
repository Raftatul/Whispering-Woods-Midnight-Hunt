using Godot;
using System;

public partial class GameDebug : Control
{
    public static GameDebug Instance { get; private set; }

    [Export]
    private VBoxContainer _vBoxContainer;

    public override void _Ready()
    {
        Visible = false;
        Instance = this;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("debug"))
            Visible = !Visible;
    }

    public void AddDebugProperty(string name, object value)
    {
        var target = _vBoxContainer.GetNodeOrNull<Label>(name);

        if (target != null)
        {
            target.Text = $"{name}: {value}";
            return;
        }

        var property = new Label();
        property.Text = $"{name}: {value}";
        property.Name = name;
        _vBoxContainer.AddChild(property);
    }
}
