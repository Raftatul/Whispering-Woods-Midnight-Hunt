using Godot;
using System;
using Steamworks;

public partial class SteamworksHelper : Node
{
  public static SteamworksHelper Instance { get; private set; }

  protected Callback<GameOverlayActivated_t> gameOverlayActivated; 

  private CallResult<NumberOfCurrentPlayers_t> numberOfCurrentPlayers;

  private SteamAPIWarningMessageHook_t steamAPIWarningMessageHook;

  private static uint appId = 480;

  private bool isSteamInitialized = false;

  public bool IsSteamInitialized
  {
    get { return Instance.isSteamInitialized; }
  }

  //get steam name of current user
  public string GetSteamName()
  {
    if (!isSteamInitialized) return "Steamworks not initialized";
    return SteamFriends.GetPersonaName();
    
  }


#region Steam Callbacks
  private void OnGameOverlayActivated(GameOverlayActivated_t pCallback) 
  {
    if (pCallback.m_bActive != 0) 
    {
      GD.Print("Steam Overlay has been activated");
    }
    else 
    {
      GD.Print("Steam Overlay has been closed");
    }
  }

  private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure) 
  {
    if (pCallback.m_bSuccess != 1 || bIOFailure) 
    {
      GD.Print("There was an error retrieving the NumberOfCurrentPlayers.");
    }
    else 
    {
      GD.Print("The number of players playing your game: " + pCallback.m_cPlayers);
    }
  }

  private void OnSteamAPIWarningMessageHook(int nSeverity, System.Text.StringBuilder pchDebugText) 
  {
    GD.Print(pchDebugText);
  }

#endregion Steam Callbacks

#region Godot Methods
    public override void _EnterTree() 
  {
    Instance = this;
    try
    {
      if (SteamAPI.RestartAppIfNecessary((AppId_t)appId)) 
      {
        GD.Print("Steamworks: restarting app...");
        GetTree().Quit();
        return;
      }
    }
    catch (System.DllNotFoundException e)
    {
      GD.PushError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e,this);
      GetTree().Quit();
    }
    isSteamInitialized = SteamAPI.Init();
    if (!isSteamInitialized) 
    {
      GD.Print("Steamworks: SteamAPI.Init() failed!");
      return;
    }
    if (steamAPIWarningMessageHook == null) 
    {
      steamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(OnSteamAPIWarningMessageHook);
      SteamClient.SetWarningMessageHook(steamAPIWarningMessageHook);
    }
  }

  public override void _Ready() 
  {
    if (Instance != this) 
    {
      GD.Print("Steamworks: There is more than one SteamworksHelper in the scene!");
      return;
    }
    if (!Packsize.Test()) 
    {
      GD.Print("Steamworks: Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
    }
    if (!DllCheck.Test()) 
    {
      GD.Print("Steamworks: DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
    }
    if (SteamAPI.Init()) 
    {
      gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
      numberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
    }
  
  }

    public override void _PhysicsProcess(double delta)
    {
      if (!isSteamInitialized) return;



      if (Input.IsKeyPressed(Key.Shift) && Input.IsKeyPressed(Key.Tab))
      {
        GD.Print("Steamworks: checking if Steam Overlay is enabled...");
        bool bIsOverlayEnabled = SteamUtils.IsOverlayEnabled();
        GD.Print("Steamworks: Steam Overlay is " + (bIsOverlayEnabled ? "enabled" : "disabled"));
      }

      if (Input.IsKeyPressed(Key.Space))
      {
        SteamAPICall_t handle = SteamUserStats.GetNumberOfCurrentPlayers();
        numberOfCurrentPlayers.Set(handle);
        GD.Print("Steamworks: checking how many players are playing our game...");
      }

      SteamAPI.RunCallbacks();
    }

    public override void _ExitTree() 
  {
    // Tell Steam we're done.
    GD.Print("Steamworks: shutting down...");
    try {
      SteamAPI.Shutdown();
      GD.Print("Steamworks shutdown succeeded!");
    }
    catch (Exception e) {
      GD.Print("Steamworks shutdown threw an exception :O");
      GD.Print(e);
    }
  }
#endregion Godot Methods
}


