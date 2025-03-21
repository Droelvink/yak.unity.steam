using com.yak.singleton;
using Steamworks;

namespace com.yak.steam
{
    public class Overlay
    {
        public void Friends() => SteamFriends.ActivateGameOverlay("friends");
        public void Community() => SteamFriends.ActivateGameOverlay("community");
        public void Players() => SteamFriends.ActivateGameOverlay("players");
        public void Settings() => SteamFriends.ActivateGameOverlay("settings");
        public void OfficialGamegroup() => SteamFriends.ActivateGameOverlay("officialgamegroup");
        public void Achievements() => SteamFriends.ActivateGameOverlay("achievements");
        
        public void SteamBrowser(string url, bool openAsModal = false) => SteamFriends.ActivateGameOverlayToWebPage(url, openAsModal ? EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Modal : EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default);
        public void StorePage() => SteamFriends.ActivateGameOverlayToStore(new AppId_t(Steam.AppId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
        
        public void UserProfile(SteamProfile profile) => SteamFriends.ActivateGameOverlayToUser("steamid", profile.Id);
        public void UserChat(SteamProfile profile) => SteamFriends.ActivateGameOverlayToUser("chat", profile.Id);
        public void UserStats(SteamProfile profile) => SteamFriends.ActivateGameOverlayToUser("stats", profile.Id);
        public void UserAchievements(SteamProfile profile) => SteamFriends.ActivateGameOverlayToUser("achievements", profile.Id);
    }
}
