using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField]
    TMP_Text displayText;

    public ulong ClientID {  get; private set; }
    public int Coins {  get; private set; }
    
    FixedString32Bytes playerName;

    public void Initialize(ulong clientID, FixedString32Bytes playerName, int coins)
    {
        this.ClientID = clientID;
        this.playerName = playerName;
        this.Coins = coins;

        UpdateDisplayText();
    }

    private void UpdateDisplayText()
    {
        displayText.text = $"1. {playerName} ({Coins})";
    }

    public void UpdateCoins(int newCoins)
    {
        Coins = newCoins;

        UpdateDisplayText();
    }
}
