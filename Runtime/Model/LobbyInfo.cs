using Steamworks;
using UnityEngine;

namespace com.yak.steam
{
    public class LobbyInfo
    {
        public CSteamID LobbyID { get; private set; }
        public CSteamID Owner { get; private set; }
        public string LobbyName { get; private set; }
        public int CurrentPlayers { get; private set; }
        public int MaxPlayers { get; private set; }

        public LobbyInfo(CSteamID lobbyID, CSteamID owner, string lobbyName, int currentPlayers, int maxPlayers)
        {
            LobbyID = lobbyID;
            Owner = owner;
            LobbyName = lobbyName;
            CurrentPlayers = currentPlayers;
            MaxPlayers = maxPlayers;
        }
    }
}
