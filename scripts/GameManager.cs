using Steamworks;
using System.Collections.Generic;

public class GameManager
{
    public static List<PlayerMovement> Players = new List<PlayerMovement>();

    public static string PlayerInstanceName = "Player";

    public static void OnPlayerJoinedCallback(Friend friend)
    {
        PlayerMovement player = new PlayerMovement();
        player.FriendData = friend;
        Players.Add(player);
    }
}