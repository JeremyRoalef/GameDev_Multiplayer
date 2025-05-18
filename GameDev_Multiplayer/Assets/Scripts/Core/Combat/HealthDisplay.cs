using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    Health health;

    [SerializeField]
    Image healthBarImage;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        base.OnNetworkSpawn();
        health.CurrentHealth.OnValueChanged += HandleHealthChanged;

        //Initialize value
        HandleHealthChanged(0, health.CurrentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) return;

        base.OnNetworkDespawn();
        health.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    /// <summary>
    /// Method to handle changes to the network varaible attached to this game object's health class.
    /// Since the CurrentHealth property in health class is a network variable, this will be called automatically
    /// by the server when its value is chaged.
    /// </summary>
    void HandleHealthChanged(int oldHealth, int newHealth)
    {
        //Do not integer divide. 9/10 = 0 ||| 9.0/10.0 = 0.9
        healthBarImage.fillAmount = (float)newHealth / health.MaxHealth;
    }


}
