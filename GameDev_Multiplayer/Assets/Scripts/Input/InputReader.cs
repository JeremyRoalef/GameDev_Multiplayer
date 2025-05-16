using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "ScriptableObjects/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    //Events to listen to by player objects
    public event Action<bool> PrimaryFireEvent;
    public event Action<Vector2> MoveEvent;

    public Vector2 AimPos {  get; private set; }


    //Generated C# scripts from Controls input action map
    Controls controls;

    //Enabling & creating controls
    void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        }

        controls.Player.Enable();
    }

    //Disabling controls
    void OnDisable()
    {
        controls.Player.Disable();
    }

    //Listens to the player's input for movement (WASD or arrow keys). Sends value to events for other scripts to listen to
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    //Listens to the player's input for firing (space or left click). Sends value ot events for other scripts to listen to
    public void OnPrimaryFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PrimaryFireEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            PrimaryFireEvent?.Invoke(false);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        AimPos = context.ReadValue<Vector2>();
    }
}
