using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer: IDisposable
{
    public Action<string> OnClientLeave;

    NetworkManager networkManager;

    /// <summary>
    /// Dictionary to convert the client network ID into user auth ID
    /// </summary>
    Dictionary<ulong, string> clientIDToAuth = new Dictionary<ulong, string>();

    /// <summary>
    /// Dictionary to convert the user auth ID into user data game object
    /// </summary>
    Dictionary<string, UserData> authIDToUserData = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        //Called when people connect to the server
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        //Called when the server is created (from StartHost or StartServer methods)
        networkManager.OnServerStarted += OnNetworkReady;
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        //Convert payload to a string
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        //Add client information to server storage
        clientIDToAuth[request.ClientNetworkId] = userData.userAuthID;
        authIDToUserData[userData.userAuthID] = userData;


        Debug.Log(userData.userName);

        //Finish connection to server
        response.Approved = true;

        //Set player spawn point
        response.Position = SpawnPoint.GetRandomSpawnPos();
        response.Rotation = Quaternion.identity;

        //Create the player game object
        response.CreatePlayerObject = true;
    }

    private void OnNetworkReady()
    {
        //Called when a clinet disconnects from the server
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientID)
    {
        //Remove player information
        if (clientIDToAuth.TryGetValue(clientID, out string authId))
        {
            clientIDToAuth.Remove(clientID);
            authIDToUserData.Remove(authId);
            OnClientLeave?.Invoke(authId);
        }
    }

    public void Dispose()
    {
        if (!networkManager) return;

        //Unsubscribe from events
        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnServerStarted -= OnNetworkReady;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;

        //shut down the network manager
        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }

    public UserData GetUserDataByClientID(ulong clientID)
    {
        if (clientIDToAuth.TryGetValue(clientID, out string authID))
        {
            if (authIDToUserData.TryGetValue(authID, out UserData userData))
            {
                //User data found
                return userData;
            }
            //No authorization ID found
            else
            {
                Debug.LogWarning("No Auth ID found");
                return null;
            }
        }
        //No client ID found
        else
        {
            Debug.LogWarning("No Client ID found");
            return null;
        }
    }
}
