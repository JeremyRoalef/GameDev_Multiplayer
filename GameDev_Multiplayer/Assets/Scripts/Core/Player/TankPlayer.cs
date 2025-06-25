using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    CinemachineVirtualCamera virtualCamera;

    [field: SerializeField]
    public Health Health {  get; private set; }

    [field: SerializeField]
    public CoinWallet Wallet { get; private set; }

    [SerializeField]
    SpriteRenderer playerMiniMapSpriteRenderer;

    [Header("Settings")]
    [SerializeField]
    int cameraPriority = 100;

    [SerializeField]
    [Tooltip("The color the player will appear as on the mini map")]
    Color selfPlayerMiniMapColor = Color.blue;

    [SerializeField]
    [Tooltip("The color friendly players will appear as on the mini map")]
    Color friendlyPlayerMiniMapColor = Color.green;

    [SerializeField]
    [Tooltip("The color enemy players will appear as on the mini map")]
    Color enemyPlayerMiniMapColor = Color.red;

    public static event Action<TankPlayer> OnAnyPlayerSpawned;
    public static event Action<TankPlayer> OnAnyPlayerDespawned;

    //Note: No string data type can be synced over network. Use FixedString32Bytes instead.
    //Note Note: The conversion beteen string and FixedString32Bytes happens automatically
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);
            PlayerName.Value = userData.userName;

            OnAnyPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            virtualCamera.Priority = cameraPriority;
            playerMiniMapSpriteRenderer.color = selfPlayerMiniMapColor;
        }
        else
        {
            playerMiniMapSpriteRenderer.color = enemyPlayerMiniMapColor;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            OnAnyPlayerDespawned?.Invoke(this);
        }
    }
}
