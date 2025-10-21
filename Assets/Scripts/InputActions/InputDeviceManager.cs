using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputDeviceManager : MonoBehaviour
{
    [SerializeField] private PlayerInput player1;
    [SerializeField] private PlayerInput player2;

    private InputUser user1;
    private InputUser user2;
    private bool player2Assigned = false; // Para saber si ya tiene gamepad

    void Awake()
    {
        // Crear usuarios independientes
        user1 = InputUser.CreateUserWithoutPairedDevices();
        user2 = InputUser.CreateUserWithoutPairedDevices();

        // Asignar teclado al Player1
        if (Keyboard.current != null)
            InputUser.PerformPairingWithDevice(Keyboard.current, user1);

        // Asignar primer gamepad al Player2 si está conectado al inicio
        if (Gamepad.all.Count > 0)
        {
            InputUser.PerformPairingWithDevice(Gamepad.all[0], user2);
            player2Assigned = true;
        }

        // Asociar actions
        user1.AssociateActionsWithUser(player1.actions);
        user2.AssociateActionsWithUser(player2.actions);

        // Suscribirse a cambios de dispositivos
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // Cuando se conecta un nuevo gamepad
        if (change == InputDeviceChange.Added && device is Gamepad)
        {
            if (!player2Assigned)
            {
                InputUser.PerformPairingWithDevice(device, user2);
                player2Assigned = true;
                Debug.Log($"Gamepad {device.displayName} asignado a Player2");
            }
        }
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
}