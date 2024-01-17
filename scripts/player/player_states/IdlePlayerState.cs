using Godot;
using System;

public partial class IdlePlayerState : PlayerState
{
    public override void Enter()
    {
        Player.TargetVelocity.Y = 0f;
        Player.MoveSpeed = Player.PlayerData.WalkSpeed;
    }

    public override void Update(float delta)
    {
        if (Player.Velocity.Length() > 0.0 && Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Walk");
    }

    public override void PhysicsUpdate(float delta)
    {
        Player.RegenStamina(delta);

        if (!Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Jump");
    }

    public override void Input(InputEvent @event)
    {
        if (@event.IsActionPressed("crouch"))
        {
            Player.CrouchAnimationPlayer.Play("Crouch");
            Player.AnimationManager.RequestTransition("Trans_Crouch/transition_request", "crouch");

            EmitSignal(SignalName.Transition, "Crouch");
        }
        if (@event.IsActionPressed("jump") && Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Jump");
    }
}
