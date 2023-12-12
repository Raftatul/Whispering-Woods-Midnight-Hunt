using Godot;
using Steamworks;
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
	public PackedScene PlayerCardScene { get; set; }

	[Export]
	public VBoxContainer LobbyListContainer { get; set; }

	[Export]
	public VBoxContainer PlayerListContainer { get; set; }

	public override void _Ready()
	{
		SteamManager.OnLobbyListRefreshedCompleted += OnLobbyListRefreshedCompletedCallback;
		CreateLobbyButton.Pressed += CreateLobbyButtonPressed;
		GetallLobbiesButton.Pressed += GetallLobbiesButtonPressed;
		InviteFriendButton.Pressed += InviteFriendButtonPressed;
		SteamManager.OnPlayerJoinedLobby += OnPlayerJoinedLobbyCallback;
		SteamManager.OnPlayerLeftLobby += OnPlayerLeftLobbyCallback;
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

	private void OnPlayerJoinedLobbyCallback(Friend friend)
	{
		var playerCard = PlayerCardScene.Instantiate() as PlayerCard;
		ImageTexture avatar = ImageTexture.CreateFromImage(SteamManager.GetImageFromSteamImage(friend.GetMediumAvatarAsync().Result.Value));
		playerCard.SetLabels(friend.Name, avatar);
		PlayerListContainer.AddChild(playerCard);
	}

	private void OnPlayerLeftLobbyCallback(Friend friend)
	{
		GD.Print("Player left lobby: " + friend.Name);
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
