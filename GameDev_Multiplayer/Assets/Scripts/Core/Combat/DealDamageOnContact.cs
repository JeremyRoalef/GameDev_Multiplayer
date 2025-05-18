using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField]
    int damage = 5;

    ulong clientOwnerID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            if (netObj.OwnerClientId == clientOwnerID)
            {
                return;
            }
        }

        if (other.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage);
        }
    }



    /// <summary>
    /// Sets the owner of the projectile to prevent collision logic with projectile's owner
    /// </summary>
    /// <param name="ownerClientID">Big integer</param>
    public void SetOwner(ulong ownerClientID)
    {
        this.clientOwnerID = ownerClientID;
    }
}
