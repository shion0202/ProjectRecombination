using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static bool Initialized { get; private set; }

    private static SteamManager instance;

    [SerializeField]
    private uint appId = 480;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
#if !UNITY_EDITOR
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(appId)))
            {
                Debug.Log("[Steam] Restarting through Steam.");
                Application.Quit();
                return;
            }
#endif
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steam] steam_api dll not found. Steamworks.NET plugin may not be installed correctly.\n" + e);
            return;
        }

        Initialized = SteamAPI.Init();

        if (!Initialized)
        {
            Debug.LogWarning("[Steam] SteamAPI.Init failed. Steam client may not be running, App ID may be missing, or the account may not own this App ID.");
            return;
        }

        Debug.Log("[Steam] Initialized.");
        Debug.Log("[Steam] User: " + SteamFriends.GetPersonaName());
    }

    private void Update()
    {
        if (!Initialized)
            return;

        SteamAPI.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        if (!Initialized)
            return;

        SteamAPI.Shutdown();
        Initialized = false;
    }
}