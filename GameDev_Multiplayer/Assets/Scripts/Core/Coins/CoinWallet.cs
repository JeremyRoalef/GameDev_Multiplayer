using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    //Note: The x and y position of the player is being sent to the server, so the server will always
    //      detect the clien's trigger event
    private void OnTriggerEnter2D(Collider2D other)
    {
        int coinValue = 0;

        if (other.TryGetComponent<Coin>(out Coin coin))
        {
            coinValue = coin.Collect();
        }

        if (!IsServer) return;
        TotalCoins.Value += coinValue;
    }

    /// <summary>
    /// SERVER ONLY! Method to spend coins in wallet.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>Returns true if sufficient funds</returns>
    public bool TrySpendCoins(int amount)
    {
        if (amount > TotalCoins.Value)
        {
            return false;
        }
        else
        {
            TotalCoins.Value -= amount;
            return true;
        }
    }

    /// <summary>
    /// Method to check for sufficient funds given the passed amount
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>True if sufficient. False otherwise</returns>
    public bool CanSpendCoins(int amount)
    {
        if (amount > TotalCoins.Value)
        {
            return false;
        }

        return true;
    }
}
