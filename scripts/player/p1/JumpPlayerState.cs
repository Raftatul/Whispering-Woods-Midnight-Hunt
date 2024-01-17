using Godot;
using System;

public partial class JumpPlayerState : PlayerState
{
    public override void Enter()
    {
        Player.TargetVelocity.Y = Player.PlayerData.JumpForce;
        Player.WalkAnimationPlayer.Pause();

        Player.AnimationManager.Rpc("RequestTransition", "Trans_Jump/transition_request", "jump");
    }

    public override void PhysicsUpdate(float delta)
    {
        if (Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Idle");
    }

    public override void Exit()
    {
        Player.AnimationManager.Rpc("RequestTransition", "Trans_Jump/transition_request", "land");
    }
}
