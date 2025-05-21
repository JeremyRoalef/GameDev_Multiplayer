using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField]
    ClientSingleton clientPrefab;

    [SerializeField]
    HostSingleton hostPrefab;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);

        //This checks if this is being run on the dedicated server, since the server doesn't need a graphics device type
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    async Task LaunchInMode(bool isDedicatedServer)
    {
        //Dedicated server logic
        if (isDedicatedServer)
        {

        }

        //Host & client logic
        else
        {
            //Spawn client & host singletons
            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool authenticated = await clientSingleton.CreateClient();

            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();

            if (authenticated)
            {
                //go to main menu
                clientSingleton.GameManager.GoToMenu();
            }
            else
            {
                //handle failure
            }
        }
    }
}
