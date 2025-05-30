using com.yak.singleton;
using UnityEngine.Events;

namespace com.yak.steam
{
    public class Steam : Singleton<Steam>
    {
        // Fetch from a config file
        public static uint AppId { get; private set; } = SteamSettings.Instance.AppId;

        public static bool Initialized { get; private set; } = false;
        public static Profiles Profiles { get; private set; }  = new Profiles();
        public static Overlay Overlay { get; private set; } = new Overlay();
        public static Lobby Lobby { get; private set; } = new Lobby();
        public static LobbyBrowser LobbyBrowser { get; private set; } = new LobbyBrowser();
        public static UnityEvent OnInitialized { get; private set; } = new UnityEvent();
    }
}