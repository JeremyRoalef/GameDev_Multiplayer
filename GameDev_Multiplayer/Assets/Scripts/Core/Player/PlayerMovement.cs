using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/*
 * Node: Most methods will need to check if the script is being run by the owner, not the server or another client
 *       This can be done through the IsOwner boolean in NetworkBehaviour
 */

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("The scriptable object for player movement")]
    InputReader inputReader;

    [SerializeField]
    [Tooltip("The body transform of the tank object")]
    Transform bodyTransform;

    [SerializeField]
    [Tooltip("The object's rigidbody component")]
    Rigidbody2D rb;

    [Header("Settings")]

    [SerializeField]
    [Tooltip("How fast will the player move?")]
    float moveSpeed = 10f;

    [SerializeField]
    [Tooltip("How fast will the player turn?")]
    float turnRate = 30f;

    Vector2 previousMovementInput;

    //Called when the game object spawns in the network
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        //Subscibe
        inputReader.MoveEvent += HandleMove;
    }

    //Called when the game object despawns from the network 
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        //Unsubscribe
        inputReader.MoveEvent -= HandleMove;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        //Move player up/down relative to itself
        rb.velocity = (Vector2)bodyTransform.up * previousMovementInput.y * moveSpeed;
    }

    private void Update()
    {
        if (!IsOwner) return;

        float zRotation = previousMovementInput.x * -turnRate * Time.deltaTime;

        //Rotate player clockwise/counter-clockwise
        bodyTransform.Rotate(
            0,
            0,
            zRotation);
    }

    void HandleMove(Vector2 moveInput)
    {
        //Store value in local variable
        previousMovementInput = moveInput;
    }
}
