using Godot;
using Steamworks.Data;
using System;
using System.Collections.Generic;

public partial class SceneManager : Node2D
{
	[Export]
	public Button CreateLobbyButton { get; set; }

	[Export]
	public Button GetallLobbiesButton { get; set; }

	[Export]
	public Button InviteFriendButton { get; set; }

	[Export]
	public PackedScene LobbyElementScene { get; set; }

	[Export]
	public VBoxContainer LobbyListContainer { get; set; }

	public override void _Ready()
	{
		SteamManager.OnLobbyListRefreshedCompleted += OnLobbyListRefreshedCompletedCallback;
		CreateLobbyButton.Pressed += CreateLobbyButtonPressed;
		GetallLobbiesButton.Pressed += GetallLobbiesButtonPressed;
		InviteFriendButton.Pressed += InviteFriendButtonPressed;
	}

	private void OnLobbyListRefreshedCompletedCallback(List<Lobby> lobbies)
	{
		foreach (var item in lobbies)
		{
			LobbyElements lobbyElement = LobbyElementScene.Instantiate() as LobbyElements;
			lobbyElement.SetLabels(item.Id.ToString(), item.GetData("ownerNameDataString") + " lobby", item);
			LobbyListContainer.AddChild(lobbyElement);
		}
		
	}

	public void CreateLobbyButtonPressed()
	{
		SteamManager.Instance.CreateLobby();
	}

	public void GetallLobbiesButtonPressed()
	{
		SteamManager.Instance.GetMultiplayerLobbyList();
	}

	public void InviteFriendButtonPressed()
	{
		SteamManager.Instance.OpenFriendInviteOverlay();
	}
}
