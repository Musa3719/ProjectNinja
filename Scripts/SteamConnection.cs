using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamConnection : MonoBehaviour
{
    public static SteamConnection _instance;
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        Steamworks.SteamClient.Init(2474400, true);
        DontDestroyOnLoad(gameObject);
    }
}
