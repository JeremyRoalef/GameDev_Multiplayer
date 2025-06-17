using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField]
    RespawningCoin coinPrefab;

    [SerializeField]
    int maxCoins = 50;

    [SerializeField]
    int coinValue = 10;

    [SerializeField]
    Vector2 xSpawnRange;

    [SerializeField]
    Vector2 ySpawnRange;

    [SerializeField]
    [Tooltip("Which layers will the coin spawner account for to prevent spawning in such areas")]
    LayerMask layerMask;

    Collider2D[] coinBuffer = new Collider2D[1];

    float radius;

    public override void OnNetworkSpawn()
    {
        //Server only
        if (!IsServer) { return; }
        base.OnNetworkSpawn();

        //Get the radius of the coin
        radius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        //Spawn Coins
        for (int i = 0; i < maxCoins; i++)
        {
            SpawnCoin();
        }
    }

    /// <summary>
    /// Spawns and initializes a coin across the network
    /// </summary>
    void SpawnCoin()
    {
        //Make the coin instance
        RespawningCoin coinInstance = Instantiate(
            coinPrefab, 
            GetSpawnPos(), 
            Quaternion.identity
            );
        
        //Initialize the coin
        coinInstance.SetValue(coinValue);

        //Synchronize the instantiation for the clients
        coinInstance.GetComponent<NetworkObject>().Spawn();

        coinInstance.OnCollected += HandleCoinCollected;
    }

    private void HandleCoinCollected(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPos();
        coin.Reset();
    }

    /// <summary>
    /// Returns an x,y position in the scene world
    /// </summary>
    /// <returns>The position the object will spawn</returns>
    Vector2 GetSpawnPos()
    {
        float x = 0;
        float y = 0;

        while (true)
        {
            x = Random.Range(xSpawnRange.x, ySpawnRange.y);
            y = Random.Range(ySpawnRange.x, xSpawnRange.y);

            Vector2 spawnPoint = new Vector2(x, y);
            int nunColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, radius, coinBuffer, layerMask);

            if (nunColliders == 0)
            {
                return spawnPoint;
            }
        }
    }
}
