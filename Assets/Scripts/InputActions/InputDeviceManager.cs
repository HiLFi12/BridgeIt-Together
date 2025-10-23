using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputDeviceManager : MonoBehaviour
{
    [SerializeField] private PlayerInput player1;
    [SerializeField] private PlayerInput player2;

    private Gamepad player1Gamepad = null;
    private Gamepad player2Gamepad = null;

    private HashSet<InputDevice> player1Devices = new HashSet<InputDevice>();
    private HashSet<InputDevice> player2Devices = new HashSet<InputDevice>();

    private Player player1Script;
    private Player player2Script;

    void Awake()
    {
        // Crear instancias separadas de ActionAssets
        player1.actions = Instantiate(player1.actions);
        player2.actions = Instantiate(player2.actions);

        player1Script = player1.GetComponent<Player>();
        player2Script = player2.GetComponent<Player>();

        // Deshabilitar el auto-switching
        player1.neverAutoSwitchControlSchemes = true;
        player2.neverAutoSwitchControlSchemes = true;

        // Asignar dispositivos iniciales
        if (Keyboard.current != null)
        {
            player1Devices.Add(Keyboard.current);
        }

        if (Gamepad.all.Count > 0)
        {
            player2Gamepad = Gamepad.all[0];
            player2Devices.Add(player2Gamepad);
            player2Devices.Add(Keyboard.current);
        }
        if (Gamepad.all.Count > 1)
        {
            player1Gamepad = Gamepad.all[1];
            player1Devices.Add(player1Gamepad);
        }

        // Configurar filtros de dispositivos
        SetupDeviceFilters();

        // Set initial UI types
        player1Script.SetUIType(player1Gamepad != null);
        player2Script.SetUIType(player2Gamepad != null);

        // Habilitar acciones
        player1.actions.Enable();
        player2.actions.Enable();

        // Suscribirse a cambios de dispositivos
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void SetupDeviceFilters()
    {
        // Convertir HashSet a lista para Player1
        var p1DeviceList = new List<InputDevice>(player1Devices);
        if (p1DeviceList.Count > 0)
        {
            player1.actions.devices = new UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputDevice>(p1DeviceList.ToArray());
        }

        // Convertir HashSet a lista para Player2
        var p2DeviceList = new List<InputDevice>(player2Devices);
        if (p2DeviceList.Count > 0)
        {
            player2.actions.devices = new UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputDevice>(p2DeviceList.ToArray());
        }

        Debug.Log($"Player1 devices: {player1Devices.Count}");
        Debug.Log($"Player2 devices: {player2Devices.Count}");
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (!(device is Gamepad newGamepad))
            return;

        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Reconnected:
                // Si es el gamepad de Player2
                if (player2Gamepad != null && AreGamepadsSame(newGamepad, player2Gamepad))
                {
                    player2Gamepad = newGamepad;
                    player2Devices.Remove(device); // Remover versión vieja
                    player2Devices.Add(newGamepad);
                    SetupDeviceFilters();
                    player2Script.SetUIType(true);
                    Debug.Log($"Gamepad reconectado a Player2");
                    return;
                }

                // Si es el gamepad de Player1
                if (player1Gamepad != null && AreGamepadsSame(newGamepad, player1Gamepad))
                {
                    player1Gamepad = newGamepad;
                    player1Devices.Remove(device); // Remover versión vieja
                    player1Devices.Add(newGamepad);
                    SetupDeviceFilters();
                    player1Script.SetUIType(true);
                    Debug.Log($"Gamepad reconectado a Player1");
                    return;
                }

                // Nuevo gamepad
                if (player2Gamepad == null)
                {
                    player2Gamepad = newGamepad;
                    player2Devices.Add(newGamepad);
                    SetupDeviceFilters();
                    player2Script.SetUIType(true);
                    Debug.Log($"Gamepad asignado a Player2");
                }
                else if (player1Gamepad == null)
                {
                    player1Gamepad = newGamepad;
                    player1Devices.Add(newGamepad);
                    SetupDeviceFilters();
                    player1Script.SetUIType(true);
                    Debug.Log($"Gamepad asignado a Player1");
                }
                break;

            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
                // Remover de los sets pero mantener la referencia para reconocer al reconectar
                if (player2Devices.Contains(device))
                {
                    player2Devices.Remove(device);
                    if (device == player2Gamepad)
                    {
                        player2Script.SetUIType(false);
                    }
                    SetupDeviceFilters();
                    Debug.Log($"Gamepad removido de Player2");
                }
                else if (player1Devices.Contains(device))
                {
                    player1Devices.Remove(device);
                    if (device == player1Gamepad)
                    {
                        player1Script.SetUIType(false);
                    }
                    SetupDeviceFilters();
                    Debug.Log($"Gamepad removido de Player1");
                }
                break;
        }
    }

    private bool AreGamepadsSame(Gamepad a, Gamepad b)
    {
        return a.description.manufacturer == b.description.manufacturer &&
               a.description.product == b.description.product &&
               a.description.serial == b.description.serial;
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
}