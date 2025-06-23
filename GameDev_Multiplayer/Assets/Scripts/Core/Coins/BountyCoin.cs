using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BountyCoin : Coin
{
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
            Destroy(gameObject);
            return coinValue;
        }
        //Client Logic
        else
        {
            ShowCoin(false);
            return 0;
        }
    }
}
