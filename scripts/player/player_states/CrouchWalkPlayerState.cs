using Godot;
using System;

public partial class CrouchWalkPlayerState : PlayerState
{
    [Export]
    private RayCast3D _rayCast;

    [Export]
    private CollisionShape3D _standUpCollider;

    public override void Enter()
    {
        Player.MoveSpeed = Player.PlayerData.CrouchSpeed;

        Player.CameraAnimPlayer.Play("CrouchWalk", customSpeed: 1.25f);
    }

    public override void PhysicsUpdate(float delta)
    {
        Player.UpdateGravity(delta);
        Player.UpdateInput();
        Player.UpdateVelocity();

        Player.RegenStamina(delta);

        if (Player.Velocity.Length() == 0f)
            EmitSignal(SignalName.Transition, "Crouch");
        if (!Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "Jump");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UnCrouch()
    {
        _standUpCollider.Disabled = false;
        Player.AnimationManager.RequestTransition(Player.TransCrouch, "uncrouch");
    }

    public override void Input(InputEvent @event)
    {
        if ((@event.IsActionPressed("jump") || @event.IsActionPressed("crouch")) && !_rayCast.IsColliding())
        {
            Rpc(MethodName.UnCrouch);

            EmitSignal(SignalName.Transition, "Walk");
        }
    }

    public override void Exit()
    {
        Player.CameraAnimPlayer.Pause();
    }
}
