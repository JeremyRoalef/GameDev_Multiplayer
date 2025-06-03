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

    [Header("Settings")]
    [SerializeField]
    int cameraPriority = 100;

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
