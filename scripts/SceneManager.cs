using Godot;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

public partial class SceneManager : CanvasLayer
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

    [Export]
    private Node _level;

    public override void _Ready()
    {
        SteamManager.OnLobbyListRefreshedCompleted += OnLobbyListRefreshedCompletedCallback;
        CreateLobbyButton.Pressed += CreateLobbyButtonPressed;
        GetallLobbiesButton.Pressed += GetallLobbiesButtonPressed;
        InviteFriendButton.Pressed += InviteFriendButtonPressed;
        SteamManager.OnPlayerJoinedLobby += OnPlayerJoinedLobbyCallback;
        SteamManager.OnPlayerLeftLobby += OnPlayerLeftLobbyCallback;
        DataParser.OnStartGame += StartGame;
        StartGameButton.Pressed += StartGameButtonPressed;
        DataParser.OnJoin += JoinServer;
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

    public void StartGameButtonPressed()
    {
        if (SteamManager.Instance.IsHost)
        {
            Dictionary<string, string> dataToSend = new Dictionary<string, string>
            {
                { "DataType", "StartGame" },
                { "SceneToLoad", "res://main.tscn" }
            };

            StartGame(dataToSend);
        }
    }

    public void StartGame(Dictionary<string, string> data)
    {
        string address = CreateServer(true);

        Dictionary<string, string> dataToSend = new Dictionary<string, string>
        {
            { "DataType", "Join" },
            { "Data", address}
        };

        PackedScene map = GD.Load<PackedScene>(data["SceneToLoad"]);
        Node mapNode = map.Instantiate();
        _level.AddChild(mapNode);

        AddPlayer();

        SteamManager.Instance.SendMessageToAll(OwnJsonParser.Serialize(dataToSend));
        Visible = false;
    }

    private void AddPlayer(int id = 1)
    {
        var player = PlayerMovement.Instantiate() as PlayerMovement;
        player.Name = id.ToString();
        player.FriendData = GameManager.Players[SteamManager.Instance.PlayerId.AccountId].FriendData;
        _level.GetChild(0).AddChild(player);
        player.GlobalPosition += new Vector3(0, 10, 0);
    }

    private ENetMultiplayerPeer _peer = new();
    private const int MAX_CONNECTIONS = 6;
    private const int PORT = 7000;
    private const string DEFAULT_SERVER_IP = "127.0.0.1";

    private string SetupUPNP()
    {
        Upnp upnp = new ();

        int error = upnp.Discover();
        Debug.Assert(error == (int)Upnp.UpnpResult.Success, "UPNP Discover Failed! Error %s" + error);

        Debug.Assert(upnp.GetGateway() != null && upnp.GetGateway().IsValidGateway(), "UPNP Invalid Gateway!");

        int mapResult = upnp.AddPortMapping(PORT);

        Debug.Assert(mapResult == (int)Upnp.UpnpResult.Success, "UPNP AddPortMapping Failed! Error %s" + mapResult);

        Debug.Print("Success! Join Address : " + upnp.GetGateway().QueryExternalAddress() + ":" + PORT);

        return upnp.GetGateway().QueryExternalAddress();
    }

    private string CreateServer(bool online)
    {
        Error error = _peer.CreateServer(PORT, MAX_CONNECTIONS);

        if (error != Error.Ok)
            return "";

        Multiplayer.MultiplayerPeer = _peer;

        string address = DEFAULT_SERVER_IP;
        if (online)
        {
            address = SetupUPNP();
        }

        return address;
    }

    private void JoinServer(Dictionary<string, string> data)
    {
        string address = data["Data"];

        if (address == "")
            address = DEFAULT_SERVER_IP;
        
        Error error = _peer.CreateClient(address , PORT);

        if (error != Error.Ok)
            return;
        
        Multiplayer.MultiplayerPeer = _peer;

        Visible = false;

        return;
    }
}