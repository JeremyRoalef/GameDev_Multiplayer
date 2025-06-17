using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField]
    Transform leaderboardEntityHolder;

    [SerializeField]
    LeaderboardEntityDisplay leaderboardEntityPrefab;

    NetworkList<LeaderboardEntityState> leaderboardEntities;
    List<LeaderboardEntityDisplay> entityDisplays = new List<LeaderboardEntityDisplay>();

    private void Awake()
    {
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
            foreach (LeaderboardEntityState entity in leaderboardEntities)
            {
                HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState> 
                { 
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

        if (IsServer)
        {
            //Initial player leaderboards
            TankPlayer[] tankPlayers = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (TankPlayer player in tankPlayers)
            {
                HandlePlayerSpawned(player);
            }

            //New players added to scene
            TankPlayer.OnAnyPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnAnyPlayerDespawned += HandlePlayerDespawned;
        }

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsClient)
        {
            leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
        }

        if (IsServer)
        {
            TankPlayer.OnAnyPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnAnyPlayerDespawned -= HandlePlayerDespawned;
        }
    }

    void HandlePlayerSpawned(TankPlayer player)
    {
        leaderboardEntities.Add(new LeaderboardEntityState {
            ClientID = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
            });
    }

    void HandlePlayerDespawned(TankPlayer player)
    {
        if (leaderboardEntities == null) return;

        foreach (LeaderboardEntityState entity in leaderboardEntities)
        {
            if (entity.ClientID != player.OwnerClientId) continue;

            leaderboardEntities.Remove(entity);
            break;
        }
    }

    private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                //If there are no client IDs with this client's ID, add it
                if (!entityDisplays.Any(x => x.ClientID == changeEvent.Value.ClientID))
                {
                    LeaderboardEntityDisplay leaderboardEntity = 
                        Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);

                    leaderboardEntity.Initialize(
                        changeEvent.Value.ClientID, 
                        changeEvent.Value.PlayerName, 
                        changeEvent.Value.Coins
                        );

                    entityDisplays.Add(leaderboardEntity);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                LeaderboardEntityDisplay displayToRemove = 
                    entityDisplays.FirstOrDefault(x => x.ClientID == changeEvent.Value.ClientID);

                if (displayToRemove != null)
                {
                    displayToRemove.transform.parent = null;
                    Destroy(displayToRemove.gameObject);
                    entityDisplays.Remove(displayToRemove);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                LeaderboardEntityDisplay displayToUpdate =
                    entityDisplays.FirstOrDefault(x => x.ClientID == changeEvent.Value.ClientID);

                if (displayToUpdate != null)
                {
                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;
        }
    }
}
