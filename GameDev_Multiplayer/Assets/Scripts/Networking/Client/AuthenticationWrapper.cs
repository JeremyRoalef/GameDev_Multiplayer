using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    /// <summary>
    /// Method to authenticate the player. This attempts to sign the player in anonymously
    /// </summary>
    /// <param name="maxRetries"></param>
    /// <returns></returns>
    public static async Task<AuthState> DoAuth(int maxRetries = 5)
    {
        //If already authenticated, then return
        if (AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }

        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticating. Waiting for authentication to complete");
            await Authenticating();
            return AuthState;
        }

        await SignInAnonymouslyAsync(maxRetries);

        return AuthState;
    }

    static async Task<AuthState> Authenticating()
    {
        while (AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            //wait until authenticated
            await Task.Delay(200);
        }

        return AuthState;
    }

    static async Task SignInAnonymouslyAsync(int maxRetries)
    {
        //Authenticating
        AuthState = AuthState.Authenticating;

        int tries = 0;
        while (AuthState == AuthState.Authenticating && tries < maxRetries)
        {
            try
            {
                //attempt anonymous sign in
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                //Check if singing in was successful
                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated;
                    break;
                }
            }
            //Handle errors

            //Failure to authenticate
            catch (AuthenticationException authEx)
            {
                Debug.LogError(authEx);
                AuthState = AuthState.Error;
            }
            //Failed to get the connection
            catch (RequestFailedException requestFailedEx)
            {
                Debug.LogError(requestFailedEx);
                AuthState = AuthState.Error;
            }

            tries++;
            //Wait 1 second before second attempt
            await Task.Delay(1000);
        }

        //Player not authenticated
        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"Player unsuccessfully signed in after {tries} attempts");
            AuthState = AuthState.TimeOut;
        }
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