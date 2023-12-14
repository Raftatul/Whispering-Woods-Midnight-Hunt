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
	public Button StartGameButton { get; set; }


	[Export]
	public PackedScene LobbyElementScene { get; set; }

	[Export]
	public PackedScene PlayerCardScene { get; set; }

	[Export]
	public VBoxContainer LobbyListContainer { get; set; }

	[Export]
	public VBoxContainer PlayerListContainer { get; set; }

	[Export]
	public PackedScene PlayerMovement { get; set; }

	public override void _Ready()
	{
		SteamManager.OnLobbyListRefreshedCompleted += OnLobbyListRefreshedCompletedCallback;
		CreateLobbyButton.Pressed += CreateLobbyButtonPressed;
		GetallLobbiesButton.Pressed += GetallLobbiesButtonPressed;
		InviteFriendButton.Pressed += InviteFriendButtonPressed;
		SteamManager.OnPlayerJoinedLobby += OnPlayerJoinedLobbyCallback;
		SteamManager.OnPlayerLeftLobby += OnPlayerLeftLobbyCallback;
		DataParser.OnStartGame += StartGame;
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
		GameManager.OnPlayerJoinedCallback(friend);
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

	public void StartGameButtonPressed(Dictionary<string, string> data)
	{
		if (SteamManager.Instance.IsHost)
		{

		Dictionary<string, string> dataToSend = new Dictionary<string, string>
		{
			{ "DataType", "StartGame" },
			{ "SceneToLoad", "res://main.tscn" }
		};
		SteamManager.Instance.SendMessageToAll(OwnJsonParser.Serialize(dataToSend));
		StartGame(dataToSend);
		}

	}

	public void StartGame(Dictionary<string, string> data)
	{
		GetTree().ChangeSceneToFile(data["SceneToLoad"]);
		foreach (var item in GameManager.Players)
		{
			var player = PlayerMovement.Instantiate() as PlayerMovement;
			player.Name = item.FriendData.Id.AccountId.ToString();
			player.FriendData = item.FriendData;
			if(player.Name == SteamManager.Instance.PlayerId.AccountId.ToString())
			{
				player.ControlledByPlayer = true;
			}

		}
	}
}
