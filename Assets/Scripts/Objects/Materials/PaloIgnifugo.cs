using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

/// <summary>
/// Palo ignífugo, objeto único de la era prehistórica.
/// Puede encenderse al acercarse a una fogata y luego usarse para obtener resina de la corteza.
/// </summary>
public class PaloIgnifugo : MonoBehaviour, ICollidableNT
{
    [Header("Configuración")]
    [SerializeField] private float radioDeteccion = 2.0f;
    [SerializeField] private GameObject efectoFuegoPrefab; // Efecto visual de fuego
    [SerializeField] private float tiempoEncendido = 10.0f; // Tiempo que permanece encendido
    
    private bool estaEncendido = false;
    private GameObject efectoFuego = null;
    private float tiempoRestante = 0f;
    
    private void Start()
    {
        // Inicialmente el palo no está encendido
        SetEncendido(false);
    }
    
    private void Update()
    {
        // Detectar fogatas cercanas para encenderse
        if (!estaEncendido)
        {
            DetectarFogatas();
        }
        else
        {
            // Si está encendido, controlar el tiempo restante
            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante <= 0)
            {
                // Se apaga cuando se acaba el tiempo
                SetEncendido(false);
                Debug.Log("El palo ignífugo se ha apagado.");
            }
        }
    }
    
    // Detecta fogatas cercanas para encender el palo
    private void DetectarFogatas()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radioDeteccion);
        
        foreach (Collider col in colliders)
        {
            // Verificar si es una fogata (GenericObject2 de la era prehistórica)
            GenericObject2 fogata = col.GetComponent<GenericObject2>();
            if (fogata != null)
            {
                // Verificar que sea de la era prehistórica
                if (fogata.GetEra() == BridgeQuadrantSO.EraType.Prehistoric)
                {
                    SetEncendido(true);
                    Debug.Log("¡Palo ignífugo encendido!");
                    break;
                }
            }
        }
    }
    
    // Establece el estado de encendido del palo
    public void SetEncendido(bool encendido)
    {
        estaEncendido = encendido;
        
        if (encendido)
        {
            // Activar efecto visual de fuego
            if (efectoFuegoPrefab != null && efectoFuego == null)
            {
                efectoFuego = Instantiate(efectoFuegoPrefab, transform);
                efectoFuego.transform.localPosition = Vector3.zero;
            }
            else if (efectoFuego != null)
            {
                efectoFuego.SetActive(true);
            }
            
            tiempoRestante = tiempoEncendido;
        }
        else
        {
            // Desactivar efecto visual de fuego
            if (efectoFuego != null)
            {
                efectoFuego.SetActive(false);
            }
            
            tiempoRestante = 0f;
        }
    }
    
    // Verifica si el palo está encendido
    public bool EstaEncendido()
    {
        return estaEncendido;
    }
    
    // Método para dibujar el radio de detección en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
} 