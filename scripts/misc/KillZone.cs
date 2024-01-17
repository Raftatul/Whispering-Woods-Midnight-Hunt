using Godot;
using System;

public partial class KillZone : Area3D
{
    public override void _Ready()
    {
        BodyEntered += OnKillZoneBodyEntered;
    }

    public void OnKillZoneBodyEntered(Node3D body)
    {
        body.GlobalPosition = new Vector3(0, 10, 0);
    }
}
