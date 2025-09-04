using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestor de interacciones que permite detectar cuando se mantiene presionado un botón
/// y notificar a los objetos interactuables correspondientes.
/// </summary>
public class InteractionManager : MonoBehaviour
{
    [Header("Configuración de interacción")]
    [SerializeField] private float radioInteraccion = 2.0f;
    [SerializeField] private LayerMask capaInteractuable;
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E;
    
    private GameObject objetoSeleccionado;
    private IHoldInteractable objetoConInteraccionProlongada;
    private bool interaccionEnProceso = false;
    
    private void Update()
    {
        // Detectar objetos interactuables cercanos
        DetectarObjetosInteractuables();
        
        // Manejar interacción al presionar la tecla
        if (Input.GetKeyDown(teclaInteraccion))
        {
            IniciarInteraccion();
        }
        
        // Manejar interacción al soltar la tecla
        if (Input.GetKeyUp(teclaInteraccion))
        {
            DetenerInteraccion();
        }
    }
    
    private void DetectarObjetosInteractuables()
    {
        // Si ya estamos en proceso de interacción, no cambiamos el objeto seleccionado
        if (interaccionEnProceso) return;
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, radioInteraccion, capaInteractuable);
        
        GameObject mejorObjeto = null;
        InteractPriority mejorPrioridad = InteractPriority.VeryLow;
        float distanciaMasCercana = float.MaxValue;
        
        foreach (Collider col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distancia = Vector3.Distance(transform.position, col.transform.position);
                
                // Si tiene mayor prioridad o igual prioridad pero está más cerca
                if (interactable.InteractPriority > mejorPrioridad || 
                    (interactable.InteractPriority == mejorPrioridad && distancia < distanciaMasCercana))
                {
                    mejorPrioridad = interactable.InteractPriority;
                    distanciaMasCercana = distancia;
                    mejorObjeto = col.gameObject;
                }
            }
        }
        
        // Actualizar el objeto seleccionado
        objetoSeleccionado = mejorObjeto;
        
        // Opcional: Mostrar algún indicador visual sobre el objeto seleccionado
        // MostrarIndicadorVisual();
    }
    
    private void IniciarInteraccion()
    {
        if (objetoSeleccionado == null) return;
        
        // Intentar obtener un interactuable de tipo "hold"
        objetoConInteraccionProlongada = objetoSeleccionado.GetComponent<IHoldInteractable>();
        
        // Obtener el componente IInteractable
        IInteractable interactable = objetoSeleccionado.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interaccionEnProceso = true;
            interactable.Interact(gameObject);
        }
    }
    
    private void DetenerInteraccion()
    {
        // Si estábamos interactuando con un objeto que requiere mantener presionado, notificarle que se detuvo la interacción
        if (objetoConInteraccionProlongada != null)
        {
            objetoConInteraccionProlongada.DetenerInteraccion();
            objetoConInteraccionProlongada = null;
        }
        
        interaccionEnProceso = false;
    }
    
    // Método para dibujar el radio de interacción en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}