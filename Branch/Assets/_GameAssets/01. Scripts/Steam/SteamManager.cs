using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static bool Initialized { get; private set; }

    private static SteamManager instance;

    // Callback for Steam overlay activation
    private Callback<GameOverlayActivated_t> gameOverlayActivated;

    public static bool IsOverlayActive { get; private set; }
    
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

        // Set up callback for Steam overlay activation
        gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);

        Debug.Log("[Steam] Overlay callback registered.");
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

    // Callback method for Steam overlay activation
    private void OnGameOverlayActivated(GameOverlayActivated_t callback)
    {
        IsOverlayActive = callback.m_bActive != 0;

        if (IsOverlayActive)
        {
            Debug.Log("[Steam] GameOverlayActivated_t received. Steam Overlay opened.");
        }
        else
        {
            Debug.Log("[Steam] GameOverlayActivated_t received. Steam Overlay closed.");
        }
    }
}