using Godot;
using Steamworks.Data;
using System;
using System.Collections.Generic;

public partial class ChatBox : Control
{
	[Export]
	public LineEdit ChatInput { get; set; }

	[Export]
	public Button SendButton { get; set; }

	[Export]
	public RichTextLabel ChatHistory { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DataParser.OnChatMessageReceived += OnChatMessageCallback;
		SendButton.Pressed += OnSendButtonPressed;
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnSendButtonPressed()
	{
		if (ChatInput.Text.Length > 0)
		{
			Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "DataType", "ChatMessage" },
                { "UserName", SteamManager.Instance.PlayerName },
                { "ChatMessage", ChatInput.Text }
            };
			OnChatMessageCallback(data);
			string json = OwnJsonParser.Serialize(data);
			if (SteamManager.Instance.IsHost)
			{
				SteamManager.Instance.SendMessageToAll(json);
			}
			else
			{
				SteamManager.Instance.SteamConnectionManager.Connection.SendMessage(json);
			}
		}
	}

	private void OnChatMessageCallback(Dictionary<string, string> data)
	{
		ChatHistory.Text += data["UserName"] + ": " + data["ChatMessage"] + System.Environment.NewLine;
	}
}
