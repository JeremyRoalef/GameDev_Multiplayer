using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAim : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("The parent transform of the player's turret")]
    Transform turretTransform;

    [SerializeField]
    [Tooltip("The scriptable object for player input")]
    InputReader inputReader;

    /*
     * Note to self: transform.LookAt makes the forward vector point in the direction of the target
     * So this method is completely useless in 2D video games :/
     */

    private void LateUpdate()
    {
        if (!IsOwner) return;

        //Aim turret at mouse world position
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(inputReader.AimPos);
        Vector2 dir = mouseWorldPos - (Vector2)turretTransform.position;

        turretTransform.up = dir;
    }
}
