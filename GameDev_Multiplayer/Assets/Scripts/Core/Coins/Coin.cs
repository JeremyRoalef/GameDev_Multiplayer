using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Coin : NetworkBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;

    protected int coinValue;
    protected bool isCollected;

    public abstract int Collect();

    public void SetValue(int value)
    {
        coinValue = value;
    }

    protected void ShowCoin(bool isVisible)
    {
        spriteRenderer.enabled = isVisible;
    }
}
