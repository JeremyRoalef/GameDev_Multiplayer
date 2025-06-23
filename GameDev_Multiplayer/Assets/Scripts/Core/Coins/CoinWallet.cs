using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    BountyCoin bountyCoinPrefab;

    [SerializeField]
    Health health;

    [Header("Settings")]
    [SerializeField]
    LayerMask bountyCoinLayerMask;

    [SerializeField]
    float bountyCoinPercentage = 50;

    [SerializeField]
    int bountyCoinCount = 10;

    [SerializeField]
    int minBountyCoinValue = 4;

    [SerializeField]
    float bountyCoinSpawnRadius = 3f;

    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    Collider2D[] coinBuffer = new Collider2D[1];
    float coinRadius;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        coinRadius = bountyCoinPrefab.GetComponent<CircleCollider2D>().radius;

        health.OnDie += HandleDie;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        health.OnDie -= HandleDie;
    }

    private void HandleDie(Health health)
    {
        //Generate bounty coins
        int bountyValue = (int)(TotalCoins.Value * (bountyCoinPercentage / 100f));
        int bountyCoinValue = bountyValue / bountyCoinCount;

        if (bountyCoinValue < minBountyCoinValue) return;

        for (int i = 0; i < bountyCoinCount; i++)
        {
            BountyCoin bountyCoinInstance = Instantiate(bountyCoinPrefab, GetSpawnPos(), Quaternion.identity);
            bountyCoinInstance.SetValue(bountyCoinValue);
            //spawn bounty coin over the network
            bountyCoinInstance.NetworkObject.Spawn();
        }
    }

    //Note: The x and y position of the player is being sent to the server, so the server will always
    //      detect the clien's trigger event
    private void OnTriggerEnter2D(Collider2D other)
    {
        int coinValue = 0;

        if (other.TryGetComponent<Coin>(out Coin coin))
        {
            coinValue = coin.Collect();
        }

        if (!IsServer) return;
        TotalCoins.Value += coinValue;
    }

    /// <summary>
    /// SERVER ONLY! Method to spend Coins in wallet.
    /// </summary>
    /// <param playerName="amount"></param>
    /// <returns>Returns true if sufficient funds</returns>
    public bool TrySpendCoins(int amount)
    {
        if (amount > TotalCoins.Value)
        {
            return false;
        }
        else
        {
            TotalCoins.Value -= amount;
            return true;
        }
    }

    /// <summary>
    /// Method to check for sufficient funds given the passed amount
    /// </summary>
    /// <param playerName="amount"></param>
    /// <returns>True if sufficient. False otherwise</returns>
    public bool CanSpendCoins(int amount)
    {
        if (amount > TotalCoins.Value)
        {
            return false;
        }

        return true;
    }

    Vector2 GetSpawnPos()
    {
        while (true)
        {
            Vector2 spawnPoint = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * bountyCoinSpawnRadius;
            int nunColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, bountyCoinLayerMask);

            if (nunColliders == 0)
            {
                return spawnPoint;
            }
        }
    }
}
