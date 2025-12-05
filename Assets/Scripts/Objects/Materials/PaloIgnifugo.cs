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

    private bool estaEncendido = false;
    private float tiempoRestante = 0f;

    public int UIIndex { get; private set; }

    private void Awake()
    {
        // Inicializar UIIndex ANTES de que cualquier sistema lo lea
        UIIndex = turnedOffIndex;
    }

    private void Start()
    {
        // Recolocar el efecto de fuego en el spawn point si está asignado
        if (efectoFuego != null && fuegoSpawnPoint != null)
        {
            efectoFuego.transform.SetParent(fuegoSpawnPoint, false);
            efectoFuego.transform.localPosition = Vector3.zero;
            efectoFuego.transform.localRotation = Quaternion.identity;
        }
        // Inicializar estado visual
        estaEncendido = false;
        if (efectoFuego != null)
        {
            efectoFuego.SetActive(false);
        }
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
        
        // Actualizar la UI del jugador que lo sostiene
        PlayerUIManager playerUIManager = GetHoldingPlayerUIManager();
        if (playerUIManager != null)
        {
            playerUIManager.RefreshHeldObjectUI(index);
        }
    }

    public void SetUIIndex(int index)
    {
        UIIndex = index;
        
        // Actualizar la UI del jugador que lo sostiene
        PlayerUIManager playerUIManager = GetHoldingPlayerUIManager();
        if (playerUIManager != null)
        {
            playerUIManager.RefreshHeldObjectUI(index);
        }
    }
    
    /// <summary>
    /// Obtiene el PlayerUIManager del jugador que actualmente está sosteniendo este objeto.
    /// </summary>
    private PlayerUIManager GetHoldingPlayerUIManager()
    {
        // Buscar al jugador que está sosteniendo este objeto
        PlayerObjectHolder[] holders = FindObjectsByType<PlayerObjectHolder>(FindObjectsSortMode.None);
        foreach (var holder in holders)
        {
            if (holder.GetHeldObject() == gameObject)
            {
                // Encontramos al jugador que sostiene este objeto
                return holder.GetComponent<PlayerUIManager>();
            }
        }
        return null;
    }

    public bool EstaEncendido()
    {
        return estaEncendido;
    }

    public void OnLaunched(Vector3 targetPosition)
    {
    }
}