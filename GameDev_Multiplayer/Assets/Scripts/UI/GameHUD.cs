using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            //Shut down the server
            HostSingleton.Instance.GameManager.Shutdown();

            //Clients should leave too
            ClientSingleton.Instance.GameManager.Disconnect();
        }
        else
        {
            //Clients should leave too
            ClientSingleton.Instance.GameManager.Disconnect();
        }
    }
}
