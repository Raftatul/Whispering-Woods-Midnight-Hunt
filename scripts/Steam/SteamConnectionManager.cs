using Steamworks;
using Steamworks.Data;
using System;
using Godot;

public class SteamConnectionManager : ConnectionManager
{
    public override void OnConnected(ConnectionInfo info)
    {
        base.OnConnected(info);
        GD.Print("OnConnected fired");
    }

    public override void OnConnecting(ConnectionInfo info)
    {
        base.OnConnecting(info);
        GD.Print("OnConnecting fired");
    }

    public override void OnDisconnected(ConnectionInfo info)
    {
        base.OnDisconnected(info);
        GD.Print("OnDisconnected fired");
    }

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(data, size, messageNum, recvTime, channel);
        GD.Print("got message");
    }
}