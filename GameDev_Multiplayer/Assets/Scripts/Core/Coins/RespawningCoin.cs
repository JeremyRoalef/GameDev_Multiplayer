using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> OnCollected;
    Vector3 previousPos;

    private void Start()
    {
        if (IsServer) return;
        previousPos = transform.position;
    }

    private void Update()
    {
        if (IsServer) return;
        if (previousPos != transform.position)
        {
            ShowCoin(true);
        }
        previousPos = transform.position;
    }

    public override int Collect()
    {
        //Server Logic
        if (IsServer)
        {
            if (isCollected)
            {
                return 0;
            }

            isCollected = true;
            OnCollected?.Invoke(this);
            return coinValue;
        }
        //Client Logic
        else
        {
            ShowCoin(false);
            return 0;
        }
    }

    /// <summary>
    /// Reset the coin to allow pickup again
    /// </summary>
    public void Reset()
    {
        isCollected = false;
    }
}
