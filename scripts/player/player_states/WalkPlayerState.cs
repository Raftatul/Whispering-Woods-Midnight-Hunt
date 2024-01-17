using Godot;
using System;

public partial class WalkPlayerState : PlayerState
{
    public override void Enter()
    {
        Player.TargetVelocity.Y = 0f;
        Player.MoveSpeed = Player.PlayerData.WalkSpeed;
        
        Player.CameraAnimPlayer.Play("Walk", customSpeed: 2f);
    }

    public override void Update(float delta)
    {
        if (Player.Velocity.Length() == 0.0)
            EmitSignal(SignalName.Transition, "Idle");
    }

    public override void PhysicsUpdate(float delta)
    {
        Player.UpdateGravity(delta);
        Player.UpdateInput();
        Player.UpdateVelocity();

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
            Player.AnimationManager.RequestTransition("Trans_Crouch/transition_request", "crouch");

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
