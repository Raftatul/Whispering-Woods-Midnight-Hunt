using Godot;
using Steamworks.Data;
using System;

public partial class LobbyElements : Control
{
	[Export]
	public Button JoinLobbyButton { get; set; }

	[Export]
	public Label LobbyIdLabel { get; set; }

	[Export]
	public Label LobbyNameLabel { get; set; }
	// Called when the node enters the scene tree for the first time.

	private Lobby lobby { get; set; }

	public void JoinLobbyButtonPressed()
	{
		lobby.Join();
	}

	public void SetLabels(string id, string name, Lobby lobby)
	{
		LobbyIdLabel.Text = id;
		LobbyNameLabel.Text = name;
		this.lobby = lobby;
	}

	
	#region  Godot Methods
	public override void _Ready()
	{
		JoinLobbyButton.Pressed += JoinLobbyButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	#endregion  Godot Methods
}
