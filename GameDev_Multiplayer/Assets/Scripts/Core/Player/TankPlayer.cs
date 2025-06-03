using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    CinemachineVirtualCamera virtualCamera;

    [Header("Settings")]
    [SerializeField]
    int cameraPriority = 100;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            virtualCamera.Priority = cameraPriority;
        }
    }
}
