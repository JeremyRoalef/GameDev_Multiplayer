using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField]
    TMP_Text lobbyNameText;

    [SerializeField]
    TMP_Text lobbyPlayersText;

    LobbiesList lobbiesList;
    Lobby lobby;

    public void Initialize(LobbiesList lobbiesList, Lobby lobby)
    {
        lobbyNameText.text = lobby.Name;
        lobbyPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

        this.lobbiesList = lobbiesList;
        this.lobby = lobby;
    }

    public void Join()
    {
        lobbiesList.JoinAsync(lobby);
    }
}
