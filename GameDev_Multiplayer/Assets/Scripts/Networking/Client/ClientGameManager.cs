using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    const string MAIN_MENU_SCENE_STRING = "Menu";

    public async Task<bool> InitAsync()
    {
        //Wait for unity services to initialize. Must do this before any client authorization
        await UnityServices.InitializeAsync();

        //Attempt to sign in
        AuthState authState = await AuthenticationWrapper.DoAuth();

        //Check if authentication was successful
        if (authState == AuthState.Authenticated)
        {
            return true;
        }
        
        //failure to authenticate
        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE_STRING);
    }
}
