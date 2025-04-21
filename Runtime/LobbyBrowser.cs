using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine.Events;

namespace com.yak.steam
{
    public class LobbyBrowser : IDisposable
    {
        private readonly Callback<LobbyMatchList_t> _lobbyListReceivedEvent;

        public UnityEvent<List<LobbyInfo>> OnLobbyListUpdated = new ();
        public void RequestLobbyList() => SteamMatchmaking.RequestLobbyList();

        public LobbyBrowser()
        {
            _lobbyListReceivedEvent = new Callback<LobbyMatchList_t>(OnLobbyListReceived);
        }
        
        public void Dispose()
        {
            _lobbyListReceivedEvent?.Dispose();
        }

        private void OnLobbyListReceived(LobbyMatchList_t @event)
        {
            var lobbyIds = new List<LobbyInfo>();
            for (var i = 0; i < @event.m_nLobbiesMatching; i++)
            {
                var id = SteamMatchmaking.GetLobbyByIndex(i);
                if (SteamMatchmaking.GetLobbyData(id, "yaknet_app") != Steam.AppId.ToString()) continue;
                var owner = new CSteamID(ulong.Parse(SteamMatchmaking.GetLobbyData(id, "owner")));
                var lobbyName = SteamMatchmaking.GetLobbyData(id, "lobbyName");
                var currentPlayers = SteamMatchmaking.GetNumLobbyMembers(id);
                var maxPlayers = int.Parse(SteamMatchmaking.GetLobbyData(id, "maxPlayers"));
                lobbyIds.Add(new LobbyInfo(id, owner, lobbyName, currentPlayers, maxPlayers));
            }
            OnLobbyListUpdated?.Invoke(lobbyIds);
        }
     }
 }
