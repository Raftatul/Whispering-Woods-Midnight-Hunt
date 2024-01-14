using Godot;
using System;

public partial class Item : RigidBody3D
{
    [Export]
    private IInteractable _interactable;

    public void Interact()
    {
        QueueFree();
    }
}
