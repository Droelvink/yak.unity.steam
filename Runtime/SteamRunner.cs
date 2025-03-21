using System;
using System.Reflection;
using Steamworks;
using UnityEngine;

namespace com.yak.steam
{
    [DefaultExecutionOrder(0)]
    public class SteamRunner : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Execute()
        {
            var bootstrapperObject = Resources.Load<GameObject>("SteamRunner");
            var obj = Instantiate(bootstrapperObject);
            obj.name = "SteamRunner";
            DontDestroyOnLoad(obj);
        }
        
        protected void Awake()
        {
            
            if (Application.isEditor && !SteamSettings.Instance.SteamEnabled) return;
            Init();
        }

        public void Init()
        {
            if (!Packsize.Test())
                throw new Exception(
                    "[com.yak.steam] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            if (!DllCheck.Test())
                throw new Exception(
                    "[com.yak.steam] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            try
            {
                if (SteamAPI.RestartAppIfNecessary(new AppId_t(Steam.AppId)))
                {
                    Debug.LogWarning("[com.yak.steam] RestartAppIfNecessary returned true, restarting the app.", this);
                    Application.Quit();
                    return;
                }
            }
            catch (DllNotFoundException e)
            {
                Debug.LogError("[com.yak.steam] Could not load [lib]steam_api.dll/so/dylib. Exception: \n" + e, this);
                Application.Quit();
                return;
            }
            
            SetInitializedState(SteamAPI.Init());
            if(SteamAPI.Init()) Debug.Log("[com.yak.steam] Steam initialization was successful.");
            else Debug.LogError("[com.yak.steam] Steam initialization failed.");
        }
        

        private void SetInitializedState(bool state)
        {
            var initializedProperty = typeof(Steam).GetProperty("Initialized", BindingFlags.Static | BindingFlags.Public);
            var setter = initializedProperty?.GetSetMethod(true);
            if (setter != null) setter.Invoke(null, new object[] { state });
        }
        
        private void Update()
        {
            if (!Steam.Initialized) return;
            SteamAPI.RunCallbacks();
        }
    }
}
