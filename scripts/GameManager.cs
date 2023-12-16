using Steamworks;
using System.Collections.Generic;

public class GameManager
{
    public static Dictionary<uint, PlayerMovement> Players = new Dictionary<uint, PlayerMovement>();

    public static void OnPlayerJoinedCallback(Friend friend)
    {
        PlayerMovement player = new PlayerMovement();
        player.FriendData = friend;
        Players.Add(friend.Id.AccountId, player);
    }
}