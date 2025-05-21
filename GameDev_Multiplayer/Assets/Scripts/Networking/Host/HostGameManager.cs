using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager
{
    //Note: no need for init method because the host is a client

    Allocation allocation;
    string joinCode;
    const int MAX_CONNECTIONS = 20;

    const string GAME_SCENE_STRING = "GameplayScene";

    public async Task StartHostAsync()
    {
        //Allocation setup
        try
        {
            allocation = await Relay.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
            return;
        }


        try
        {
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"join code: {joinCode}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
            return;
        }

        //Transport setup (allows external connection to game)
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(allocation, "udp");
        transport.SetRelayServerData(relayServerData);

        //The start host button in the network manager game object
        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_STRING, LoadSceneMode.Single);
    }
}
