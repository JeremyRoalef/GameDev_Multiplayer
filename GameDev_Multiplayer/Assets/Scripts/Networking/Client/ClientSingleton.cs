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

    ClientGameManager gameManager;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public async Task CreateClient()
    {
        gameManager = new ClientGameManager();
        await gameManager.InitAsync();
    }
}
