using Godot;
using System.Collections.Generic;

public partial class ChatBox : Control
{
    [Export]
    public LineEdit ChatInput { get; set; }

    [Export]
    public Button SendButton { get; set; }

    [Export]
    public RichTextLabel ChatHistory { get; set; }

    public override void _Ready()
    {
        DataParser.OnChatMessageReceived += OnChatMessageCallback;
        SendButton.Pressed += OnSendButtonPressed;

        ChatHistory.ScrollFollowing = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_text_completion_accept"))
        {
            OnSendButtonPressed();
        }
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
                ChatInput.Text = "";
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