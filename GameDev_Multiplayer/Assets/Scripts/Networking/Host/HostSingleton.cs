using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    HostGameManager hostGameManager;

    private static HostSingleton instance;
    public static HostSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindObjectOfType<HostSingleton>();
            if (instance == null)
            {
                Debug.LogWarning("No host singleton in scene");
                return null;
            }

            return instance;
        }
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreateHost()
    {
        //Note: the host is a client, so the init logic doesn't need to happen here
        hostGameManager = new HostGameManager();
    }
}
