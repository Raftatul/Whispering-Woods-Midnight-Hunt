using Godot;
using System;
using Steamworks;
using Steamworks.Data;

public partial class SteamworksHelper : Node
{
  public static SteamworksHelper Instance { get; private set; }

  public static uint AppId { get; private set; } = 480;
  public string PlayerName { get; set; }
  public SteamId PlayerId { get; set; }
  public bool connectedToSteam { get; set; }
  public SteamworksHelper()
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
    GD.Print($"Lobby created with id {lobby.Id}");
  }

  private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
  {
    GD.Print("User joined lobby " + friend.Name);
  }

  private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
  {
    GD.Print("User disconnected from lobby " + friend.Name);
  }

  private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
  {
    GD.Print("User left lobby " + friend.Name);
  }
  #endregion Steam Callbacks



  #region Godot Methods

  public override void _Ready()
  {
    SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
    SteamMatchmaking.OnLobbyCreated += OnLobbyCreated; 
    SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
    SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
    SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
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


