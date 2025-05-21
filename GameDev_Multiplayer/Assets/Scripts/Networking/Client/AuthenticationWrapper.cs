using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    /// <summary>
    /// Method to authenticate the player. This attempts to sign the player in anonymously
    /// </summary>
    /// <param name="maxTries"></param>
    /// <returns></returns>
    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        //If already authenticated, then return
        if (AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }

        //Authenticating
        AuthState = AuthState.Authenticating;

        int tries = 0;
        while (AuthState == AuthState.Authenticating && tries < maxTries)
        {
            //attempt anonymous sign in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            //Check if singing in was successful
            if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
            {
                AuthState = AuthState.Authenticated;
                break;
            }

            tries++;

            //Wait 1 second before second attempt
            await Task.Delay(1000);
        }

        return AuthState;
    }
}

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}