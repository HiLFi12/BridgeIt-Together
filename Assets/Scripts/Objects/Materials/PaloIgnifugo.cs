using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class PaloIgnifugo : MonoBehaviour, IHitable, IUIActivatable
{
    [Header("Configuración")]
    [SerializeField] private float radioDeteccion = 2.0f;
    [SerializeField] private GameObject efectoFuego;
    [SerializeField] private float tiempoEncendido = 9999f;

    [Header("UI Configuration")]
    [SerializeField] private int uiIndexApagado = 0;
    [SerializeField] private int uiIndexEncendido = 1;
    private int uiIndex = 0;
    public int UIIndex => uiIndex;

    private bool estaEncendido = false;
    private float tiempoRestante = 0f;

    private void Start()
    {
        SetEncendido(false);
    }

    private void Update()
    {
        if (!estaEncendido)
        {
            DetectarFogatas();
        }
        else
        {
            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante <= 0)
            {
                SetEncendido(false);
                Debug.Log("El palo ignífugo se ha apagado.");
            }
        }
    }

    private void DetectarFogatas()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radioDeteccion);

        foreach (Collider col in colliders)
        {
            GenericObject2 fogata = col.GetComponent<GenericObject2>();
            if (fogata != null && fogata.GetEra() == BridgeQuadrantSO.EraType.Prehistoric)
            {
                SetEncendido(true);
                Debug.Log("¡Palo ignífugo encendido!");
                break;
            }
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

        // Cambiar el índice de UI según el estado
        uiIndex = encendido ? uiIndexEncendido : uiIndexApagado;
    }

    public bool EstaEncendido()
    {
        return estaEncendido;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }

    public void OnLaunched(Vector3 targetPosition)
    {
    }

    public void SetUIIndex(int index)
    {
        uiIndex = index;
    }
}