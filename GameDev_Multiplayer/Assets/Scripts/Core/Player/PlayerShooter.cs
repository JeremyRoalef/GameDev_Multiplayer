using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShooter : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    GameObject serverProjectilePrefab;

    [SerializeField]
    GameObject clientProjectilePrefab;

    [SerializeField]
    InputReader inputReader;

    [SerializeField]
    Transform projectieSpawnPos;

    [Header("Settings")]
    [SerializeField]
    float projectileSpeed;

    bool isFiring;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        base.OnNetworkSpawn();

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        base.OnNetworkDespawn();

        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!isFiring) return;

        SpawnClientProjectile(projectieSpawnPos.position, projectieSpawnPos.up);
        SpawnServerProjectileServerRPC(projectieSpawnPos.position, projectieSpawnPos.up);
    }

    /// <summary>
    /// Spawn the projectile for the owner of the script
    /// </summary>
    void SpawnClientProjectile(Vector3 spawnPos, Vector3 dir)
    {
        //Create projectile
        GameObject newProjectile = Instantiate(
            clientProjectilePrefab, 
            spawnPos, 
            Quaternion.identity
            );

        //Set direction
        newProjectile.transform.up = dir;
    }

    /// <summary>
    /// Tell the server to spawn its own projectile for logic handling
    /// </summary>
    [ServerRpc]
    void SpawnServerProjectileServerRPC(Vector3 spawnPos, Vector3 dir)
    {
        //Create projectile
        GameObject newProjectile = Instantiate(
            serverProjectilePrefab,
            spawnPos,
            Quaternion.identity
            );

        //Set direction
        newProjectile.transform.up = dir;

        //Tell all clients to make this projectile
        SpawnCientProjectileClientRPC(spawnPos, dir);
    }

    /// <summary>
    /// Runs for all clients (even the owner) to spawn a projectile
    /// </summary>
    [ClientRpc]
    void SpawnCientProjectileClientRPC(Vector3 spawnPos, Vector3 dir)
    {
        //Filter out the owner (already made projectile)
        if (IsOwner) return;

        SpawnClientProjectile(spawnPos, dir);
    }

    void HandlePrimaryFire(bool isFiring)
    {
        this.isFiring = isFiring;
    }
}
