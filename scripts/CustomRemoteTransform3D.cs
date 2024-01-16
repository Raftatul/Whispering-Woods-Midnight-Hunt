using Godot;
using System;

public partial class CustomRemoteTransform3D : RemoteTransform3D
{
    public override void _PhysicsProcess(double delta)
    {
        if (RemotePath == null)
            return;
        
        Vector3 targetPosition = GetNode<Node3D>(RemotePath).GlobalPosition;
        targetPosition.Y = GlobalPosition.Y;

        GetNode<Node3D>(RemotePath).GlobalPosition = targetPosition;
    }
}
