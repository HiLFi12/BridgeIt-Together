using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Transform objetivo;
    [SerializeField] private Transform objetivo2;
    [SerializeField] private bool seguirDosJugadores = false;

    [Header("Posición")]
    [SerializeField] private Vector3 offset = new Vector3(10f, 10f, -10f); 
    [SerializeField] private bool seguirRotacionY = false;          
    [SerializeField] private float alturaMinima = 1.0f;
    [SerializeField] private float distanciaExtra = 2.0f; // Distancia extra para cuando sigue a dos jugadores

    [Header("Suavizado")]
    [SerializeField] private bool usarSuavizado = true;             
    [SerializeField] private float velocidadSuavizado = 5.0f;       

    [Header("Limites de Cámara")]
    [SerializeField] private bool usarLimites = false;               
    [SerializeField] private Vector2 limitesX = new Vector2(-50f, 50f); 
    [SerializeField] private Vector2 limitesZ = new Vector2(-50f, 50f); 

    [Header("Colisiones")]
    [SerializeField] private bool detectarColisiones = false;        
    [SerializeField] private float distanciaDeteccion = 5.0f;        
    [SerializeField] private LayerMask capasColision;                

    private Vector3 posicionObjetivo;
    private Vector3 posicionSuavizada;
    private float distanciaAlObjetivo;
    private Vector3 direccionAlObjetivo;
    private Vector3 puntoMedio;

    private void Start()
    {
        if (objetivo == null)
        {
            Debug.LogWarning("CameraFollow: No hay objetivo asignado. Asigna un objetivo en el Inspector.");
            enabled = false; 
            return;
        }

        if (seguirDosJugadores && objetivo2 == null)
        {
            Debug.LogWarning("CameraFollow: Se activó seguirDosJugadores pero falta el segundo objetivo. La cámara seguirá solo al primer objetivo.");
            seguirDosJugadores = false;
        }

        distanciaAlObjetivo = offset.magnitude;

        posicionObjetivo = CalcularPosicionObjetivo();
        transform.position = posicionObjetivo;

        ActualizarPuntoMira();
    }

    private void LateUpdate()
    {
        if (objetivo == null) return;
        if (seguirDosJugadores && objetivo2 == null) seguirDosJugadores = false;

        posicionObjetivo = CalcularPosicionObjetivo();

        if (usarLimites)
        {
            posicionObjetivo.x = Mathf.Clamp(posicionObjetivo.x, limitesX.x, limitesX.y);
            posicionObjetivo.z = Mathf.Clamp(posicionObjetivo.z, limitesZ.x, limitesZ.y);
        }

        if (detectarColisiones)
        {
            direccionAlObjetivo = (puntoMedio - posicionObjetivo).normalized;
            AjustarPorColisiones();
        }

        if (usarSuavizado)
        {
            transform.position = Vector3.Lerp(transform.position, posicionObjetivo,
                velocidadSuavizado * Time.deltaTime);
        }
        else
        {
            transform.position = posicionObjetivo;
        }

        if (transform.position.y < alturaMinima)
        {
            Vector3 pos = transform.position;
            pos.y = alturaMinima;
            transform.position = pos;
        }

        ActualizarPuntoMira();
    }

    private void ActualizarPuntoMira()
    {
        if (seguirDosJugadores && objetivo2 != null)
        {
            puntoMedio = (objetivo.position + objetivo2.position) / 2f;
            transform.LookAt(puntoMedio);
        }
        else
        {
            puntoMedio = objetivo.position;
            transform.LookAt(objetivo);
        }
    }

    private Vector3 CalcularPosicionObjetivo()
    {
        Vector3 targetPos;
        Vector3 targetOffset = offset;

        // Calcular el punto medio y ajustar el offset basado en la distancia entre los jugadores
        if (seguirDosJugadores && objetivo2 != null)
        {
            puntoMedio = (objetivo.position + objetivo2.position) / 2f;
            
            // Ajustar la distancia de la cámara basada en la separación entre jugadores
            float distanciaEntreJugadores = Vector3.Distance(objetivo.position, objetivo2.position);
            float factorEscala = 1.0f + (distanciaEntreJugadores * 0.1f);
            factorEscala = Mathf.Clamp(factorEscala, 1.0f, 2.5f); // Limitar el zoom out
            
            targetOffset *= factorEscala;
            
            if (seguirRotacionY)
            {
                // Usar la rotación promedio de ambos jugadores o la rotación del punto medio
                float anguloY = (objetivo.eulerAngles.y + objetivo2.eulerAngles.y) / 2f;
                Quaternion rotacion = Quaternion.Euler(0, anguloY, 0);
                Vector3 offsetRotado = rotacion * targetOffset;

                targetPos = puntoMedio + offsetRotado;
            }
            else
            {
                targetPos = puntoMedio + targetOffset;
            }
        }
        else
        {
            if (seguirRotacionY)
            {
                float anguloY = objetivo.eulerAngles.y;
                Quaternion rotacion = Quaternion.Euler(0, anguloY, 0);
                Vector3 offsetRotado = rotacion * targetOffset;

                targetPos = objetivo.position + offsetRotado;
            }
            else
            {
                targetPos = objetivo.position + targetOffset;
            }
        }
        return targetPos;
    }

    private void AjustarPorColisiones()
    {
        RaycastHit hit;
        Vector3 inicioRay = puntoMedio;
        Vector3 direccionRay = -direccionAlObjetivo; 

        if (Physics.Raycast(inicioRay, direccionRay, out hit, distanciaAlObjetivo, capasColision))
        {
            float distanciaAjustada = hit.distance * 0.8f; 
            posicionObjetivo = puntoMedio - direccionRay * distanciaAjustada;

            Debug.DrawLine(inicioRay, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(inicioRay, inicioRay + direccionRay * distanciaAlObjetivo, Color.green);
        }
    }

    public void CambiarObjetivos(Transform nuevoObjetivo1, Transform nuevoObjetivo2 = null)
    {
        if (nuevoObjetivo1 != null)
        {
            objetivo = nuevoObjetivo1;
        }
        
        if (nuevoObjetivo2 != null)
        {
            objetivo2 = nuevoObjetivo2;
            seguirDosJugadores = true;
        }
        else
        {
            seguirDosJugadores = false;
        }
    }

    public void AjustarOffset(Vector3 nuevoOffset)
    {
        offset = nuevoOffset;
        distanciaAlObjetivo = offset.magnitude;
    }

    public void AjustarVelocidadSuavizado(float nuevaVelocidad)
    {
        velocidadSuavizado = Mathf.Max(0.1f, nuevaVelocidad);
    }

    private void OnDrawGizmosSelected()
    {
        if (objetivo == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(objetivo.position, 1f);
        
        if (seguirDosJugadores && objetivo2 != null)
        {
            Gizmos.DrawWireSphere(objetivo2.position, 1f);
            
            // Dibujar punto medio
            Gizmos.color = Color.magenta;
            Vector3 puntoMedioGizmo = (objetivo.position + objetivo2.position) / 2f;
            Gizmos.DrawWireSphere(puntoMedioGizmo, 0.8f);
            Gizmos.DrawLine(objetivo.position, objetivo2.position);
        }

        Gizmos.color = Color.cyan;
        Vector3 posObjetivo = seguirDosJugadores && objetivo2 != null ? 
            (objetivo.position + objetivo2.position) / 2f : objetivo.position;
        Gizmos.DrawLine(posObjetivo, posObjetivo + offset);
        Gizmos.DrawWireSphere(posObjetivo + offset, 0.5f);

        if (usarLimites)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((limitesX.x + limitesX.y) / 2, transform.position.y,
                (limitesZ.x + limitesZ.y) / 2);
            Vector3 size = new Vector3(limitesX.y - limitesX.x, 2, limitesZ.y - limitesZ.x);
            Gizmos.DrawWireCube(center, size);
        }
    }
}