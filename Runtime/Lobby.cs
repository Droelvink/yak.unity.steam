using System;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace com.yak.steam
{
    public class Lobby : IDisposable
    {
        public CSteamID ID { get; private set; }
        private Dictionary<string, string> _lobbyData = new();
        
        private Callback<LobbyCreated_t> _lobbyCreatedCallback;
        private Callback<LobbyEnter_t> _lobbyEnterCallback;
        private Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;
        private Callback<LobbyChatMsg_t> _lobbyChatMsgCallback;

        private readonly byte[] _chatBuffer = new byte[2048];
        
        public UnityEvent<LobbyData> LobbyDataUpdatedEvent = new ();
        public UnityEvent<ChatMessageData> ChatMessageReceivedEvent = new ();

        
        // public SteamLobby Current;
        //
        // private const string LobbyMemberDataKey = "__lobbymemberdata";
        // private const string LobbyDataKey = "__lobbydata";
        // private const string CmdKey = "__CMD__";
        // private const char CmdSplitKey = ':';
        // private const char ArgSplitKey = ';';
        //
        // private Callback<LobbyCreated_t> _lobbyCreatedCallback;
        // private Callback<LobbyEnter_t> _lobbyEnterCallback;
        // private Callback<LobbyChatMsg_t> _onLobbyChatMessage;
        //
        // public UnityEvent<CSteamID, string> OnChatMessageReceived { get; } = new();
        // public UnityEvent<CSteamID, string, string[]> OnCommandReceived { get; } = new();
        // public UnityEvent<CSteamID, string, string> OnLobbyMemberDataUpdated { get; } = new();
        // public UnityEvent<string, string> OnLobbyDataUpdated { get; } = new();
        //
        // private readonly byte[] _chatBuffer = new byte[2048];

        public Lobby()
        {
            _lobbyChatMsgCallback = Callback<LobbyChatMsg_t>.Create(OnChatMessageReceivedCallback);
            _lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdateCallback);
        }
        
        public bool InLobby() => ID != CSteamID.Nil;
        
        public void Create(string lobbyName, int maxConnections, InviteRule.Rule inviteRule, Action<bool> onComplete)
        {
            if (InLobby())
            {
                Debug.LogWarning("[com.yak.steam] Tried to create a lobby while already in a lobby");
                return;
            }
            
            _lobbyCreatedCallback = Callback<LobbyCreated_t>.Create((@event) =>
            {
                _lobbyCreatedCallback?.Dispose();
                if (@event.m_eResult != EResult.k_EResultOK) onComplete(false);
                else
                {
                    var steamID = new CSteamID(@event.m_ulSteamIDLobby);
                    ID = steamID;
                    SetData("lobbyName", lobbyName);
                    SetData("maxPlayers", maxConnections.ToString());
                    SetData("yaknet_app", Steam.AppId.ToString()); // this is here for testing
                    SetData("owner", Steam.Profiles.Self().Id.ToString());
                }
            });
            _lobbyEnterCallback = Callback<LobbyEnter_t>.Create((@event) =>
            {
                _lobbyEnterCallback?.Dispose();
                var success = @event.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess;
                if (!success) ID = CSteamID.Nil;
                onComplete?.Invoke(success);
            });
            SteamMatchmaking.CreateLobby(InviteRule.InviteRuleToLobbyType(inviteRule), maxConnections);
        }
        
        public void Leave()
        {
            if (!InLobby())
            {
                Debug.LogWarning("[com.yak.steam] Tried to leave a lobby but no lobby is currently active");
                return;
            }
            SteamMatchmaking.LeaveLobby(ID);
            ID = CSteamID.Nil;
        }
        
        public void Join(CSteamID lobbyID, Action<bool> onComplete)
        {
            if (InLobby())
            {
                Debug.LogWarning("[com.yak.steam] Tried to join a lobby while already in a lobby");
                return;
            }
            
            _lobbyEnterCallback = Callback<LobbyEnter_t>.Create((@event) =>
            {
                _lobbyEnterCallback?.Dispose();
                ID = lobbyID;
                onComplete?.Invoke(@event.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess);
            });
            SteamMatchmaking.JoinLobby(lobbyID);
        }
        
        public void SendChatMessage(string message)
        {
            if (!InLobby())
            {
                Debug.LogWarning("[com.yak.steam] Tried to send a chat message but no lobby is currently active");
                return;
            }
            var msgBytes = Encoding.UTF8.GetBytes(message);
            SteamMatchmaking.SendLobbyChatMsg(ID, msgBytes, msgBytes.Length);
        }

        private void OnLobbyDataUpdateCallback(LobbyDataUpdate_t @event)
        {
            var datacount = SteamMatchmaking.GetLobbyDataCount(ID);
            for (var i = 0; i < datacount; i++)
            {
                if (!SteamMatchmaking.GetLobbyDataByIndex(ID, i, out var key, 255, out var value, 255)) continue;
                Debug.Log($"{key} - {value}");
                if (_lobbyData.ContainsKey(key) && _lobbyData[key] != value) LobbyDataUpdatedEvent?.Invoke(new LobbyData {Key = key, Value = value});
                _lobbyData[key] = value;
            }
        }


        private void OnChatMessageReceivedCallback(LobbyChatMsg_t @event)
        {
            if (!InLobby() || @event.m_ulSteamIDLobby != ID.m_SteamID)
            {
                Debug.LogWarning($"[com.yak.steam] Received chat message but no lobby is currently {(ID == CSteamID.Nil ? "inactive" :"invalid")}");
                return;
            }
            var msglen = SteamMatchmaking.GetLobbyChatEntry(ID, (int)@event.m_iChatID, out var userID, _chatBuffer, _chatBuffer.Length, out var chatEntryType);
            var msg = Encoding.UTF8.GetString(_chatBuffer, 0, msglen);
            ChatMessageReceivedEvent?.Invoke(new ChatMessageData {Owner = userID,Message = msg});
        }
        
        public void SetData(string key, string value) //, bool notify = false
        {
            if (!InLobby())
            {
                Debug.LogWarning("[com.yak.steam] Tried to set lobby data but no lobby is currently active");
                return;
            }
            SteamMatchmaking.SetLobbyData(ID, key, value);
            // SendChatMessage($"{DataUpdatePrefix}:0:{key}:{value}");
            // if (notify) SendCommand(LobbyDataKey, key, value);
        }
        
        public void SetMemberData(string key, string value) //, bool notify = false
        {
            if (!InLobby())
            {
                Debug.LogWarning("[com.yak.steam] Tried to set lobby member data but no lobby is currently active");
                return;
            }
            SteamMatchmaking.SetLobbyMemberData(ID, key, value);
            // SendChatMessage($"{DataUpdatePrefix}:1:{Steam.Profiles.Self().Id.m_SteamID}:{key}:{value}");
            // if (notify) SendCommand(LobbyMemberDataKey, Steam.Profiles.Self().Id.ToString(), key, value);
        }
        
        public void Dispose()
        {
            if(InLobby()) Leave();
            _lobbyCreatedCallback?.Dispose();
            _lobbyEnterCallback?.Dispose();
            _lobbyChatMsgCallback?.Dispose();
        }
        
        public struct ChatMessageData
        {
            public CSteamID Owner;
            public string Message;
        }

        public struct LobbyData
        {
            public string Key;
            public string Value;
        }
        
        
        //
        // public void SendChatMessage(string message)
        // {
        //     if (Current == null)
        //     {
        //         Debug.LogWarning("[com.yak.steam] Tried to send a chat message but no lobby is currently active");
        //         return;
        //     }
        //     var msgBytes = Encoding.UTF8.GetBytes(message);
        //     SteamMatchmaking.SendLobbyChatMsg(Current.Id, msgBytes, msgBytes.Length);
        // }
        //
        // public void SendCommand(string command, params string[] args)
        // {
        //     if (Current == null)
        //     {
        //         Debug.LogWarning("[com.yak.steam] Tried to send a command but no lobby is currently active");
        //         return;
        //     }
        //     var sb = new StringBuilder().Append(CmdKey).Append(command);
        //     if(args.Length > 0) sb.Append(CmdSplitKey).Append(string.Join(ArgSplitKey, args));
        //     var cmdBytes = Encoding.UTF8.GetBytes(sb.ToString());
        //     SteamMatchmaking.SendLobbyChatMsg(Current.Id, cmdBytes, cmdBytes.Length);
        // }
        //
        
        //
        // private void OnChatMessageReceivedCallback(LobbyChatMsg_t @event)
        // {
        //     if (Current == null)
        //     {
        //         Debug.LogWarning("[com.yak.steam] Received chat message but there is no active lobby");
        //         return;
        //     }
        //     if (@event.m_ulSteamIDLobby != Current.Id.m_SteamID)
        //     {
        //         Debug.LogWarning("[com.yak.steam] Received message from an unknown server");
        //         return;
        //     }
        //
        //     var msglen = SteamMatchmaking.GetLobbyChatEntry(Current.Id, (int)@event.m_iChatID, out var userID, _chatBuffer,
        //         _chatBuffer.Length, out var chatEntryType);
        //     var msg = Encoding.UTF8.GetString(_chatBuffer, 0, msglen);
        //     if (msg.StartsWith(CmdKey))
        //     {
        //         var split = msg.Trim().Split(CmdSplitKey);
        //         var cmd = split[0][CmdKey.Length..];
        //         var args = split.Length <= 1 ? Array.Empty<string>() : split[1].Trim().Split(ArgSplitKey);
        //         switch (cmd)
        //         {
        //             case LobbyDataKey:
        //                 OnLobbyDataUpdated?.Invoke(args[0], args[1]);
        //                 break;
        //             case LobbyMemberDataKey:
        //                 OnLobbyMemberDataUpdated?.Invoke(new CSteamID(ulong.Parse(args[0])), args[1], args[2]);
        //                 break;
        //             default:
        //                 OnCommandReceived?.Invoke(userID, cmd, args);
        //                 break;
        //         }
        //     } else OnChatMessageReceived?.Invoke(userID, msg);
        // }
    }


}
