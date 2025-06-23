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
    [Tooltip("The ticks per second that the players in zone will be healed at")]
    float healthTickRate = 0.5f;

    [SerializeField]
    int costToHealPerTick;

    [SerializeField]
    int healthPerTick;

    [SerializeField]
    [Tooltip("The number of ticks per second that the zone will recharge at")]
    float healingZoneRechargeTickRate = 10f;

    [SerializeField]
    int healingZonePowerRechargePerTick = 1;

    List<TankPlayer> playersInZone = new List<TankPlayer>();

    NetworkVariable<int> HealPower = new NetworkVariable<int>();

    WaitForSecondsRealtime tickWaitDuration;
    WaitForSecondsRealtime cooldownWaitDuration;
    WaitForSecondsRealtime rechargeTickWaitDuration;

    bool isOnCooldown;
    bool canRechargeZone;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            //update UI
            HealPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, HealPower.Value);
        }

        if (IsServer)
        {
            tickWaitDuration = new WaitForSecondsRealtime(1 / healthTickRate);
            cooldownWaitDuration = new WaitForSecondsRealtime(healCooldown);
            rechargeTickWaitDuration = new WaitForSecondsRealtime(1 / healingZoneRechargeTickRate);
            HealPower.Value = maxHealPower;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            //update UI
            HealPower.OnValueChanged -= HandleHealPowerChanged;
        }

        if (IsServer)
        {
            StopAllCoroutines();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer tankPlayer))
        {
            playersInZone.Add(tankPlayer);
            if (playersInZone.Count == 1)
            {
                Debug.Log("Starting to heal players in zone");
                StartCoroutine(HealPlayersInZone());
                canRechargeZone = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer tankPlayer))
        {
            playersInZone.Remove(tankPlayer);
            if (playersInZone.Count <= 0)
            {
                canRechargeZone = true;
                Debug.Log("No players in zone to heal");
                StopCoroutine(HealPlayersInZone());
                StartCoroutine(RechargeHealingZone());
            }
        }
    }

    void HandleHealPowerChanged(int oldValue, int newValue)
    {
        remainingHealthBar.fillAmount = (float)newValue / maxHealPower;
    }
    
    IEnumerator HealPlayersInZone()
    {
        while (true)
        {
            if (!isOnCooldown)
            {
                //Wait to heal players
                yield return tickWaitDuration;

                //Heal players in zone
                foreach (TankPlayer player in playersInZone)
                {
                    //Conditions to prevent healing
                    if (HealPower.Value <= 0) continue;
                    if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) continue;
                    if (!player.Wallet.TrySpendCoins(costToHealPerTick)) continue;


                    Debug.Log($"Healing {player.NetworkObject.name}...");

                    player.Health.RestoreHealth(healthPerTick);
                    HealPower.Value--;
                }

                //Check if out of heal power
                if (HealPower.Value <= 0 && !isOnCooldown)
                {
                    StartCoroutine(ResetHealZone());
                }
            }
            else
            {
                //Wait until next frame for the next check
                yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator ResetHealZone()
    {
        Debug.Log("Heal zone on cooldown...");
        isOnCooldown = true;
        yield return cooldownWaitDuration;
        Debug.Log("Heal zone off cooldown. Ready to heal");
        isOnCooldown = false;
        HealPower.Value = maxHealPower;
    }

    IEnumerator RechargeHealingZone()
    {
        while (canRechargeZone)
        {
            Debug.Log("Recharging healing zone...");
            //Recharge the health zone
            yield return rechargeTickWaitDuration;

            HealPower.Value += healingZonePowerRechargePerTick;
            HealPower.Value = Mathf.Clamp(HealPower.Value, 0, maxHealPower);

            if (HealPower.Value == maxHealPower)
            {
                Debug.Log("healing zone fully restored");
                //Health station full. no need to continue coroutine
                break;
            }
        }
    }
}
