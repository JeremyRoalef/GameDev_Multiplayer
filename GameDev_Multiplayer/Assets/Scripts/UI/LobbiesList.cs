using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField]
    Transform lobbyItemParentObj;

    [SerializeField]
    LobbyItem lobbyItemPrefab;

    bool isJoining;
    bool isRefreshing;

    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if (isRefreshing) return;

        isRefreshing = true;

        try
        {
            //Create the lobby options
            QueryLobbiesOptions options = new QueryLobbiesOptions();

            //Lobbies to retrieve
            options.Count = 25;
            
            //Filter lobbies
            options.Filters = new List<QueryFilter>()
            {
                //Filter #1: Remove filled up lobbies
                new QueryFilter(
                    //Get the available slots of the lobby
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    //Check if it is greater than...
                    op: QueryFilter.OpOptions.GT,
                    //Greater than 0
                    value: "0"
                    ),
                //Filter #2: remove locked lobbies
                new QueryFilter(
                    //Check if query is locked
                    field: QueryFilter.FieldOptions.IsLocked,
                    //Check if it is equal to...
                    op: QueryFilter.OpOptions.EQ,
                    //equal to 0. This means that the lobby is not locked
                    value: "0"
                    )
                //More filters if wanted
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

            //Clear previous lobbies
            foreach(Transform child in lobbyItemParentObj)
            {
                Destroy(child);
            }

            //Generate new lobbies
            foreach (Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParentObj);
                lobbyItem.Initialize(this, lobby);
            }

        }
        catch (LobbyServiceException lobbyServiceEx)
        {
            Debug.LogWarning(lobbyServiceEx);
        }



        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining) return;

        isJoining = true;

        Lobby joinLobby;

        //Try to join the lobby by its ID
        try
        {
            joinLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);

            //Get the join code key (data set up by host game manager)
            string joinCode = joinLobby.Data["JoinCode"].Value;

            //Join as client
            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException lobbyServiceEx)
        {
            Debug.LogWarning(lobbyServiceEx);
        }

        isJoining = false;
    }
}
