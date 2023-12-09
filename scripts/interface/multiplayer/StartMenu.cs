using Godot;
using System;

public partial class StartMenu : Control
{
    private void StartGame()
    {
        GetTree().ChangeSceneToFile("res://main.tscn");
    }
    
    private void QuitGame()
    {
        GetTree().Quit();
    }
}
