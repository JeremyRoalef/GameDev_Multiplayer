using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyVFXAfterStop : MonoBehaviour
{
    [SerializeField]
    ParticleSystem vfx;

    float duration;

    private void Awake()
    {
        //Null Check
        if (vfx == null)
        {
            if (!TryGetComponent<ParticleSystem>(out vfx))
            {
                Debug.Log($"No particle system on {gameObject.name}. Disabling script");
                this.enabled = false;
            }
        }

        duration = vfx.main.startLifetime.constantMax;
    }

    private void Start()
    {
        Destroy(gameObject, duration);
    }
}
