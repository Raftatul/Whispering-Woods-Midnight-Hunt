using Godot;
using System;

[GlobalClass]
public partial class PlayerData : Resource
{
    [Export]
    public float WalkSpeed = 5f;

    [Export]
    public float CrouchSpeed = 2f;

    [Export]
    public float RunSpeed = 10f;

    [Export]
    public float  JumpForce = 5f;

    [Export]
    public float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
}
