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
        Player.UpdateGravity(delta);
        Player.UpdateInput();
        Player.UpdateVelocity();

        Player.RegenStamina(delta);

        if (!Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Jump");
    }

    public override void Input(InputEvent @event)
    {
        if (@event.IsActionPressed("crouch"))
        {
            Player.CameraAnimPlayer.Play("Crouch");
            Player.AnimationManager.RequestTransition(Player.TransCrouch, "crouch");

            EmitSignal(SignalName.Transition, "Crouch");
        }
        if (@event.IsActionPressed("jump") && Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Jump");
        if (@event.IsActionPressed("emote1"))
        {
            Player.AnimationManager.Rpc(AnimationManager.MethodName.RequestOneShot, Player.Emote1, (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
    }

    public override void Exit()
    {
        Player.AnimationManager.Rpc(AnimationManager.MethodName.RequestOneShot, Player.Emote1, (int)AnimationNodeOneShot.OneShotRequest.FadeOut);
    }
}
