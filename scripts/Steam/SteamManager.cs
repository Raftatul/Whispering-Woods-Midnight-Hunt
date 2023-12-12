using Godot;
using System;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

public partial class SteamManager : Node
{
  public static SteamManager Instance { get; private set; }

  public static uint AppId { get; private set; } = 480;
  public string PlayerName { get; set; }
  public SteamId PlayerId { get; set; }
  public bool connectedToSteam { get; set; }

  [Export]
  public int MaxlobbyMembers { get; set; } = 5;

  private Lobby hostedLobby { get; set; }

  private List<Lobby> availableLobbies { get; set; } = new List<Lobby>();

  public static event Action<List<Lobby>> OnLobbyListRefreshedCompleted;

  public static event Action<Friend> OnPlayerJoinedLobby;

  public static event Action<Friend> OnPlayerLeftLobby;
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
  }

  private async void OnLobbyMemberJoined(Lobby lobby, Friend friend)
  {
    GD.Print("User joined lobby " + friend.Name);
    OnPlayerJoinedLobby(friend);  
  }

  public static Godot.Image GetImageFromSteamImage(Steamworks.Data.Image steamImage)
  {
    return Godot.Image.CreateFromData((int)steamImage.Width, (int)steamImage.Height, false, Godot.Image.Format.Rgba8, steamImage.Data);
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
    if (lobby.MemberCount > 0 )
    {
      GD.Print($"You have entered {lobby.Owner.Name}'s lobby");
      hostedLobby = lobby;
    }
    else
    {
      GD.Print($"You have joined your own lobby");
    }
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
  RoomEnter joinSuccessful = await lobby.Join();
  if (joinSuccessful != RoomEnter.Success)
  {
    GD.PrintErr("Failed to join lobby");
  }
  else
  {
    hostedLobby = lobby;
  }
}

public void OpenFriendInviteOverlay()
{
  SteamFriends.OpenGameInviteOverlay(hostedLobby.Id);
}

public async Task<bool> GetMultiplayerLobbyList()
{
  try
  {
    Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();
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

    #endregion Godot Methods
}


