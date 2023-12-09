using Godot;
using System;




public partial class MultiplayerManager : Node
{
	public static MultiplayerManager Instance { get; private set; }

	#region Signals
	[Signal]
	public delegate void OnPlayerConnectedEventHandler(double id);

	[Signal]
	public delegate void OnPlayerDisconnectedEventHandler(double id);

	[Signal]
	public delegate void OnServerConnectedEventHandler();

	[Signal]
	public delegate void OnServerDisconnectedEventHandler();

	[Signal]
	public delegate void OnUpnpCompleteEventHandler(Error error);

	#endregion Signals

	#region Constants
	private const string DEFAULT_IP = "127.0.0.1";
	private const int DEFAULT_PORT = 7000;
	private const int DEFAULT_MAX_PLAYERS = 5;

	#endregion Constants

	#region VariablesPrivate
	
	private ENetMultiplayerPeer _peer;
	private Upnp _upnp;

	#endregion VariablesPrivate






	public override void _EnterTree()
	{
		Instance = this;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
