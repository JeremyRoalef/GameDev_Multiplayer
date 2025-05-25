using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
    [SerializeField]
    Button connectButton;

    [SerializeField]
    TMP_InputField nameField;

    [SerializeField]
    [Range(0, 100)]
    int minNameLength = 1;

    [SerializeField]
    [Range(1, 100)]
    int maxNameLength = 100;

    const string PLAYER_NAME_KEY = "PlayerName";

    private void Start()
    {
        //Check if headless server
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            //Switch to next scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        //Get the player's name from their preferences. If no string, default to empty
        nameField.text = PlayerPrefs.GetString(PLAYER_NAME_KEY, string.Empty);
        HandleNameChanged();
    }

    public void HandleNameChanged()
    {
        connectButton.interactable = nameField.text.Length >= minNameLength && 
            nameField.text.Length <= maxNameLength;
    }

    public void Connect()
    {
        //Set the player's name & store it to preferences
        PlayerPrefs.SetString(PLAYER_NAME_KEY, nameField.text);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
