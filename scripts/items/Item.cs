using Godot;
using System;

public partial class Item : RigidBody3D
{
    [Export]
    private Interactable _interactable;

    public override void _Ready()
    {
        _interactable.OnInteracted += () => GD.Print("Interacted with " + Name);
    }
}
