using Godot;
using System;

public partial class JumpPlayerState : PlayerState
{
    public override void Enter()
    {
        if (Player.IsOnFloor())
            Player.TargetVelocity.Y = Player.PlayerData.JumpForce;
        
        Player.CameraAnimPlayer.Pause();

        Player.AnimationManager.Rpc("RequestTransition", Player.TransJump, "jump");
    }

    public override void PhysicsUpdate(float delta)
    {
        Player.UpdateGravity(delta);
        Player.UpdateVelocity();

        if (Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Idle");
    }

    public override void Exit()
    {
        Player.AnimationManager.Rpc("RequestTransition", Player.TransJump, "land");
    }
}
