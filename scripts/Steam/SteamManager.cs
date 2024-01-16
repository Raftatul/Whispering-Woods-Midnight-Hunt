using Godot;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class SteamManager : Node
{
    public static SteamManager Instance { get; private set; }

    public static uint AppId { get; private set; } = 480;
    public string PlayerName { get; set; }
    public SteamId PlayerId { get; set; }
    public bool connectedToSteam { get; set; }

    [Export]
    public int MaxlobbyMembers { get; set; } = 5;

    public Lobby hostedLobby { get; set; }

    private List<Lobby> availableLobbies { get; set; } = new List<Lobby>();

    public static event Action<List<Lobby>> OnLobbyListRefreshedCompleted;

    public static event Action<Friend> OnPlayerJoinedLobby;

    public static event Action<Friend> OnPlayerLeftLobby;

    public SteamSocketManager SocketManager;
    public SteamConnectionManager SteamConnectionManager;

    public bool IsHost;

    public SteamManager()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        try
        {
            SteamClient.Init(AppId, true);

            if (!SteamClient.IsValid)
            {
                GD.PrintErr("Steamworks not initialized");
                throw new Exception("Steamworks not initialized");
            }
            PlayerName = SteamClient.Name;
            PlayerId = SteamClient.SteamId;
            connectedToSteam = true;
            GD.Print("Steamworks initialized! Player name: " + PlayerName + " Player ID: " + PlayerId);
        }
        catch (Exception e)
        {
            GD.PrintErr("Steamworks not initialized" + e.Message);
            connectedToSteam = false;
        }
    }

    #region Steam Callbacks

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
    {
        GD.Print("Firing callback for on lobby game created with ip " + ip + " port " + port + " steamId " + steamId);
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            GD.PrintErr("Failed to create lobby");
            return;
        }
        GD.Print("Lobby created !");

        CreateSteamSocketServer();
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        GD.Print("User joined lobby " + friend.Name);
        OnPlayerJoinedLobby(friend);
    }

    private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
    {
        GD.Print("User disconnected from lobby " + friend.Name);
        OnPlayerLeftLobby(friend);
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        GD.Print("User left lobby " + friend.Name);
        OnPlayerLeftLobby(friend);
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (lobby.GetData("lobbyState") != GameManager.GameState.Lobby.ToString())
        {
            GD.PrintErr("Lobby is not in lobby state");
            return;
        }
        if (lobby.MemberCount > 0)
        {
            GD.Print($"You have entered {lobby.Owner.Name}'s lobby");
            hostedLobby = lobby;
            foreach (Friend friend in lobby.Members)
            {
                OnPlayerJoinedLobby(friend);
            }
            hostedLobby.SetGameServer(lobby.Owner.Id);
        }
        else
        {
            GD.Print($"You have joined your own lobby");
        }
        JoinSteamSocketServer(lobby.Owner.Id);
    }

    #endregion Steam Callbacks

    public async Task<bool> CreateLobby()
    {
        try
        {
            GD.Print("Creating lobby");
            Lobby? CreateLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(MaxlobbyMembers);

            if (!CreateLobbyOutput.HasValue)
            {
                GD.PrintErr("lobby created but did not return a value");
                throw new Exception();
            }

            hostedLobby = CreateLobbyOutput.Value;
            hostedLobby.SetPublic();
            hostedLobby.SetJoinable(true);
            hostedLobby.SetData("ownerNameDataString", PlayerName); //equivalent du dictionnaire des player id / dico infos
            hostedLobby.SetData("lobbyState", GameManager.GameState.Lobby.ToString());

            GD.Print("Lobby created with id " + hostedLobby.Id);
            return true;
        }
        catch (System.Exception e)
        {
            GD.PrintErr("Failed to create lobby " + e.Message);
            return false;
        }
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamIDFriend)
    {
        if (lobby.GetData("lobbyState") != GameManager.GameState.Lobby.ToString())
        {
            GD.PrintErr("Lobby is not in lobby state");
            return;
        }
        
        RoomEnter joinSuccessful = await lobby.Join();
        if (joinSuccessful != RoomEnter.Success)
        {
            GD.PrintErr("Failed to join lobby");
        }
        else
        {
            hostedLobby = lobby;
            foreach (Friend friend in lobby.Members)
            {
               if (friend.Id != PlayerId)
                {
                    OnPlayerJoinedLobby(friend);
                }
            }
        }
    }

    public void CreateSteamSocketServer()
    {
        SocketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
        SteamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(PlayerId, 0);
        IsHost = true;
        GD.Print("Created socket server");
    }

    public void JoinSteamSocketServer(SteamId hostId)
    {
        if (!IsHost)
        {
            GD.Print("Joining socket server");
            SteamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(hostId, 0);
        }
    }

    public static Godot.Image GetImageFromSteamImage(Steamworks.Data.Image steamImage)
    {
        return Godot.Image.CreateFromData((int)steamImage.Width, (int)steamImage.Height, false, Godot.Image.Format.Rgba8, steamImage.Data);
    }

    public void OpenFriendInviteOverlay()
    {
        SteamFriends.OpenGameInviteOverlay(hostedLobby.Id);
    }

    public async Task<bool> GetMultiplayerLobbyList()
    {
        try
        {
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithKeyValue("lobbyState", GameManager.GameState.Lobby.ToString()).WithSlotsAvailable(1).RequestAsync();
            availableLobbies.Clear();
            if (lobbies != null)
            {
                foreach (Lobby lobby in lobbies)
                {
                    GD.Print("Lobby found with id " + lobby.Id);
                    availableLobbies.Add(lobby);
                }
            }
            OnLobbyListRefreshedCompleted.Invoke(availableLobbies);
            return true;
        }
        catch (System.Exception e)
        {
            GD.PrintErr("Failed to get lobby list " + e.Message);
            return false;
        }
    }

    public void SendMessageToAll(string message)
    {
        foreach (var socket in SocketManager.Connected.Skip(1).ToArray()) //skip 1 car le premier est le serveur
        {
            socket.SendMessage(message);
        }
    }

    #region Godot Methods

    public override void _Ready()
    {
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    public override void _Process(double delta)
    {
        SteamClient.RunCallbacks();

        try
        {
            if (SocketManager != null)
            {
                SocketManager.Receive();
            }
            if (SteamConnectionManager != null && SteamConnectionManager.Connected)
            {
                SteamConnectionManager.Receive();
            }
        }
        catch (System.Exception e)
        {
            GD.PrintErr("Failed to receive socket message: " + e.Message);
            throw;
        }
    }

    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what == NotificationWMCloseRequest)
        {
            SteamClient.Shutdown();
            GetTree().Quit();
        }
    }

    public override void _ExitTree()
    {
        SteamClient.Shutdown();
    }

    internal void LeaveLobby()
    {     
        hostedLobby.Leave();
    }


    #endregion Godot Methods
}