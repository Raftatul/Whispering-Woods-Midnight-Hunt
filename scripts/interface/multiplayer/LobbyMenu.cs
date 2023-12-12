using Godot;
using System;
using Steamworks;

public partial class LobbyMenu : Control
{
    public override void _Ready()
    {
        SteamMatchmaking.OnLobbyCreated += (result, lobby) => Visible = true;
        SteamMatchmaking.OnLobbyEntered += (lobby) => Visible = true;
    }
}
