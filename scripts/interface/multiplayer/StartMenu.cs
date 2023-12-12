using Godot;
using System;
using Steamworks;

public partial class StartMenu : Control
{
    public override void _Ready()
    {
        SteamMatchmaking.OnLobbyCreated += (result, lobby) => Visible = false;
        SteamMatchmaking.OnLobbyEntered += (lobby) => Visible = false;
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
