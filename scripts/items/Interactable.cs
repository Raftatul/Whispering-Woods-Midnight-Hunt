using Godot;
using System;

public partial class Interactable : Area3D
{
    [Export]
    private float _interactionDelay = 1f;

    private Timer _interactionTimer;

    private bool _canBeInteracted = true;

    [Signal]
    public delegate void OnInteractedEventHandler();

    public override void _EnterTree()
    {
        _interactionTimer = new Timer();
        _interactionTimer.OneShot = true;
        _interactionTimer.WaitTime = _interactionDelay;

        _interactionTimer.Timeout += () => _canBeInteracted = true;

        AddChild(_interactionTimer);
    }

    public void Interact()
    {
        if (!_canBeInteracted)
            return;
        
        _interactionTimer.Start();
        _canBeInteracted = false;
        EmitSignal(SignalName.OnInteracted);
    }
}
