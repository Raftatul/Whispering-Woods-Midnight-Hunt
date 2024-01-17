using Godot;
using System;

public partial class CustomRemoteTransform3D : RemoteTransform3D
{
    public override void _PhysicsProcess(double delta)
    {
        if (RemotePath == null)
            return;
        
        var target = GetNode<Node3D>(RemotePath);

        Vector3 targetPosition = target.GlobalPosition;
        targetPosition.Y = GlobalPosition.Y;

        target.GlobalPosition = targetPosition;
    }
}
