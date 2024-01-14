using Godot;
using System;

public partial class Item : RigidBody3D, IInteractable
{
    public void Interact()
    {
        QueueFree();
    }
}
