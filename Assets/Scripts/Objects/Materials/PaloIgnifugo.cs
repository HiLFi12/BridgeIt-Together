using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class PaloIgnifugo : MonoBehaviour, IHitable
{
    [Header("Configuración")]
    [SerializeField] private float radioDeteccion = 2.0f;
    [SerializeField] private GameObject efectoFuego;
    [SerializeField] private float tiempoEncendido = 9999f;

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
}