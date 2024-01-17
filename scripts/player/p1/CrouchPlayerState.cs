using Godot;
using System;

public partial class CrouchPlayerState : PlayerState
{
    [Export]
    private RayCast3D _rayCast;

    [Export]
    private CollisionShape3D _standUpCollider;

    public override void Enter()
    {
        _standUpCollider.Disabled = true;
        Player.AnimationManager.RequestTransition("Trans_Crouch/transition_request", "crouch");
    }

    public override void PhysicsUpdate(float delta)
    {
        Player.RegenStamina(delta);

        if (Player.Velocity.Length() > 0.0f && Player.IsOnFloor())
            EmitSignal(SignalName.Transition, "CrouchWalk");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UnCrouch()
    {
        _standUpCollider.Disabled = false;
        Player.CrouchAnimationPlayer.PlayBackwards("Crouch");
        Player.AnimationManager.RequestTransition("Trans_Crouch/transition_request", "uncrouch");
    }

    public override void Input(InputEvent @event)
    {
        if ((@event.IsActionPressed("jump") || @event.IsActionPressed("crouch")) && !_rayCast.IsColliding())
        {
            Rpc(MethodName.UnCrouch);

            EmitSignal(SignalName.Transition, "Idle");
        }
    }
}
