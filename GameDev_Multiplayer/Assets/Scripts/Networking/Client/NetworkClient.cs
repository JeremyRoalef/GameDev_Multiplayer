using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    const string MENU_SCENE_NAME = "Menu";

    NetworkManager networkManager;

    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        //Called when a client disconnects from the server
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    public void Dispose()
    {
        //Unsubscribe
        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    public void Disconnect()
    {
        //If disconnected from server, return to menu
        if (SceneManager.GetActiveScene().name != MENU_SCENE_NAME)
        {
            SceneManager.LoadScene(MENU_SCENE_NAME);
        }

        //If trying to be a client, stop trying to be a client
        if (networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();
        }
    }

    private void OnClientDisconnect(ulong clientID)
    {
        //Check if it's the client
        if (clientID != 0 && clientID != networkManager.LocalClientId) return;

        Disconnect();
    }
}
