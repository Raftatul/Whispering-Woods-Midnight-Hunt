using Godot;
using System;

public partial class WalkPlayerState : PlayerState
{
    public override void Enter()
    {
        Player.MoveSpeed = Player.PlayerData.WalkSpeed;
        
        Player.WalkAnimationPlayer.Play("Walk");
    }

    public override void Update(float delta)
    {
        if (Player.Velocity.Length() == 0.0)
            EmitSignal(SignalName.Transition, "Idle");
    }

    public override void PhysicsUpdate(float delta)
    {
        Player.RegenStamina(delta);

        if (!Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Jump");
    }

    public override void Input(InputEvent @event)
    {
        if (@event.IsActionPressed("run"))
            EmitSignal(SignalName.Transition, "Run");
        if (@event.IsActionPressed("crouch"))
        {
            Player.CrouchAnimationPlayer.Play("Crouch");
            Player.AnimationManager.RequestTransition("Trans_Crouch/transition_request", "crouch");

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
