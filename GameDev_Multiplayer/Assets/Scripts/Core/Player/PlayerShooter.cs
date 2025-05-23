using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShooter : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    UnityEngine.GameObject serverProjectilePrefab;

    [SerializeField]
    UnityEngine.GameObject clientProjectilePrefab;

    [SerializeField]
    InputReader inputReader;

    [SerializeField]
    Transform projectieSpawnPos;

    [SerializeField]
    CoinWallet wallet;

    [SerializeField]
    Collider2D playerCollider;

    [SerializeField]
    UnityEngine.GameObject muzzleFlash;

    [Header("Settings")]
    [SerializeField]
    [Tooltip("How fast should the projectile move?")]
    float projectileSpeed;

    [SerializeField]
    [Tooltip("How many projectile will be fired per second?")]
    float fireRate;

    [SerializeField]
    [Tooltip("How long should the muzzle flash effect play for?")]
    float muzzleFlashDuration;

    [SerializeField]
    int costToFire = 1;

    bool isFiring;
    bool canShoot = true;
    float muzzleFlashTimer;

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
        if (muzzleFlashTimer > 0)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer <= 0)
            {
                muzzleFlash.SetActive(false);
            }
        }

        if (!IsOwner) return;
        if (!isFiring) return;
        if (!canShoot) return;
        
        if (wallet.CanSpendCoins(costToFire))
        {
            ShootWeapon();
        }
        else
        {
            HandleInsufficientFunds();
        }
    }

    private void HandleInsufficientFunds()
    {
        Debug.Log("You Broke Loser LOL");
    }

    /// <summary>
    /// Spawn the projectile for the owner of the script
    /// </summary>
    void SpawnClientProjectile(Vector3 spawnPos, Vector3 dir)
    {
        //Create projectile
        UnityEngine.GameObject newProjectile = Instantiate(
            clientProjectilePrefab, 
            spawnPos,
            Quaternion.identity
            );

        //Set direction
        newProjectile.transform.up = dir;

        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;


        //Prevent player from interacting with its own projectile
        Physics2D.IgnoreCollision(playerCollider, newProjectile.GetComponent<Collider2D>());
        if(newProjectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
    }

    /// <summary>
    /// Tell the server to spawn its own projectile for logic handling
    /// </summary>
    [ServerRpc]
    void SpawnServerProjectileServerRPC(Vector3 spawnPos, Vector3 dir)
    {
        if (!wallet.TrySpendCoins(costToFire)) return;

        //Create projectile
        UnityEngine.GameObject newProjectile = Instantiate(
            serverProjectilePrefab,
            spawnPos,
            Quaternion.identity
            );

        //Set direction
        newProjectile.transform.up = dir;

        //Tell all clients to make this projectile
        SpawnCientProjectileClientRPC(spawnPos, dir);

        //Set owner of projectile in the DealDamageOnContact script
        if (newProjectile.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact ddoc))
        {
            ddoc.SetOwner(OwnerClientId);
        }

        //Prevent player from interacting with its own projectile
        Physics2D.IgnoreCollision(playerCollider, newProjectile.GetComponent<Collider2D>());
        if (newProjectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
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

    void ShootWeapon()
    {
        //Start shoot delay
        canShoot = false;
        StartCoroutine(EnableShoot());
        
        //Instantiate bullet
        SpawnClientProjectile(projectieSpawnPos.position, projectieSpawnPos.up);
        SpawnServerProjectileServerRPC(projectieSpawnPos.position, projectieSpawnPos.up);
    }

    IEnumerator EnableShoot()
    {
        yield return new WaitForSeconds(1 / fireRate);
        canShoot = true;
    }
}
