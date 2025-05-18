using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawningCoin : Coin
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
