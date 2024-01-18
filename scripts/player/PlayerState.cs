using Godot;
using System;

public partial class PlayerState : State
{
    public PlayerController Player;

    public override async void _Ready()
    {
        await ToSignal(Owner, "ready");
        Player = Owner as PlayerController;
    }
}
