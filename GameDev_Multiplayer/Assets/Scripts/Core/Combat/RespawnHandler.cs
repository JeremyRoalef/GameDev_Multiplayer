using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField]
    TankPlayer playerPrefab;

    [SerializeField]
    [Tooltip("for 25%, put 25, not 0.25")]
    float percentageOfCoinsKeptAfterDeath = 25f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        //Handle initial player spawning
        TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach(TankPlayer player in players)
        {
            HandlePlayerSpawned(player);
        }

        TankPlayer.OnAnyPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnAnyPlayerDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsServer) return;

        TankPlayer.OnAnyPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnAnyPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        //Subscribe to the OnDie event. Signature throws out the health script given and passes the TankPlayer script instead
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(TankPlayer player)
    {
        int playerKeptCoins = (int)(player.Wallet.TotalCoins.Value * percentageOfCoinsKeptAfterDeath / 100);

        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayer(player.OwnerClientId, playerKeptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientID, int playerCoins)
    {
        yield return null;

        //Create new player
        TankPlayer playerInstance = 
            Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        //Assign the object to the owner client ID
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientID);

        playerInstance.Wallet.TotalCoins.Value = playerCoins;
    }
}
