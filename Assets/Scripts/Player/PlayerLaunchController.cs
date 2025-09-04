using System.Collections;
using UnityEngine;

/// <summary>
/// Controla el efecto de lanzamiento de jugadores cuando son golpeados por vehículos
/// </summary>
public class PlayerLaunchController : MonoBehaviour
{
    private bool estaSiendoLanzado = false;
    private Vector3 posicionInicial;
    private Vector3 posicionDestino;
    private float alturaMaxima;
    private float duracionVuelo;
    private AnimationCurve curvaVuelo;
    
    private CharacterController characterController;
    private PlayerController playerController;
    private WalkableSurfaceDetector surfaceDetector;
    
    // Referencias para restaurar el control del jugador
    private bool controlOriginalHabilitado = true;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        surfaceDetector = GetComponent<WalkableSurfaceDetector>();
        
        // Verificar si hay Rigidbody y manejarlo apropiadamente
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.LogWarning($"PlayerLaunchController detectó Rigidbody en {gameObject.name}. Esto puede causar conflictos con CharacterController.");
        }
    }
    
    /// <summary>
    /// Verifica si el jugador está actualmente siendo lanzado
    /// </summary>
    public bool EstaSiendoLanzado()
    {
        return estaSiendoLanzado;
    }
    
    /// <summary>
    /// Lanza al jugador hacia una posición específica
    /// </summary>
    public void LanzarJugador(Vector3 destino, float altura, float duracion, AnimationCurve curva)
    {
        if (estaSiendoLanzado) 
        {
            Debug.Log($"❌ {gameObject.name} ya está siendo lanzado, ignorando nueva solicitud");
            return; // Ya está siendo lanzado
        }
        
        Debug.Log($"🚀 INICIANDO LANZAMIENTO DE {gameObject.name} hacia {destino}");
        
        posicionInicial = transform.position;
        posicionDestino = destino;
        alturaMaxima = altura;
        duracionVuelo = duracion;
        curvaVuelo = curva ?? AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        StartCoroutine(EjecutarLanzamiento());
    }
    
    private IEnumerator EjecutarLanzamiento()
    {
        estaSiendoLanzado = true;
        
        // Desactivar controles del jugador
        DesactivarControles();
        
        float tiempoTranscurrido = 0f;
        
        while (tiempoTranscurrido < duracionVuelo)
        {
            float progreso = tiempoTranscurrido / duracionVuelo;
            
            // Interpolación horizontal suave
            Vector3 posicionHorizontal = Vector3.Lerp(posicionInicial, posicionDestino, progreso);
            
            // Crear una parábola más suave y menos dramática para el vuelo
            float alturaParabola = Mathf.Sin(progreso * Mathf.PI) * 0.8f; // Reducir la intensidad de la parábola
            float alturaActual = posicionInicial.y + (alturaMaxima * alturaParabola);
            
            Vector3 nuevaPosicion = new Vector3(posicionHorizontal.x, alturaActual, posicionHorizontal.z);
            
            // Usar CharacterController.Move para mantener las colisiones
            if (characterController != null && characterController.enabled)
            {
                Vector3 movimiento = nuevaPosicion - transform.position;
                characterController.Move(movimiento);
            }
            else
            {
                // Fallback: movimiento directo solo si no hay CharacterController
                transform.position = nuevaPosicion;
            }
            
            tiempoTranscurrido += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        // Asegurar que llegue cerca del destino usando CharacterController
        Vector3 posicionFinal = new Vector3(posicionDestino.x, posicionDestino.y, posicionDestino.z);
        
        if (characterController != null && characterController.enabled)
        {
            Vector3 movimientoFinal = posicionFinal - transform.position;
            characterController.Move(movimientoFinal);
        }
        else
        {
            transform.position = posicionFinal;
        }
        
        // Efecto de aterrizaje
        StartCoroutine(EfectoAterrizaje());
    }
    
    private IEnumerator EfectoAterrizaje()
    {
        // Esperar un momento antes de restaurar controles
        yield return new WaitForSeconds(0.1f);
        
        // Reactivar controles
        ReactivarControles();
        
        // Actualizar la posición segura en el detector de superficie
        if (surfaceDetector != null)
        {
            surfaceDetector.ActualizarPosicionSegura(transform.position);
        }
        
        estaSiendoLanzado = false;
    }
    
    private void DesactivarControles()
    {
        // Desactivar el PlayerController temporalmente
        if (playerController != null)
        {
            controlOriginalHabilitado = playerController.enabled;
            playerController.enabled = false;
        }
        
        // Mantener el CharacterController activo para conservar las colisiones
        // pero desactivar el WalkableSurfaceDetector para evitar reposicionamientos automáticos
        if (surfaceDetector != null)
        {
            surfaceDetector.SetRepositioning(true);
        }
    }
    
    private void ReactivarControles()
    {
        // Reactivar el PlayerController
        if (playerController != null && controlOriginalHabilitado)
        {
            playerController.enabled = true;
        }
        
        // Reactivar el WalkableSurfaceDetector
        if (surfaceDetector != null)
        {
            surfaceDetector.SetRepositioning(false);
        }
        
        // Asegurar que el CharacterController esté activo (debería estarlo ya)
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
    
    /// <summary>
    /// Método para cancelar el lanzamiento en caso de emergencia
    /// </summary>
    public void CancelarLanzamiento()
    {
        if (estaSiendoLanzado)
        {
            StopAllCoroutines();
            ReactivarControles();
            estaSiendoLanzado = false;
            Debug.Log($"Lanzamiento de {gameObject.name} cancelado");
        }
    }
    
    void OnDestroy()
    {
        // Asegurar que se reactiven los controles si el componente es destruido
        if (estaSiendoLanzado)
        {
            CancelarLanzamiento();
        }
    }
}
