using Steamworks;
using System.Collections.Generic;
using System.Linq;

public class GameManager
{
    public static List<PlayerController> Players = new List<PlayerController>();

    public static string PlayerInstanceName = "Player";

    public static SceneManager SceneManager { get; set; }

    public enum GameState
    {
        Lobby,
        InGame
    }

    public static GameState States = GameState.Lobby; //TODO change name

    public static void OnPlayerJoinedCallback(Friend friend)
    {
        PlayerController player = new PlayerController();
        player.FriendData = friend;
        Players.Add(player);
    }

    public static void OnPlayerReady(Dictionary<string, string> data)
    {
       var players = Players;
       PlayerController player = players.Where(x => x.FriendData.Id.AccountId.ToString() == data["PlayerName"]).FirstOrDefault();
       player.IsReady = bool.Parse(data["IsReady"]);

       if (SteamManager.Instance.IsHost)
       {
            SteamManager.Instance.SendMessageToAll(OwnJsonParser.Serialize(data));
            if (players.Count(x => x.IsReady) == players.Count)
            {
                Dictionary<string, string> startGame = new Dictionary<string, string>
                {
                    { "DataType", "StartGame" }
                };
                SteamManager.Instance.SendMessageToAll(OwnJsonParser.Serialize(startGame));
                SceneManager.StartGameCallback(startGame);
            }
       }
    }
}