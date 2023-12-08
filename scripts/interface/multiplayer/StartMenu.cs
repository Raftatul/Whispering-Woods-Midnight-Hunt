using Godot;
using System;

public partial class StartMenu : Control
{
    private void QuitGame()
    {
        GetTree().Quit();
    }
}
