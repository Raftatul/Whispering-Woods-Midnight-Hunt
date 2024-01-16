using Steamworks;
using System.Collections.Generic;

public class GameManager
{
    public static List<PlayerController> Players = new List<PlayerController>();

    public static string PlayerInstanceName = "Player";

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
}