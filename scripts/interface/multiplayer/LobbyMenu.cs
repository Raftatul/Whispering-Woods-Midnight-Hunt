using Godot;
using Steamworks;

public partial class LobbyMenu : Control
{
    public override void _Ready()
    {
        SteamMatchmaking.OnLobbyCreated += (result, lobby) => Visible = true;
        SteamMatchmaking.OnLobbyEntered += (lobby) => Visible = true;
    }
}