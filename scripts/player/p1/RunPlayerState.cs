using Godot;
using System;

public partial class RunPlayerState : PlayerState
{
    public override void Enter()
    {
        Player.MoveSpeed = Player.PlayerData.RunSpeed;

        Player.WalkAnimationPlayer.Play("Walk", customSpeed: 2f);
    }

    public override void PhysicsUpdate(float delta)
    {
        Player.DepleteStamina(delta);
        
        if (Player.CurrentStamina <= 0.0f)
            EmitSignal(SignalName.Transition, "Walk");
    }

    public override void Input(InputEvent @event)
    {
        if (@event.IsActionReleased("run"))
            EmitSignal(SignalName.Transition, "Walk");
        if (@event.IsActionPressed("crouch"))
        {
            Player.CrouchAnimationPlayer.Play("Crouch");
            EmitSignal(SignalName.Transition, "CrouchWalk");
        }
        if (@event.IsActionPressed("jump") && Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Jump");
    }

    public override void Exit()
    {
        Player.WalkAnimationPlayer.Pause();
    }
}
