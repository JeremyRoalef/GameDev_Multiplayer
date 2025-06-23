using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    Image remainingHealthBar;


    [Header("Settings")]
    [SerializeField]
    int maxHealPower = 30;

    [SerializeField]
    float healCooldown = 15f;

    [SerializeField]
    float healthTickRate = 0.5f;

    [SerializeField]
    int costToHealPerTick;

    [SerializeField]
    int healthPerTick;

    List<TankPlayer> playersInZone = new List<TankPlayer>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer tankPlayer))
        {
            playersInZone.Add(tankPlayer);
            Debug.Log("Player in zone");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer tankPlayer))
        {
            playersInZone.Remove(tankPlayer);
            Debug.Log("Player left zone");
        }
    }
}
