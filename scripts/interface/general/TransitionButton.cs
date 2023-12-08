using Godot;
using System;

public partial class TransitionButton : BaseButton
{
    [Export]
    private CanvasItem _nodeToHide;

    [Export]
    private CanvasItem _nodeToShow;

    public override void _Ready()
    {
        Pressed += () => _nodeToHide.Visible = false;
        Pressed += () => _nodeToShow.Visible = true;
    }
}
