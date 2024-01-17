using Godot;
using System;

public partial class RunPlayerState : PlayerState
{
    public override void Enter()
    {
        Player.MoveSpeed = Player.PlayerData.RunSpeed;

        Player.CameraAnimPlayer.Play("Walk", customSpeed: 4f);
    }

    public override void PhysicsUpdate(float delta)
    {
        Player.UpdateGravity(delta);
        Player.UpdateInput();
        Player.UpdateVelocity();

        Player.DepleteStamina(delta);
        
        if (Player.CurrentStamina <= 0.0f)
            EmitSignal(SignalName.Transition, "Walk");
        if (!Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Jump");
    }

    public override void Input(InputEvent @event)
    {
        if (@event.IsActionReleased("run"))
            EmitSignal(SignalName.Transition, "Walk");
        if (@event.IsActionPressed("crouch"))
        {
            Player.CameraAnimPlayer.Play("Crouch");
            EmitSignal(SignalName.Transition, "CrouchWalk");
        }
        if (@event.IsActionPressed("jump") && Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Jump");
    }

    public override void Exit()
    {
        Player.CameraAnimPlayer.Pause();
    }
}
