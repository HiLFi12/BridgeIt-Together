using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class PaloIgnifugo : MonoBehaviour, IHitable, IUIActivatable
{
    [Header("Configuración")]
    [SerializeField] private GameObject efectoFuego;
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