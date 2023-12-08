using Godot;
using System;

public partial class Controls : Control
{
    [Export]
    private InputAction _inputAction;

    [Export]
    private PackedScene _inputRemappingScene;

    [Export]
    private BoxContainer _actionsContainer;

    public override void _Ready()
    {
        foreach (Node child in _actionsContainer.GetChildren())
            child.QueueFree();

        foreach (var action in _inputAction.Actions)
        {
            InputRemapping inputRemapping = _inputRemappingScene.Instantiate<InputRemapping>();
            inputRemapping.SetAction(action);
            _actionsContainer.AddChild(inputRemapping);
        }
    }
}
