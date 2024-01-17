using Godot;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;

public partial class SceneManager : CanvasLayer
{
    [Export]
    private Node _level;

    [ExportCategory("UI")]
    [Export]
    public Button CreateLobbyButton { get; set; }

    [Export]
    public Button GetallLobbiesButton { get; set; }

    [Export]
    public Button InviteFriendButton { get; set; }

    [Export]
    public Button StartGameButton { get; set; }

    [Export]
    public Button BackButton { get; set; }

    [Export]
    private Button _quitButton;

    [Export]
    public VBoxContainer LobbyListContainer { get; set; }

    [Export]
    public VBoxContainer PlayerListContainer { get; set; }

    [ExportCategory("Menu")]
    [Export]
    private Control _startMenu;

    [Export]
    private Control _lobbyMenu;

    [ExportCategory("PackedScene")]
    [Export]
    public PackedScene LobbyElementScene { get; set; }

    [Export]
    public PackedScene PlayerCardScene { get; set; }

    [Export]
    public PackedScene PlayerScene { get; set; }

    [Export]
    private PackedScene _map1;

    private string _address;

    [Signal]
    public delegate void OnServerClosingEventHandler();

    

    public override void _Ready()
    {
        SteamManager.OnLobbyListRefreshedCompleted += OnLobbyListRefreshedCompletedCallback;
        SteamManager.OnPlayerJoinedLobby += OnPlayerJoinedLobbyCallback;
        SteamManager.OnPlayerLeftLobby += OnPlayerLeftLobbyCallback;

        SteamMatchmaking.OnLobbyEntered += (lobby) => _startMenu.Visible = false;
        SteamMatchmaking.OnLobbyEntered += (lobby) => _lobbyMenu.Visible = true;

        DataParser.OnJoin += JoinServer;

        //UI
        CreateLobbyButton.Pressed += CreateLobbyButtonPressed;
        GetallLobbiesButton.Pressed += GetallLobbiesButtonPressed;
        InviteFriendButton.Pressed += InviteFriendButtonPressed;
        StartGameButton.Pressed += StartGameButtonPressed;
        BackButton.Pressed += BackButtonPressed;
        _quitButton.Pressed += QuitGame;

        Multiplayer.PeerConnected += _playerIDs.Add;
        Multiplayer.PeerConnected += AddPlayer;
        Multiplayer.PeerDisconnected +=PlayerLeaving;

        OnServerClosing += ServerClosing;

    }

    private void ServerClosing()
    {
       GetTree().ReloadCurrentScene();
    }

    private void PlayerLeaving(long id)
    {
        if (Multiplayer.IsServer())
        {
            _playerIDs.Remove(id);
            EmitSignal(SignalName.OnServerClosing);
        }
        else
        {
            GetNodeOrNull<PlayerController>(id.ToString())?.QueueFree();
        }
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

    public async void CreateLobbyButtonPressed()
    {
        await SteamManager.Instance.CreateLobby();
        _address = CreateServer(true);
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
            SteamManager.Instance.SendMessageToAll(OwnJsonParser.Serialize(new Dictionary<string, string>
            {
                { "DataType", "Join" },
                { "Data", _address }
            }));
            GameManager.States = GameManager.GameState.InGame;
            SteamManager.Instance.hostedLobby.SetData("lobbyState", GameManager.States.ToString());
            SteamManager.Instance.hostedLobby.SetJoinable(false);
            StartGame();
        }
    }

    public void BackButtonPressed()
    {
        GD.Print("BackButtonPressed");

        foreach (var item in PlayerListContainer.GetChildren())
        {
            item.QueueFree();
        }

        _startMenu.Visible = true;
        _lobbyMenu.Visible = false;

        _peer.Close();
        SteamManager.Instance.LeaveLobby();
    }

    private void QuitGame()
    {
        GetTree().Quit();
    }

    public void StartGame()
    {
        Visible = false;
        
        _playerIDs.Add(1);
        Node mapNode = _map1.Instantiate();
        _level.AddChild(mapNode);

        AddPlayer();
    }

    private void AddPlayer(long id = 1)
    {
        var player = PlayerScene.Instantiate() as PlayerController;
        player.Name = id.ToString();
        player.FriendData = GameManager.Players[_playerIDs.Count - 1].FriendData;
        _level.GetChild(0).AddChild(player);
    }

    private ENetMultiplayerPeer _peer = new();
    private const int MAX_CONNECTIONS = 6;
    private const int PORT = 7000;
    private const string DEFAULT_SERVER_IP = "127.0.0.1";

    private Godot.Collections.Array<long> _playerIDs = new Godot.Collections.Array<long>();

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