using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;
public class HostGameManager
{
    //Note: no need for init method because the host is a client

    Allocation allocation;
    string joinCode;
    string lobbyID;
    
    const int MAX_CONNECTIONS = 20;

    const string GAME_SCENE_STRING = "GameplayScene";

    public async Task StartHostAsync()
    {
        /*
         * Order of hosting events:
         *  1) Allocation setup
         *  2) Retrieve join code
         *  3) Transport setup
         *  4) Lobby Setup
         *  5) Start hosting
         */

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

        //Get the join code
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

        //Store reference to the unity transport on the network manager
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //Create the relay server data ("udd" and "dtls" are common)
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

        //Set the relay server data for our transport
        transport.SetRelayServerData(relayServerData);


        //Set up the lobby
        try
        {
            //Initialize lobby options
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                        )
                }
            };

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(
                "My Lobby", 
                MAX_CONNECTIONS, 
                lobbyOptions
                );

            lobbyID = lobby.Id;

            //Tell the host instance to run this code's heartbeat coroutine
            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException lobbyServiceEx)
        {
            Debug.LogWarning($"{lobbyServiceEx.Message}");
            return;
        }


        //The start host button in the network manager game object
        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_STRING, LoadSceneMode.Single);
    }

    //Create lobby heartbeat (so it doesn't shut down)
    IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while(true)
        {
            //Tell the lobby service to keep this lobby alive
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyID);
            yield return delay;
        }
    }
}
