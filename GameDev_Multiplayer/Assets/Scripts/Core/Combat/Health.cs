using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    //Syntax for serializing a property attribute
    [field: SerializeField]
    public int MaxHealth { get; private set; } = 100;

    //Network variables sync values between all clients automatically
    //Also has a OnValueChanged action, so all clients can run their own code based on the value change
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

    bool isDead = false;
    public Action<Health> OnDie;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        base.OnNetworkSpawn();

        CurrentHealth.Value = MaxHealth;
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healthValue)
    {
        ModifyHealth(healthValue);
    }

    void ModifyHealth(int value)
    {
        if (isDead) return;

        CurrentHealth.Value += value;
        CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value, 0, MaxHealth);

        if (CurrentHealth.Value <= 0)
        {
            isDead = true;
            OnDie?.Invoke(this);
        }
    }
}
