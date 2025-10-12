using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Configuración de Teleporter")]
    [SerializeField] private Transform destinoTeleport;
    [SerializeField] private Teleporter teleporterDestino; // Referencia al otro teleportador
    [SerializeField] private float cooldownTime = 3f;
    [SerializeField] private BatterySystem batterySystem;
    
    [Header("Sistema Visual")]
    [SerializeField] private GameObject visualReferenceCargado;
    [SerializeField] private GameObject visualReferenceCargado2;
    [SerializeField] private GameObject visualReferenceDescargado;
    [SerializeField] private Renderer[] objetosACambiarEmision;
    [SerializeField] private Color colorEmisionCargado = Color.blue;
    [SerializeField] private Color colorEmisionDescargado = Color.red;
    [SerializeField] private float intensidadEmisionCargado = 2f;
    [SerializeField] private float intensidadEmisionDescargado = 1f;
    [SerializeField] private float velocidadCambioColor = 2f;
    
    [Header("Emisión de Cooldown")]
    [SerializeField] private Renderer materialCooldown;
    [SerializeField] private Renderer materialCooldownDestino; // Renderer del material del otro teleportador
    [SerializeField] private Color colorEmisionCooldown = Color.cyan;
    [SerializeField] private float intensidadMaximaCooldown = 5f;
    [SerializeField] private float intensidadMinimaCooldown = 0f;
    [SerializeField] private float tiempoEfectoDuplicacion = 0.1f; // Tiempo que dura el efecto de duplicación
    
    [Header("Cooldown de Reutilización")]
    [SerializeField] private float tiempoEntreUsos = 2f; // Tiempo que debe esperar antes de poder usarse de nuevo
    
    private float cooldownActual;
    private bool cooldownActivo = false;
    private HashSet<GameObject> objetosDentro = new HashSet<GameObject>();
    
    private Color[] coloresEmisionActuales;
    private Color colorEmisionObjetivo;
    private float intensidadEmisionActual;
    private float intensidadEmisionObjetivo;
    private Material[][] materialesInstanciados;
    private Material materialCooldownInstanciado;
    private Material materialCooldownDestinoInstanciado;
    
    private bool esperandoTeletransporte = false;
    private float tiempoEsperaTeletransporte = 0f;
    
    // Sistema de cooldown individual para cada teleportador
    private float cooldownReutilizacion = 0f;

    private void Start()
    {
        // Verificar que el collider sea trigger
        Collider col = GetComponent<Collider>();
        
        cooldownActual = cooldownTime;
        
        // Inicializar materiales de emisión
        if (objetosACambiarEmision != null && objetosACambiarEmision.Length > 0)
        {
            coloresEmisionActuales = new Color[objetosACambiarEmision.Length];
            materialesInstanciados = new Material[objetosACambiarEmision.Length][];
            
            for (int i = 0; i < objetosACambiarEmision.Length; i++)
            {
                if (objetosACambiarEmision[i] != null)
                {
                    Material[] mats = objetosACambiarEmision[i].materials;
                    materialesInstanciados[i] = new Material[mats.Length];
                    
                    for (int j = 0; j < mats.Length; j++)
                    {
                        materialesInstanciados[i][j] = new Material(mats[j]);
                    }
                    
                    objetosACambiarEmision[i].materials = materialesInstanciados[i];
                    
                    if (materialesInstanciados[i].Length > 0 && materialesInstanciados[i][0].HasProperty("_EmissionColor"))
                    {
                        coloresEmisionActuales[i] = materialesInstanciados[i][0].GetColor("_EmissionColor");
                    }
                    else
                    {
                        coloresEmisionActuales[i] = Color.black;
                    }
                }
            }
        }
        
        // Crear instancia propia del material de cooldown
        if (materialCooldown != null)
        {
            materialCooldownInstanciado = new Material(materialCooldown.material);
            materialCooldown.material = materialCooldownInstanciado;
            
            // Iniciar con emisión mínima
            if (materialCooldownInstanciado.HasProperty("_EmissionColor"))
            {
                materialCooldownInstanciado.SetColor("_EmissionColor", colorEmisionCooldown * intensidadMinimaCooldown);
                materialCooldownInstanciado.DisableKeyword("_EMISSION");
            }
        }
        
        // Crear instancia del material de cooldown destino
        if (materialCooldownDestino != null)
        {
            materialCooldownDestinoInstanciado = new Material(materialCooldownDestino.material);
            materialCooldownDestino.material = materialCooldownDestinoInstanciado;
            
            // Iniciar con emisión mínima
            if (materialCooldownDestinoInstanciado.HasProperty("_EmissionColor"))
            {
                materialCooldownDestinoInstanciado.SetColor("_EmissionColor", colorEmisionCooldown * intensidadMinimaCooldown);
                materialCooldownDestinoInstanciado.DisableKeyword("_EMISSION");
            }
        }
    }
    
    private void Update()
    {
        // Actualizar visual según el estado de la batería
        ActualizarVisual();
        
        // Actualizar emisión del material de cooldown
        ActualizarEmisionCooldown();
        
        // Actualizar cooldown de reutilización
        if (cooldownReutilizacion > 0f)
        {
            cooldownReutilizacion -= Time.deltaTime;
            
            if (cooldownReutilizacion <= 0f)
            {
                cooldownReutilizacion = 0f;
                Debug.Log($"[Teleporter] {gameObject.name} - Cooldown de reutilización completado.");
            }
        }
        
        // Si estamos esperando para teletransportar (efecto de duplicación activo)
        if (esperandoTeletransporte)
        {
            tiempoEsperaTeletransporte -= Time.deltaTime;
            
            if (tiempoEsperaTeletransporte <= 0f)
            {
                // Ejecutar la teletransportación después del efecto
                EjecutarTeletransportacion();
                esperandoTeletransporte = false;
            }
            return;
        }
        
        // Si hay objetos dentro, verificar batería y activar el cooldown
        if (objetosDentro.Count > 0)
        {
            // Verificar si está en cooldown de reutilización
            if (cooldownReutilizacion > 0f)
            {
                // No hacer nada si está en cooldown de reutilización
                if (cooldownActivo)
                {
                    Debug.Log($"[Teleporter] {gameObject.name} en cooldown de reutilización, pausando proceso.");
                    cooldownActivo = false;
                }
                return;
            }
            
            // Solo funcionar si la batería está cargada
            if (!TieneBateria())
            {
                // Si no hay batería, no hacer nada (mantener objetos en espera)
                if (cooldownActivo)
                {
                    Debug.Log("[Teleporter] Batería descargada, pausando cooldown.");
                    cooldownActivo = false;
                }
                return;
            }
            
            if (!cooldownActivo)
            {
                cooldownActivo = true;
                Debug.Log("[Teleporter] Cooldown iniciado con batería activa.");
            }
            
            // Decrementar el cooldown
            cooldownActual -= Time.deltaTime;
            
            // Cuando el cooldown llega a 0, activar efecto de duplicación
            if (cooldownActual <= 0f)
            {
                IniciarEfectoDuplicacion();
            }
        }
        else
        {
            // Si no hay objetos dentro, resetear el cooldown
            if (cooldownActivo)
            {
                ResetearCooldown();
            }
        }
    }

    private bool TieneBateria()
    {
        return batterySystem != null && batterySystem.IsCharged;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Agregar el objeto a la lista de objetos dentro
        GameObject obj = other.gameObject;
        
        // Verificar si el objeto es un hijo de un jugador (objeto en la mano)
        if (EsObjetoEnManoDeJugador(obj))
        {
            Debug.Log($"[Teleporter] '{obj.name}' es un objeto en la mano de un jugador, no se agregará a la lista.");
            return;
        }
        
        if (!objetosDentro.Contains(obj))
        {
            objetosDentro.Add(obj);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remover el objeto de la lista
        GameObject obj = other.gameObject;
        
        if (objetosDentro.Contains(obj))
        {
            objetosDentro.Remove(obj);
        }
    }
    
    private bool EsObjetoEnManoDeJugador(GameObject obj)
    {
        if (obj == null) return false;
        
        // Recorrer los padres del objeto para ver si alguno tiene PlayerObjectHolder
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            PlayerObjectHolder holder = parent.GetComponent<PlayerObjectHolder>();
            if (holder != null && holder.HasObjectInHand() && holder.GetHeldObject() == obj)
            {
                return true;
            }
            parent = parent.parent;
        }
        
        return false;
    }

    private void TeletransportarTodos()
    {
        Debug.Log($"[Teleporter] TeletransportarTodos() llamado. Objetos dentro: {objetosDentro.Count}");
        
        // Verificar que haya batería antes de teletransportar
        if (!TieneBateria())
        {
            Debug.LogWarning("[Teleporter] No se puede teletransportar: la batería no está cargada.");
            ResetearCooldown();
            return;
        }
        
        if (destinoTeleport == null)
        {
            Debug.LogError("[Teleporter] No se puede teletransportar: no hay destino asignado.");
            ResetearCooldown();
            return;
        }
        
        // Crear un nuevo Vector3 con la posición del destino
        Vector3 posicionDestino = new Vector3(destinoTeleport.position.x, destinoTeleport.position.y, destinoTeleport.position.z);
        
        Debug.Log($"[Teleporter] Destino válido: {destinoTeleport.name} en posición {posicionDestino}");
        
        // Crear una copia de la lista para evitar problemas al modificar durante la iteración
        List<GameObject> objetosATeletransportar = new List<GameObject>(objetosDentro);
        
        Debug.Log($"[Teleporter] Intentando teletransportar {objetosATeletransportar.Count} objeto(s)...");
        
        foreach (GameObject obj in objetosATeletransportar)
        {
            if (obj != null)
            {
                Vector3 posicionAnterior = obj.transform.position;
                
                // Verificar si tiene CharacterController (requiere tratamiento especial)
                CharacterController charController = obj.GetComponent<CharacterController>();
                if (charController != null)
                {
                    // Desactivar temporalmente para poder mover
                    charController.enabled = false;
                    obj.transform.position = posicionDestino;
                    charController.enabled = true;
                    Debug.Log($"[Teleporter] '{obj.name}' (con CharacterController) teletransportado de {posicionAnterior} a {obj.transform.position}");
                }
                else
                {
                    // Teletransportar al destino usando new Vector3
                    obj.transform.position = posicionDestino;
                    Debug.Log($"[Teleporter] '{obj.name}' teletransportado de {posicionAnterior} a {obj.transform.position}");
                }
                
                // Si tiene Rigidbody, resetear velocidad
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
#if UNITY_6000_0_OR_NEWER
                    rb.linearVelocity = Vector3.zero;
#else
                    rb.velocity = Vector3.zero;
#endif
                    rb.angularVelocity = Vector3.zero;
                    Debug.Log($"[Teleporter] Velocidades del Rigidbody de '{obj.name}' reseteadas");
                }
            }
            else
            {
                Debug.LogWarning("[Teleporter] Un objeto en la lista era null, omitiendo...");
            }
        }
        
        Debug.Log("[Teleporter] Teletransportación completada. Limpiando lista y reseteando cooldown.");
        
        // Limpiar la lista y resetear el cooldown
        objetosDentro.Clear();
        ResetearCooldown();
        
        // Activar cooldown de reutilización en este teleportador
        cooldownReutilizacion = tiempoEntreUsos;
        Debug.Log($"[Teleporter] {gameObject.name} - Cooldown de reutilización iniciado por {tiempoEntreUsos} segundos.");
        
        // Activar cooldown de reutilización en el teleportador destino
        if (teleporterDestino != null)
        {
            teleporterDestino.ActivarCooldownReutilizacion(tiempoEntreUsos);
            Debug.Log($"[Teleporter] Cooldown de reutilización activado en teleportador destino: {teleporterDestino.gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[Teleporter] {gameObject.name} - No se pudo activar cooldown en teleportador destino: referencia no asignada.");
        }
    }
    
    private void ResetearCooldown()
    {
        cooldownActual = cooldownTime;
        cooldownActivo = false;
        esperandoTeletransporte = false;
    }
    
    // Método público para obtener el tiempo restante (útil para UI o debug)
    public float GetTiempoRestante()
    {
        return cooldownActual;
    }
    
    // Método público para obtener el progreso del cooldown (0 a 1)
    public float GetProgresoCooldown()
    {
        return cooldownTime > 0 ? (cooldownTime - cooldownActual) / cooldownTime : 0f;
    }
    
    // Método público para verificar si el cooldown está activo
    public bool IsCooldownActivo()
    {
        return cooldownActivo;
    }
    
    // Método público para obtener la cantidad de objetos dentro
    public int GetCantidadObjetosDentro()
    {
        return objetosDentro.Count;
    }
    
    private void ActualizarVisual()
    {
        if (batterySystem == null) return;

        bool isCharged = batterySystem.IsCharged;

        // Activar/desactivar visuales según el estado de carga
        if (visualReferenceCargado != null)
        {
            visualReferenceCargado.SetActive(isCharged);
        }
        
        if (visualReferenceCargado2 != null)
        {
            visualReferenceCargado2.SetActive(isCharged);
        }
        
        if (visualReferenceDescargado != null)
        {
            visualReferenceDescargado.SetActive(!isCharged);
        }

        // Cambiar emisión de materiales progresivamente
        if (objetosACambiarEmision != null && objetosACambiarEmision.Length > 0)
        {
            colorEmisionObjetivo = isCharged ? colorEmisionCargado : colorEmisionDescargado;
            intensidadEmisionObjetivo = isCharged ? intensidadEmisionCargado : intensidadEmisionDescargado;

            for (int i = 0; i < objetosACambiarEmision.Length; i++)
            {
                if (objetosACambiarEmision[i] != null && materialesInstanciados[i] != null)
                {
                    coloresEmisionActuales[i] = Color.Lerp(coloresEmisionActuales[i], colorEmisionObjetivo, Time.deltaTime * velocidadCambioColor);
                    intensidadEmisionActual = Mathf.Lerp(intensidadEmisionActual, intensidadEmisionObjetivo, Time.deltaTime * velocidadCambioColor);
                    
                    foreach (Material mat in materialesInstanciados[i])
                    {
                        if (mat != null && mat.HasProperty("_EmissionColor"))
                        {
                            mat.SetColor("_EmissionColor", coloresEmisionActuales[i] * intensidadEmisionActual);
                            
                            if (coloresEmisionActuales[i].maxColorComponent > 0.01f)
                            {
                                mat.EnableKeyword("_EMISSION");
                            }
                            else
                            {
                                mat.DisableKeyword("_EMISSION");
                            }
                        }
                    }
                }
            }
        }
    }

    private void ActualizarEmisionCooldown()
    {
        // Calcular el progreso del cooldown (0 = inicio, 1 = listo para teletransportar)
        float progreso = cooldownActivo ? (cooldownTime - cooldownActual) / cooldownTime : 0f;
        
        // La intensidad es inversamente proporcional al cooldown (más progreso = más brillo)
        float intensidadActual = Mathf.Lerp(intensidadMinimaCooldown, intensidadMaximaCooldown, progreso);
        
        // Si estamos en efecto de duplicación, duplicar la intensidad
        if (esperandoTeletransporte)
        {
            intensidadActual *= 2f;
        }
        
        Color colorEmision = colorEmisionCooldown * intensidadActual;
        
        // Aplicar el color de emisión al material propio
        if (materialCooldownInstanciado != null && materialCooldownInstanciado.HasProperty("_EmissionColor"))
        {
            materialCooldownInstanciado.SetColor("_EmissionColor", colorEmision);
            
            if (intensidadActual > 0.01f)
            {
                materialCooldownInstanciado.EnableKeyword("_EMISSION");
            }
            else
            {
                materialCooldownInstanciado.DisableKeyword("_EMISSION");
            }
        }
        
        // Aplicar el color de emisión al material de destino
        if (materialCooldownDestinoInstanciado != null && materialCooldownDestinoInstanciado.HasProperty("_EmissionColor"))
        {
            materialCooldownDestinoInstanciado.SetColor("_EmissionColor", colorEmision);
            
            if (intensidadActual > 0.01f)
            {
                materialCooldownDestinoInstanciado.EnableKeyword("_EMISSION");
            }
            else
            {
                materialCooldownDestinoInstanciado.DisableKeyword("_EMISSION");
            }
        }
    }

    private void IniciarEfectoDuplicacion()
    {
        Debug.Log("[Teleporter] Iniciando efecto de duplicación de emisión.");
        
        // Iniciar el timer para esperar antes de teletransportar
        tiempoEsperaTeletransporte = tiempoEfectoDuplicacion;
        esperandoTeletransporte = true;
    }

    private void EjecutarTeletransportacion()
    {
        Debug.Log("[Teleporter] Ejecutando teletransportación.");
        
        // Teletransportar todos los objetos dentro al destino
        TeletransportarTodos();
    }
    
    // Método público para activar el cooldown de reutilización desde otro teleportador
    public void ActivarCooldownReutilizacion(float tiempo)
    {
        cooldownReutilizacion = tiempo;
        Debug.Log($"[Teleporter] {gameObject.name} - Cooldown de reutilización activado externamente por {tiempo} segundos.");
    }
    
    // Método público para verificar si está en cooldown de reutilización
    public bool EstaEnCooldownReutilizacion()
    {
        return cooldownReutilizacion > 0f;
    }
    
    // Método público para obtener el tiempo restante de cooldown de reutilización
    public float GetCooldownReutilizacionRestante()
    {
        return cooldownReutilizacion;
    }
}
