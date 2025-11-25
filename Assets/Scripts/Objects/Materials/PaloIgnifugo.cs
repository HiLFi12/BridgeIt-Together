using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class PaloIgnifugo : MonoBehaviour, IHitable, IUIActivatable
{
    [Header("Configuración")]
    [SerializeField] private GameObject efectoFuego;
    [SerializeField] private Transform fuegoSpawnPoint;
    [SerializeField] private float tiempoEncendido = 9999f;

    [Header("UI Configuration")]
    [SerializeField] private int turnedOffIndex = 3;
    [SerializeField] private int turnedOnIndex = 4;
    private PlayerUIManager playerUIManager;

    private bool estaEncendido = false;
    private float tiempoRestante = 0f;

    public int UIIndex { get; private set; }

    private void Start()
    {
        playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        // Recolocar el efecto de fuego en el spawn point si está asignado
        if (efectoFuego != null && fuegoSpawnPoint != null)
        {
            efectoFuego.transform.SetParent(fuegoSpawnPoint, false);
            efectoFuego.transform.localPosition = Vector3.zero;
            efectoFuego.transform.localRotation = Quaternion.identity;
        }
        SetEncendido(false);
    }

    public void SetEncendido(bool encendido)
    {
        estaEncendido = encendido;
        if (efectoFuego != null)
        {
            efectoFuego.SetActive(encendido);
        }
        tiempoRestante = encendido ? tiempoEncendido : 0f;
        int index = encendido ? turnedOnIndex : turnedOffIndex;
        SetUIIndex(index);
        if (playerUIManager != null)
        {
            playerUIManager.RefreshHeldObjectUI(index);
        }
    }

    public void SetUIIndex(int index)
    {
        if (playerUIManager != null)
        {
            playerUIManager.RefreshHeldObjectUI(index);
        }
        UIIndex = index;
    }

    public bool EstaEncendido()
    {
        return estaEncendido;
    }

    public void OnLaunched(Vector3 targetPosition)
    {
    }
}