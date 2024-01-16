using Godot;
using Steamworks;

public partial class StartMenu : Control
{
    public override void _Ready()
    {
        SteamMatchmaking.OnLobbyCreated += (result, lobby) => Visible = false;
        SteamMatchmaking.OnLobbyEntered += (lobby) => Visible = false;
        SteamManager.OnPlayerLeftLobby += PlayerLeftLobby;
    }

    private void PlayerLeftLobby(Friend friend)
    {
        GD.Print("Player left lobby: Start menu");
        if (friend.IsMe)
        {
            GD.Print("I left lobby: Start menu");
            Visible = true; // Show the start menu again
        }
    }

    private void StartGame()
    {
        GetTree().ChangeSceneToFile("res://main.tscn");
    }

    private void QuitGame()
    {
        GetTree().Quit();
    }
}