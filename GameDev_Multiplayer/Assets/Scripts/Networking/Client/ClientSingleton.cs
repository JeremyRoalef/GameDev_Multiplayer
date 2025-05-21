using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;
    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) return instance;
            
            instance = FindObjectOfType<ClientSingleton>();
            if (instance == null)
            {
                Debug.LogWarning("No client singleton in scene");
                return null;
            }

            return instance;
        }
    }

    public ClientGameManager GameManager { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public async Task<bool> CreateClient()
    {
        GameManager = new ClientGameManager();
        return await GameManager.InitAsync();
    }
}
