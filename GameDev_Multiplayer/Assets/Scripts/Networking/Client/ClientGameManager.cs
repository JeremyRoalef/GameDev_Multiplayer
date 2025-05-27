using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    JoinAllocation allocation;
    NetworkClient networkClient;

    const string MAIN_MENU_SCENE_STRING = "Menu";

    public async Task<bool> InitAsync()
    {
        //Wait for unity services to initialize. Must do this before any client authorization
        await UnityServices.InitializeAsync();

        //Create network client object
        networkClient = new NetworkClient(NetworkManager.Singleton);

        //Attempt to sign in
        AuthState authState = await AuthenticationWrapper.DoAuth();

        //Check if authentication was successful
        if (authState == AuthState.Authenticated)
        {
            return true;
        }
        
        //failure to authenticate
        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE_STRING);
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }


        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //Create the relay server data ("udd" and "dtls" are common)
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

        //Set the relay server data for our transport
        transport.SetRelayServerData(relayServerData);


        //Set up user data
        UserData userData = new UserData()
        {
            userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "N/A"),
            userAuthID = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;


        //The start host button in the network manager game object
        NetworkManager.Singleton.StartClient();

        //Note: Only the host needs to hande scene transition logic. Clients dont need to
    }
}
