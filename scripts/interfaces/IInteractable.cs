using Godot;
using System;

[GlobalClass]
public partial class IInteractable : Area3D
{
    [Export]
    public string InteractionText = "Interact";

    [Signal]
    public delegate void InteractedEventHandler();

    public override void _Ready()
    {
        CollisionLayer = (uint)1<<31;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Interact()
    {
        EmitSignal(SignalName.Interacted);
    }
}