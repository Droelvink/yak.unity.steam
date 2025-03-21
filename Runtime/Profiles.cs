using System.Collections.Generic;
using com.yak.singleton;
using Steamworks;

namespace com.yak.steam
{
    public class Profiles
    {
        private List<SteamProfile> _friends;
        private SteamProfile _self;
        
        public SteamProfile Self() => _self ??= new SteamProfile(SteamUser.GetSteamID());
        public List<SteamProfile> Friends()
        {
            if (_friends != null) return _friends;
            RefreshFriendslist();
            return _friends;
        }
        
        public SteamProfile Get(CSteamID steamID) => new(steamID);

        public List<SteamProfile> FromLobby(CSteamID lobbyID)
        {
            var profiles = new List<SteamProfile>();
            var fc = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
            for(var i = 0; i< fc; i++) profiles.Add(Get(SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i)));
            return profiles;
        }
        
        public void RefreshFriendslist()
        {
            _friends = new List<SteamProfile>();
            var fc = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            for (var i = 0; i < fc; i++) _friends.Add(Get(SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate)));
        }
    }
}
