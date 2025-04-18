using UnityEditor;
using UnityEngine;

namespace com.yak.steam
{
    public class SteamSettings : singleton.ScriptableSingleton<SteamSettings>
    {
        [field: Header("State")]
        [field: SerializeField] public bool SteamEnabled { get; private set; } = true;
        
        [field: Header("Settings")]
        [field: SerializeField] public uint AppId { get; private set; }
        
#if UNITY_EDITOR
        [MenuItem("Yak Tools/Steam/Steam Settings", false, 999)]
        public static void OpenSettings() => Open();
#endif
    }
}
