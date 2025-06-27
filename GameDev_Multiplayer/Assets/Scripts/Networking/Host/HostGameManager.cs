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
using System.Text;
using Unity.Services.Authentication;
public class HostGameManager : IDisposable
{
    //Note: no need for init method because the host is a client

    Allocation allocation;
    public NetworkServer NetworkServer {  get; private set; }

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
         *  5) Store User Data
         *  6) Start hosting
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

            string playerName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "N/A");
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(
                $"{playerName}'s Lobby", 
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

        //Create a new network server game object (my NetworkServer script)
        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        //Set up user data
        UserData userData = new UserData()
        {
            userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "N/A"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        //Convert user data to a bytearray
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        //store user data in the server
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        //The start host button in the network manager game object
        NetworkManager.Singleton.StartHost();

        NetworkServer.OnClientLeave += HandleClientLeave;

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

    public async void Dispose()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

        //Remove our lobby
        if (!string.IsNullOrEmpty(lobbyID))
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(lobbyID);
            }
            catch (LobbyServiceException lobbyServiceEx)
            {
                Debug.LogWarning(lobbyServiceEx);
            }

            lobbyID = string.Empty;
        }

        NetworkServer.OnClientLeave -= HandleClientLeave;

        NetworkServer?.Dispose();
    }

    public void Shutdown()
    {
        Dispose();
    }

    private async void HandleClientLeave(string authID)
    {
        try
        {
            //Remove player from lobby
            await LobbyService.Instance.RemovePlayerAsync(lobbyID, authID);
        }
        catch (LobbyServiceException lobbyServiceEx)
        {
            Debug.Log(lobbyServiceEx);
        }
    }
}
