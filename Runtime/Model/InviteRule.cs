using Steamworks;

namespace com.yak.steam
{
    public static class InviteRule
    {
        public enum Rule
        {
            Public,
            FriendsOnly,
            Private
        }
        
        public static ELobbyType InviteRuleToLobbyType(Rule inviteRule)
        {
            return inviteRule switch
            {
                Rule.Public => ELobbyType.k_ELobbyTypePublic,
                Rule.FriendsOnly => ELobbyType.k_ELobbyTypeFriendsOnly,
                Rule.Private => ELobbyType.k_ELobbyTypePrivate,
                _ => ELobbyType.k_ELobbyTypePrivate
            };
        }
    }
}
