using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkTransform : NetworkTransform
{
    //Called when the object is spawned into the network
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //Assign write priveleges for transform component to the owner of the object
        CanCommitToTransform = IsOwner;
    }

    protected override void Update()
    {
        //Assign ownership of transform to the owner of the object
        CanCommitToTransform = IsOwner;
        base.Update();

        //Check if networkmanager exists
        if (NetworkManager != null)
        {
            //Check if client is connected to the networkmanager or (as the host) listening to clients
            if (NetworkManager.IsConnectedClient || NetworkManager.IsListening)
            {
                if (CanCommitToTransform)
                {
                    //Send current transform state to the server. time given ensure proper interpolation
                    TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
                }
            }
        }
    }

    //If this is server, return false (Only the client has authority to this network transform)
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
