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

    [SerializeField]
    int maxEntitiesOnLeaderboard = 8;

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

        player.Wallet.TotalCoins.OnValueChanged += (oldCoins, newCoins) 
            => HandleCoinsChanged(player.OwnerClientId, newCoins);
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

        player.Wallet.TotalCoins.OnValueChanged -= (oldCoins, newCoins)
            => HandleCoinsChanged(player.OwnerClientId, newCoins);
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

        //Sort the list from largest to smallest
        entityDisplays.Sort((element1, element2) => element2.Coins.CompareTo(element1.Coins));

        for (int i = 0; i < entityDisplays.Count; i++)
        {
            //Set the order of the child entity display in hierarchy to the order in the list
            entityDisplays[i].transform.SetSiblingIndex(i);

            //Update the display text for the entity display
            entityDisplays[i].UpdateDisplayText();

            //Show the top max entities on leaderboard
            bool shouldShow = i <= maxEntitiesOnLeaderboard - 1;
            entityDisplays[i].gameObject.SetActive(shouldShow);


            //If the player is not on the leaderboard, remove the last person on the leaderboard and show the player instead
            LeaderboardEntityDisplay myDisplay =
                entityDisplays.FirstOrDefault(x => x.ClientID == NetworkManager.Singleton.LocalClientId);
            if (myDisplay != null)
            {
                if (myDisplay.transform.GetSiblingIndex() >= maxEntitiesOnLeaderboard)
                {
                    leaderboardEntityHolder.GetChild(maxEntitiesOnLeaderboard-1).gameObject.SetActive(false);
                    myDisplay.gameObject.SetActive(true);
                }
            }
        }
    }

    void HandleCoinsChanged(ulong clientID, int newCoins)
    {
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientID != clientID) continue;

            //Set leaderboard entity to a new leaderboard entity
            leaderboardEntities[i] = new LeaderboardEntityState
            {
                ClientID = leaderboardEntities[i].ClientID,
                PlayerName = leaderboardEntities[i].PlayerName,
                Coins = newCoins
            };
            return;
        }
    }
}
