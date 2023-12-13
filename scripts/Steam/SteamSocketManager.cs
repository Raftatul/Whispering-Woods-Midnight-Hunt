using Steamworks;
using Steamworks.Data;
using System;
using Godot;

public class SteamSocketManager : SocketManager
{
    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        base.OnConnected(connection, info);
        GD.Print("OnConnected fired");
    }

    public override void OnConnecting(Connection connection,ConnectionInfo info)
    {
        base.OnConnecting(connection, info);
        GD.Print("OnConnecting fired");
    }

    public override void OnDisconnected(Connection connection,ConnectionInfo info)
    {
        base.OnDisconnected(connection, info);
        GD.Print("OnDisconnected fired");
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
        DataParser.ProcessData(data, size);
    }
}