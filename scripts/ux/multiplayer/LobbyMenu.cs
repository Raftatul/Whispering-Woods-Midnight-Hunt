using Godot;
using Steamworks;

public partial class LobbyMenu : Control
{
    public override void _Ready()
    {
        SteamMatchmaking.OnLobbyCreated += (result, lobby) => Visible = true;
        SteamMatchmaking.OnLobbyEntered += (lobby) => Visible = true;
        SteamManager.OnPlayerLeftLobby += PlayerLeftLobby;
    }

    private void PlayerLeftLobby(Friend friend)
    {
        GD.Print("Player left lobby: Lobby menu");
        if (friend.IsMe)
        {
            GD.Print("I left lobby: Lobby menu");
            Visible = false;
        }
    }
}