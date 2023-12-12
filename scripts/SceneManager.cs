using Godot;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;

public partial class SceneManager : Node
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
		foreach (var item in LobbyListContainer.GetChildren())
		{
			item.QueueFree();
		}

		foreach (var item in lobbies)
		{
			LobbyElements lobbyElement = LobbyElementScene.Instantiate() as LobbyElements;
			lobbyElement.SetLabels(item.GetData("ownerNameDataString") + " lobby", item);
			LobbyListContainer.AddChild(lobbyElement);
		}
		
	}

	private void OnPlayerJoinedLobbyCallback(Friend friend)
	{
		var playerCard = PlayerCardScene.Instantiate() as PlayerCard;
		playerCard.Name = friend.Id.AccountId.ToString();
		ImageTexture avatar = ImageTexture.CreateFromImage(SteamManager.GetImageFromSteamImage(friend.GetMediumAvatarAsync().Result.Value));
		playerCard.SetLabels(friend.Name, avatar);
		PlayerListContainer.AddChild(playerCard);
	}

	private void OnPlayerLeftLobbyCallback(Friend friend)
	{
		var player = PlayerListContainer.GetNode(friend.Id.AccountId.ToString());
		PlayerListContainer.RemoveChild(player);
		player.QueueFree();
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
