using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Maneja las colisiones entre vehículos y jugadores, aplicando el efecto de "salir volando"
/// SISTEMA PRINCIPAL: Detecta impactos desde CUALQUIER LADO del vehículo (frente, atrás, izquierda, derecha)
/// NUEVA FUNCIONALIDAD: Sistema de puntos de caída personalizados (reemplaza detección automática)
/// - Los puntos se asignan manualmente en el Inspector como GameObjects vacíos
/// - El sistema selecciona el punto más cercano al lugar de la colisión
/// - Simple, directo y totalmente controlable por el desarrollador
/// </summary>
public class VehiclePlayerCollision : MonoBehaviour
{
    [Header("Configuración de Colisión")]
    [SerializeField] private float fuerzaImpacto = 3f; // Empujón más fuerte
    [SerializeField] private float alturaVuelo = 2f; // Altura más notoria
    [SerializeField] private float distanciaLanzamiento = 3f; // Distancia mucho mayor
    [SerializeField] private LayerMask playerLayer = 1;
    
    [Header("Configuración de Lanzamiento")]
    [SerializeField] private float tiempoEnAire = 1f; // Tiempo más largo en el aire
    [SerializeField] private AnimationCurve curvaVuelo = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Configuración de Parábola Pequeña")]
    [SerializeField] [Range(0.3f, 1f)] private float factorAlturaParabola = 0.6f; // Factor para altura de parábola pequeña
    [SerializeField] [Range(0.5f, 1f)] private float factorTiempoParabola = 0.8f; // Factor para tiempo de parábola pequeña
    [SerializeField] private float distanciaDeteccionPlataforma = 10f; // Distancia máxima para detectar plataformas hacia abajo
    
    [Header("Plataformas del Escenario")]
    [SerializeField] private Transform plataformaIzquierda;
    [SerializeField] private Transform plataformaDerecha;
    [SerializeField] private float offsetPlataforma = 1f;
    
    [Header("Puntos de Caída Personalizados")]
    [SerializeField] private Transform[] puntosDeCapPersonalizados;
    [SerializeField] private bool usarPuntosPersonalizados = true; // ACTIVADO por defecto - sistema principal
    
    [Header("Configuración de Materiales")]
    [SerializeField] private bool ignorarMaterialesJuego = true; // Activado por defecto
    [SerializeField] private string[] nombresMaterialesIgnorar = {
        "Adoquin",
        "PrefabMaterial1",
        "PrefabMaterial2",
        "PrefabMaterial4",
        "PaloIgnifugo",
    };
    
    [Header("Configuración de Superficies Caminables")]
    [SerializeField] private string[] tagsSuperficiesCaminables = {
        "Floor", 
        "Platform", 
        "Ground", 
        "Walkable"
    };
    [SerializeField] private string[] palabrasClaveSuperficiesCaminables = {
        "floor", "platform", "ground", "terrain", "surface", "base", 
        "plataforma", "suelo", "piso"
    };
    [SerializeField] private bool considerarObjetosEstaticosComoSuperficies = true;
    
    [Header("Configuración de Colisiones Durante Vuelo")]
    [SerializeField] private bool desactivarColisionesEnVuelo = true; // Activado por defecto
    [SerializeField] private float tiempoExtraIgnorarColisiones = 0.5f; // Tiempo extra después del aterrizaje
    
    // Propiedades públicas para configuración automática
    public Transform PlataformaIzquierda 
    { 
        get => plataformaIzquierda; 
        set => plataformaIzquierda = value; 
    }
    
    public Transform PlataformaDerecha 
    { 
        get => plataformaDerecha; 
        set => plataformaDerecha = value; 
    }
    
    [Header("Debug")]
    [SerializeField] private bool mostrarGizmos = true;
    
    private AutoMovement autoMovement;
    private bool sistemaInicializado = false;
    
    void Start()
    {
        Debug.Log($"VehiclePlayerCollision iniciado en {gameObject.name}");
        
        // Forzar valores actualizados si aún tiene valores antiguos
        VerificarYActualizarValores();
        
        autoMovement = GetComponent<AutoMovement>();
        
        // Configurar los colliders existentes del vehículo para detectar colisiones con jugadores
        ConfigurarCollidersExistentes();
        
        // Auto-detectar plataformas si no están asignadas
        if (plataformaIzquierda == null || plataformaDerecha == null)
        {
            DetectarPlataformas();
        }
        
        // Configurar el sistema para ignorar materiales del juego
        if (ignorarMaterialesJuego)
        {
            ConfigurarIgnorarMateriales();
        }
        
        sistemaInicializado = true;
        Debug.Log($"VehiclePlayerCollision configurado en {gameObject.name} - Plataformas: {(plataformaIzquierda != null ? plataformaIzquierda.name : "null")} | {(plataformaDerecha != null ? plataformaDerecha.name : "null")}");
    }
    
    void Update()
    {
        // Sistema de respaldo: detectar jugadores cercanos en cualquier dirección
        if (sistemaInicializado)
        {
            DetectarJugadoresCercanos();
        }
    }
    
    /// <summary>
    /// Sistema de respaldo para detectar jugadores cercanos desde cualquier lado
    /// </summary>
    private void DetectarJugadoresCercanos()
    {
        // Buscar todos los jugadores en un radio alrededor del vehículo
        Collider[] jugadoresCercanos = Physics.OverlapSphere(transform.position, 2f, playerLayer);
        
        foreach (Collider jugador in jugadoresCercanos)
        {
            if (jugador.CompareTag("Player"))
            {
                // Verificar si el jugador está muy cerca del vehículo
                float distancia = Vector3.Distance(transform.position, jugador.transform.position);
                
                if (distancia < 1.5f) // Muy cerca del vehículo
                {
                    // Verificar que no esté ya siendo lanzado (ya no verificamos si es realista porque TODOS son realistas)
                    PlayerLaunchController launchController = jugador.GetComponent<PlayerLaunchController>();
                    if (launchController == null || !launchController.EstaSiendoLanzado())
                    {
                        // Verificar que no esté en proceso de ignorar colisiones (evitar relanzamientos)
                        if (!EstaIgnorandoColisionesConVehiculo(jugador.gameObject))
                        {
                            string ladoImpacto = DeterminarLadoImpacto(jugador.transform.position);
                            Debug.Log($"🎯 SISTEMA DE RESPALDO: Impacto desde {ladoImpacto} con {jugador.name} (distancia: {distancia:F2})");
                            AplicarEfectoImpacto(jugador.gameObject);
                        }
                        else
                        {
                            Debug.Log($"⏳ {jugador.name} está ignorando colisiones, esperando aterrizaje...");
                        }
                    }
                }
                else if (distancia < 1.8f) // Sistema de prevención para evitar que se pegue
                {
                    // Verificar si el jugador se está moviendo con el vehículo (posible pegado)
                    VerificarYSepararJugadorPegado(jugador.gameObject, distancia);
                }
            }
        }
    }
    
    /// <summary>
    /// Verifica si un jugador está pegado al vehículo y lo separa si es necesario
    /// </summary>
    private void VerificarYSepararJugadorPegado(GameObject jugador, float distancia)
    {
        Rigidbody jugadorRb = jugador.GetComponent<Rigidbody>();
        if (jugadorRb != null)
        {
            // Si el jugador tiene muy poca velocidad relativa y está cerca, puede estar pegado
            Vector3 velocidadRelativa = (jugadorRb != null ? jugadorRb.linearVelocity : Vector3.zero) - (GetComponent<Rigidbody>() != null ? GetComponent<Rigidbody>().linearVelocity : Vector3.zero);
            
            if (velocidadRelativa.magnitude < 0.5f && distancia < 1.8f)
            {
                // Posiblemente pegado, separar suavemente
                Vector3 direccionSeparacion = (jugador.transform.position - transform.position).normalized;
                if (direccionSeparacion.magnitude < 0.1f)
                {
                    direccionSeparacion = Vector3.right;
                }
                
                Vector3 fuerzaSeparacion = direccionSeparacion * 8f; // Fuerza de separación
                fuerzaSeparacion.y = 0; // No aplicar fuerza vertical
                
                jugadorRb.AddForce(fuerzaSeparacion, ForceMode.Impulse);
                Debug.Log($"⚠️ Separando jugador {jugador.name} que podría estar pegado al vehículo");
            }
        }
    }
    
    private void ConfigurarCollidersExistentes()
    {
        // Buscar todos los colliders en el vehículo y sus hijos
        Collider[] colliders = GetComponentsInChildren<Collider>();
        
        Debug.Log($"🔧 Configurando {colliders.Length} colliders para detección de jugadores en TODO el vehículo");
        
        bool tieneColliderDeteccion = false;
        
        foreach (Collider col in colliders)
        {
            Debug.Log($"🔍 Procesando collider: {col.name} - Es trigger: {col.isTrigger}");
            
            // NO ignorar colisiones - el vehículo debe mantener su física normal
            // Solo crear triggers adicionales para detección
            
            // Mantener al menos un collider como trigger para mejor detección
            if (!tieneColliderDeteccion)
            {
                if (!col.isTrigger)
                {
                    // Duplicar este collider como trigger para detección SIN afectar el original
                    GameObject triggerObj = new GameObject($"{col.name}_PlayerDetector");
                    triggerObj.transform.SetParent(transform); // Parente al vehículo principal
                    triggerObj.transform.localPosition = Vector3.zero;
                    triggerObj.transform.localRotation = Quaternion.identity;
                    triggerObj.transform.localScale = Vector3.one;
                    
                    // Copiar el collider como trigger más grande
                    if (col is BoxCollider box)
                    {
                        BoxCollider triggerBox = triggerObj.AddComponent<BoxCollider>();
                        triggerBox.center = box.center;
                        triggerBox.size = box.size * 1.3f; // Más grande para mejor detección
                        triggerBox.isTrigger = true;
                    }
                    else if (col is SphereCollider sphere)
                    {
                        SphereCollider triggerSphere = triggerObj.AddComponent<SphereCollider>();
                        triggerSphere.center = sphere.center;
                        triggerSphere.radius = sphere.radius * 1.3f;
                        triggerSphere.isTrigger = true;
                    }
                    else if (col is CapsuleCollider capsule)
                    {
                        CapsuleCollider triggerCapsule = triggerObj.AddComponent<CapsuleCollider>();
                        triggerCapsule.center = capsule.center;
                        triggerCapsule.radius = capsule.radius * 1.3f;
                        triggerCapsule.height = capsule.height * 1.3f;
                        triggerCapsule.isTrigger = true;
                    }
                    
                    Debug.Log($"✅ Trigger detector creado para {col.name} - El collider original mantiene su física");
                    tieneColliderDeteccion = true;
                }
                else
                {
                    // Ya es un trigger, usarlo para detección
                    tieneColliderDeteccion = true;
                    Debug.Log($"✅ Usando trigger existente {col.name} para detección");
                }
            }
            
            // Asegurar que el vehículo tenga rigidbody PERO con física normal
            if (col.attachedRigidbody == null && col.gameObject == gameObject)
            {
                Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = col.gameObject.AddComponent<Rigidbody>();
                    ConfigurarRigidbodyVehiculoNormal(rb);
                    Debug.Log($"Rigidbody añadido con física normal a {col.name}");
                }
                else
                {
                    ConfigurarRigidbodyVehiculoNormal(rb);
                    Debug.Log($"Rigidbody reconfigurado con física normal en {col.name}");
                }
            }
            
            Debug.Log($"✅ Collider {col.name} mantiene física original - Trigger: {col.isTrigger}");
        }
        
        // Si no hay colliders, crear uno trigger que cubra todo el vehículo
        if (colliders.Length == 0 || !tieneColliderDeteccion)
        {
            GameObject triggerObj = new GameObject("VehiclePlayerDetector");
            triggerObj.transform.SetParent(transform);
            triggerObj.transform.localPosition = Vector3.zero;
            triggerObj.transform.localRotation = Quaternion.identity;
            triggerObj.transform.localScale = Vector3.one;
            
            BoxCollider triggerCollider = triggerObj.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = Vector3.one * 2.5f; // Grande para asegurar detección desde todos los lados
            
            Debug.Log($"🆕 Trigger detector principal creado - Cubre TODO el vehículo");
        }
        
        Debug.Log($"🎯 Configuración completa: Vehículo mantiene física normal + detectores de jugadores");
    }
    
    private void DetectarPlataformas()
    {
        // Buscar objetos con nombres comunes de plataformas
        GameObject[] plataformas = GameObject.FindGameObjectsWithTag("Floor");
        if (plataformas.Length == 0)
        {
            // Buscar por nombre si no hay tags
            GameObject izq = GameObject.Find("PlataformaIzquierda");
            GameObject der = GameObject.Find("PlataformaDerecha");
            
            if (izq == null) izq = GameObject.Find("LeftPlatform");
            if (der == null) der = GameObject.Find("RightPlatform");
            
            if (izq != null) plataformaIzquierda = izq.transform;
            if (der != null) plataformaDerecha = der.transform;
        }
        else if (plataformas.Length >= 2)
        {
            // Ordenar por posición X para identificar izquierda y derecha
            System.Array.Sort(plataformas, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            plataformaIzquierda = plataformas[0].transform;
            plataformaDerecha = plataformas[plataformas.Length - 1].transform;
        }
    }
      private void OnTriggerEnter(Collider other)
    {
        HandleTrigger(other);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }
    
    /// <summary>
    /// Método estático para que los objetos hijos puedan reportar colisiones al padre vehículo
    /// </summary>
    public static void HandleCollisionFromChild(GameObject childObject, Collision collision)
    {
        VehiclePlayerCollision vehicleScript = FindVehicleScriptInParents(childObject);
        if (vehicleScript != null)
        {
            vehicleScript.HandleCollision(collision);
        }
    }
    
    /// <summary>
    /// Método estático para que los objetos hijos puedan reportar triggers al padre vehículo
    /// </summary>
    public static void HandleTriggerFromChild(GameObject childObject, Collider other)
    {
        VehiclePlayerCollision vehicleScript = FindVehicleScriptInParents(childObject);
        if (vehicleScript != null)
        {
            vehicleScript.HandleTrigger(other);
        }
    }
    
    /// <summary>
    /// Busca el componente VehiclePlayerCollision en el objeto o sus padres
    /// </summary>
    private static VehiclePlayerCollision FindVehicleScriptInParents(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            VehiclePlayerCollision script = current.GetComponent<VehiclePlayerCollision>();
            if (script != null)
            {
                return script;
            }
            current = current.parent;
        }
        return null;
    }
    
    /// <summary>
    /// Maneja la colisión (puede ser llamado desde el método OnCollisionEnter o desde un hijo)
    /// </summary>
    public void HandleCollision(Collision collision)
    {
        Debug.Log($"🔍 VehiclePlayerCollision - Colisión procesada por VehiclePlayerCollision en: {gameObject.name}");
        ProcessCollisionLogic(collision);
    }
    
    /// <summary>
    /// Maneja el trigger (puede ser llamado desde el método OnTriggerEnter o desde un hijo)
    /// </summary>
    public void HandleTrigger(Collider other)
    {
        Debug.Log($"🔍 VehiclePlayerCollision - Trigger procesado por VehiclePlayerCollision en: {gameObject.name}");
        ProcessTriggerLogic(other);
    }
    
    /// <summary>
    /// Lógica de procesamiento de colisiones extraída para reutilización
    /// </summary>
    private void ProcessCollisionLogic(Collision collision)
    {
        Debug.Log($"🔍 VehiclePlayerCollision - Colisión detectada con: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        
        // Verificar que ESTE objeto O su padre es un vehículo
        if (!IsVehicleOrChildOfVehicle())
        {
            Debug.Log($"Este objeto {gameObject.name} no es un vehículo ni hijo de uno. No se aplicará efecto de lanzamiento.");
            return;
        }
        
        if (collision.gameObject.CompareTag("Player"))
        {
            // Procesar colisión con jugador
            if (EsImpactoRealista(collision.transform.position))
            {
                string ladoImpacto = DeterminarLadoImpacto(collision.transform.position);
                Debug.Log($"✅ ¡Impacto detectado desde {ladoImpacto}! Aplicando parábola pequeña a {collision.gameObject.name}");
                AplicarEfectoImpacto(collision.gameObject);
            }
            else
            {
                Debug.Log($"⚠️ Impacto ignorado - caso excepcional en {collision.gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// Lógica de procesamiento de triggers extraída para reutilización
    /// </summary>
    private void ProcessTriggerLogic(Collider other)
    {
        Debug.Log($"🔍 VehiclePlayerCollision - Trigger detectado con: {other.name} (Tag: {other.tag})");
        
        // Verificar que ESTE objeto O su padre es un vehículo
        if (!IsVehicleOrChildOfVehicle())
        {
            Debug.Log($"Este objeto {gameObject.name} no es un vehículo ni hijo de uno. No se aplicará efecto de lanzamiento.");
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            // Ahora TODOS los impactos son realistas desde cualquier lado del vehículo
            if (EsImpactoRealista(other.transform.position))
            {
                string ladoImpacto = DeterminarLadoImpacto(other.transform.position);
                Debug.Log($"✅ ¡Impacto detectado desde {ladoImpacto}! Aplicando parábola pequeña a {other.name}");
                AplicarEfectoImpacto(other.gameObject);
            }
            else
            {
                // Esta condición nunca debería ejecutarse ahora, pero la mantenemos por seguridad
                Debug.Log($"⚠️ Impacto ignorado - caso excepcional en {other.name}");
            }
        }
    }
    
    /// <summary>
    /// Verifica si este objeto o alguno de sus padres tiene el tag de vehículo
    /// </summary>
    private bool IsVehicleOrChildOfVehicle()
    {
        // Primero verificar el objeto actual - debe tener el tag "Vehicle" configurado en vehicleTag
        string vehicleTag = "Vehicle"; // Tag por defecto, podrías hacer esto configurable si necesitas
        
        if (gameObject.CompareTag(vehicleTag))
        {
            Debug.Log("Este objeto " + gameObject.name + " es un vehículo.");
            return true;
        }
        
        // Buscar en los padres
        Transform currentParent = transform.parent;
        while (currentParent != null)
        {
            if (currentParent.CompareTag(vehicleTag))
            {
                Debug.Log("Objeto padre " + currentParent.name + " es un vehículo.");
                return true;
            }
            currentParent = currentParent.parent;
        }
        
        Debug.Log("Este objeto " + gameObject.name + " no es un vehículo ni hijo de uno.");
        return false;
    }
    
    /// <summary>
    /// Determina si el impacto es realista - AHORA PERMITE IMPACTOS DESDE CUALQUIER LADO
    /// </summary>
    private bool EsImpactoRealista(Vector3 posicionJugador)
    {
        // CAMBIO: Ahora todos los impactos son realistas desde cualquier lado del vehículo
        // Esto permite que el trigger funcione en todo el auto, no solo en el frente
        
        // Obtener dirección del vehículo para logging
        Vector3 direccionVehiculo = Vector3.right;
        if (autoMovement != null)
        {
            var tipo = typeof(AutoMovement);
            var campoDireccion = tipo.GetField("direccionMovimiento", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campoDireccion != null)
            {
                direccionVehiculo = (Vector3)campoDireccion.GetValue(autoMovement);
            }
        }
        
        // Vector desde el vehículo al jugador
        Vector3 vectorAJugador = (posicionJugador - transform.position).normalized;
        
        // Calcular el ángulo para logging, pero ahora SIEMPRE es realista
        float angulo = Vector3.Angle(direccionVehiculo, vectorAJugador);
        
        // TODOS los impactos son válidos desde cualquier lado
        bool esRealista = true;
        
        Debug.Log($"Impacto desde cualquier lado: Angulo {angulo:F1} grados - SIEMPRE REALISTA");
        
        return esRealista;
    }
    
    /// <summary>
    /// Determina desde qué lado específico ocurre el impacto - AHORA CON DIRECCIÓN DE PARÁBOLA
    /// </summary>
    private string DeterminarLadoImpacto(Vector3 posicionJugador)
    {
        Vector3 direccionVehiculo = Vector3.right;
        if (autoMovement != null)
        {
            var tipo = typeof(AutoMovement);
            var campoDireccion = tipo.GetField("direccionMovimiento", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campoDireccion != null)
            {
                direccionVehiculo = (Vector3)campoDireccion.GetValue(autoMovement);
            }
        }
        
        Vector3 vectorAJugador = (posicionJugador - transform.position).normalized;
        float angulo = Vector3.Angle(direccionVehiculo, vectorAJugador);
        
        // Determinar lado específico con direcciones de parábola
        if (angulo <= 45f)
        {
            return "FRENTE → Parábola ADELANTE";
        }
        else if (angulo >= 135f)
        {
            return "ATRÁS → Parábola ATRÁS";
        }
        else
        {
            // Determinar si es izquierda o derecha usando producto cruz
            Vector3 cruz = Vector3.Cross(direccionVehiculo, vectorAJugador);
            if (cruz.y > 0)
                return "IZQUIERDA → Parábola DERECHA";
            else
                return "DERECHA → Parábola IZQUIERDA";
        }
    }
    
    private void AplicarEfectoImpacto(GameObject jugador)
    {
        Debug.Log($"🚗💥 INICIANDO PARÁBOLA DIRECCIONAL Y SEGURA EN {jugador.name}");
        
        // Verificar que el jugador no esté ya siendo lanzado
        PlayerLaunchController launchController = jugador.GetComponent<PlayerLaunchController>();
        if (launchController != null && launchController.EstaSiendoLanzado())
        {
            Debug.Log($"❌ {jugador.name} ya está siendo lanzado, ignorando nueva colisión");
            return;
        }
        
        // Si no tiene el componente, agregarlo
        if (launchController == null)
        {
            Debug.Log($"➕ Agregando PlayerLaunchController a {jugador.name}");
            launchController = jugador.AddComponent<PlayerLaunchController>();
        }
        
        // NUEVO: Desactivar colisiones entre el jugador y el vehículo durante el vuelo
        if (desactivarColisionesEnVuelo)
        {
            DesactivarColisionesConJugador(jugador);
        }
        
        // Hacer que el jugador suelte cualquier objeto que esté sosteniendo
        PlayerObjectHolder objectHolder = jugador.GetComponent<PlayerObjectHolder>();
        if (objectHolder != null && objectHolder.HasObjectInHand())
        {
            Debug.Log($"📦 {jugador.name} soltando objeto sostenido");
            objectHolder.DropObject();
        }
        
        // Calcular la posición de aterrizaje segura con parábola direccional
        Vector3 posicionAterrizajeSegura = CalcularPosicionAterrizaje(jugador.transform.position);
        Debug.Log($"🎯 Aterrizaje direccional calculado: {posicionAterrizajeSegura}");
        
        // Calcular valores para parábola pequeña y controlada
        float alturaParabolaPequena = CalcularAlturaParabola();
        float tiempoParabolaPequena = CalcularTiempoParabola();
        
        // Lanzar al jugador con parábola direccional hacia posición segura
        launchController.LanzarJugador(posicionAterrizajeSegura, alturaParabolaPequena, tiempoParabolaPequena, curvaVuelo);
        
        // NUEVO: Programar la reactivación de colisiones después del aterrizaje
        if (desactivarColisionesEnVuelo)
        {
            float tiempoTotalVuelo = tiempoParabolaPequena + tiempoExtraIgnorarColisiones;
            StartCoroutine(ReactivarColisionesConJugador(jugador, tiempoTotalVuelo));
        }
        
        Debug.Log($"🚀 ¡{jugador.name} realiza parábola direccional hacia plataforma segura!");
    }
    
    /// <summary>
    /// Calcula la altura de la parábola pequeña
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private float CalcularAlturaParabola()
    {
        return alturaVuelo * factorAlturaParabola; // Usar factor configurable
    }
    
    /// <summary>
    /// Calcula el tiempo de la parábola pequeña
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private float CalcularTiempoParabola()
    {
        return tiempoEnAire * factorTiempoParabola; // Usar factor configurable
    }
    
    /// <summary>
    /// Calcula la posición de aterrizaje usando SOLO el sistema de puntos personalizados
    /// SISTEMA SIMPLIFICADO: Solo puntos de caída personalizados
    /// </summary>
    private Vector3 CalcularPosicionAterrizaje(Vector3 posicionJugador)
    {
        // SISTEMA SIMPLIFICADO: Solo usar puntos personalizados
        if (usarPuntosPersonalizados && puntosDeCapPersonalizados != null && puntosDeCapPersonalizados.Length > 0)
        {
            Debug.Log("🎯 Usando sistema de puntos de caída personalizados");
            return SeleccionarMejorPuntoPersonalizado(puntosDeCapPersonalizados, Vector3.zero, posicionJugador);
        }
        
        // FALLBACK: Si no hay puntos personalizados, usar las plataformas manuales configuradas
        Debug.LogWarning("⚠️ No hay puntos personalizados configurados. Usando plataformas manuales como fallback.");
        return UsarPlataformasConfiguradas(posicionJugador, posicionJugador);
    }
    
    /// <summary>
    /// Obtiene la dirección de movimiento del vehículo (Principio SRP - Single Responsibility)
    /// </summary>
    private Vector3 ObtenerDireccionMovimientoVehiculo()
    {
        Vector3 direccionMovimiento = Vector3.right; // Dirección por defecto
        
        if (autoMovement != null)
        {
            var tipo = typeof(AutoMovement);
            var campoDireccion = tipo.GetField("direccionMovimiento", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campoDireccion != null)
            {
                direccionMovimiento = (Vector3)campoDireccion.GetValue(autoMovement);
                //Debug.Log($"Direccion de movimiento del vehiculo: {direccionMovimiento}");
            }
        }
        
        return direccionMovimiento;
    }
    
    /// <summary>
    /// Selecciona el mejor punto de caída personalizado según la cercanía al punto de colisión
    /// SISTEMA PRINCIPAL: Puntos de caída personalizados
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private Vector3 SeleccionarMejorPuntoPersonalizado(Transform[] puntosPersonalizados, Vector3 posicionObjetivo, Vector3 posicionOriginal)
    {
        if (puntosPersonalizados == null || puntosPersonalizados.Length == 0)
        {
            Debug.LogWarning("⚠️ Array de puntos personalizados vacío o nulo");
            return posicionOriginal;
        }
        
        float distanciaMenor = float.MaxValue;
        Vector3 posicionMejor = posicionOriginal;
        Transform mejorPunto = null;
        
        // Calcular la posición del choque (donde ocurrió la colisión)
        Vector3 posicionChoque = transform.position; // Posición del vehículo como referencia del choque
        
        foreach (Transform puntoPersonalizado in puntosPersonalizados)
        {
            if (puntoPersonalizado == null) continue;
            
            // NUEVA LÓGICA: Calcular distancia desde el punto de CHOQUE, no del objetivo
            float distanciaDesdeChoque = Vector3.Distance(posicionChoque, puntoPersonalizado.position);
            
            if (distanciaDesdeChoque < distanciaMenor)
            {
                distanciaMenor = distanciaDesdeChoque;
                posicionMejor = puntoPersonalizado.position;
                mejorPunto = puntoPersonalizado;
                
                Debug.Log($"🎯 Punto personalizado candidato: {puntoPersonalizado.name} - Distancia desde choque: {distanciaDesdeChoque:F2}");
            }
        }
        
        if (mejorPunto != null)
        {
            Debug.Log($"✅ Punto de caída seleccionado: {mejorPunto.name} a {distanciaMenor:F2} unidades del choque");
            
            // Ajustar la altura para asegurar que esté sobre el punto de caída
            Vector3 posicionFinal = posicionMejor;
            posicionFinal.y += 0.5f; // Un poco arriba del punto para evitar que se atasque
            
            return posicionFinal;
        }
        
        Debug.LogWarning("⚠️ No se pudo seleccionar ningún punto personalizado válido");
        return posicionOriginal;
    }
    
    /// <summary>
    /// Fallback: usar las plataformas configuradas manualmente si no hay puntos personalizados
    /// (Principio OCP - Open/Closed Principle - extensible para nuevos tipos de fallback)
    /// </summary>
    private Vector3 UsarPlataformasConfiguradas(Vector3 posicionObjetivo, Vector3 posicionOriginal)
    {
        Debug.Log("🔄 Usando plataformas configuradas manualmente como fallback");
        
        if (plataformaIzquierda != null && plataformaDerecha != null)
        {
            // Determinar cuál plataforma está más cerca del objetivo
            float distanciaIzq = Vector3.Distance(posicionObjetivo, plataformaIzquierda.position);
            float distanciaDer = Vector3.Distance(posicionObjetivo, plataformaDerecha.position);
            
            Transform plataformaElegida = distanciaIzq < distanciaDer ? plataformaIzquierda : plataformaDerecha;
            
            Vector3 posicionSegura = plataformaElegida.position;
            posicionSegura.x += offsetPlataforma * (plataformaElegida == plataformaIzquierda ? 1 : -1);
            posicionSegura.y += 1f; // Un poco arriba de la plataforma
            
            Debug.Log($"🏛️ Plataforma elegida: {plataformaElegida.name}");
            return posicionSegura;
        }
        else
        {
            Debug.LogWarning("⚠️ No hay plataformas configuradas, usando posición original");
            return posicionOriginal;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!mostrarGizmos) return;
        
        // Dibujar el área de detección de jugadores cercanos
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f); // Radio de detección
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f); // Radio de activación del efecto
        
        // Dibujar los colliders del vehículo
        DibujarCollidersVehiculo();
        
        // Dibujar conexiones con plataformas configuradas manualmente
        DibujarPlataformasConfiguradas();
        
        // NUEVO: Dibujar puntos de caída personalizados
        DibujarPuntosDeCapPersonalizados();
        
        // Dibujar todas las plataformas detectadas automáticamente
        DibujarPlataformasDetectadas();
        
        // Dibujar materiales ignorados si el sistema está activo
        if (ignorarMaterialesJuego)
        {
            DibujarMaterialesIgnorados();
        }
    }
    
    /// <summary>
    /// Dibuja los colliders del vehículo en el Scene View
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void DibujarCollidersVehiculo()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.enabled)
            {
                Gizmos.matrix = Matrix4x4.TRS(col.transform.position, col.transform.rotation, col.transform.lossyScale);
                if (col is BoxCollider box)
                {
                    if (col.isTrigger)
                        Gizmos.color = Color.green; // Triggers en verde
                    else
                        Gizmos.color = Color.yellow; // Colliders físicos en amarillo
                    
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    if (col.isTrigger)
                        Gizmos.color = Color.green;
                    else
                        Gizmos.color = Color.yellow;
                    
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
            }
        }
        Gizmos.matrix = Matrix4x4.identity;
    }
    
    /// <summary>
    /// Dibuja los puntos de caída personalizados
    /// NUEVA FUNCIONALIDAD: Visualización de puntos personalizados
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void DibujarPuntosDeCapPersonalizados()
    {
        if (!usarPuntosPersonalizados || puntosDeCapPersonalizados == null || puntosDeCapPersonalizados.Length == 0)
            return;
        
        for (int i = 0; i < puntosDeCapPersonalizados.Length; i++)
        {
            Transform punto = puntosDeCapPersonalizados[i];
            if (punto == null) continue;
            
            // Color diferente para puntos activos vs inactivos
            Gizmos.color = usarPuntosPersonalizados ? Color.cyan : Color.gray;
            
            // Dibujar esfera para el punto de caída
            Gizmos.DrawSphere(punto.position, 0.5f);
            
            // Dibujar wireframe para mejor visibilidad
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(punto.position, 0.5f);
            
            // Dibujar línea de conexión al vehículo
            Gizmos.color = usarPuntosPersonalizados ? Color.cyan : Color.gray;
            Gizmos.DrawLine(transform.position, punto.position);
            
            // Dibujar indicador de dirección hacia arriba
            Gizmos.color = Color.yellow;
            Vector3 arriba = punto.position + Vector3.up * 1f;
            Gizmos.DrawLine(punto.position, arriba);
            
            // Dibujar número del punto
            #if UNITY_EDITOR
            GUIStyle style = new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = usarPuntosPersonalizados ? Color.cyan : Color.gray },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            UnityEditor.Handles.Label(punto.position + Vector3.up * 1.5f, $"Punto {i + 1}\n{punto.name}", style);
            #endif
        }
        
        // Dibujar información del sistema en la posición del vehículo
        #if UNITY_EDITOR
        if (usarPuntosPersonalizados)
        {
            GUIStyle statusStyle = new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.green },
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
                $"PUNTOS PERSONALIZADOS\nActivos: {puntosDeCapPersonalizados.Length}", 
                statusStyle);
        }
        #endif
    }
    
    /// <summary>
    /// Dibuja las plataformas configuradas manualmente
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void DibujarPlataformasConfiguradas()
    {
        Gizmos.color = Color.magenta;
        if (plataformaIzquierda != null)
        {
            Gizmos.DrawLine(transform.position, plataformaIzquierda.position);
            // Dibujar zona segura de la plataforma
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(plataformaIzquierda.position, Vector3.one * 2f);
        }
        if (plataformaDerecha != null)
        {
            Gizmos.DrawLine(transform.position, plataformaDerecha.position);
            // Dibujar zona segura de la plataforma
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(plataformaDerecha.position, Vector3.one * 2f);
        }
    }
    
    /// <summary>
    /// Dibuja las plataformas detectadas automáticamente por tag
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void DibujarPlataformasDetectadas()
    {
        GameObject[] plataformasDetectadas = GameObject.FindGameObjectsWithTag("Floor");
        Gizmos.color = Color.blue;
        foreach (GameObject plataforma in plataformasDetectadas)
        {
            if (plataforma != null)
            {
                Gizmos.DrawWireCube(plataforma.transform.position, Vector3.one * 1.5f);
                
                // Dibujar línea de conexión al vehículo para mostrar que están detectadas
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, plataforma.transform.position);
                Gizmos.color = Color.blue;
            }
        }
    }
    
    /// <summary>
    /// Dibuja los materiales que están siendo ignorados
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void DibujarMaterialesIgnorados()
    {
        foreach (string nombreMaterial in nombresMaterialesIgnorar)
        {
            GameObject[] materiales = GameObject.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.Contains(nombreMaterial))
                .ToArray();
            
            foreach (GameObject material in materiales)
            {
                if (material != null)
                {
                    // Dibujar materiales ignorados en color rojo con transparencia
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Rojo semi-transparente
                    
                    Collider materialCollider = material.GetComponent<Collider>();
                    if (materialCollider != null)
                    {
                        Gizmos.matrix = Matrix4x4.TRS(material.transform.position, material.transform.rotation, material.transform.lossyScale);
                        
                        if (materialCollider is BoxCollider box)
                        {
                            Gizmos.DrawCube(box.center, box.size);
                        }
                        else if (materialCollider is SphereCollider sphere)
                        {
                            Gizmos.DrawSphere(sphere.center, sphere.radius);
                        }
                        else
                        {
                            // Para otros tipos de colliders, dibujar un cubo genérico
                            Gizmos.DrawCube(Vector3.zero, Vector3.one);
                        }
                        
                        Gizmos.matrix = Matrix4x4.identity;
                    }
                    else
                    {
                        // Si no tiene collider, dibujar un cubo pequeño en la posición
                        Gizmos.DrawCube(material.transform.position, Vector3.one * 0.5f);
                    }
                    
                    // Dibujar línea punteada desde el vehículo al material para mostrar que está ignorado
                    Gizmos.color = Color.red;
                    Vector3 direccion = (material.transform.position - transform.position).normalized;
                    for (int i = 0; i < 10; i++)
                    {
                        float t1 = i / 10f;
                        float t2 = (i + 0.5f) / 10f;
                        Vector3 start = Vector3.Lerp(transform.position, material.transform.position, t1);
                        Vector3 end = Vector3.Lerp(transform.position, material.transform.position, t2);
                        Gizmos.DrawLine(start, end);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Método de context menu para activar el sistema de puntos personalizados
    /// </summary>
    [ContextMenu("Activar Puntos de Caída Personalizados")]
    public void ActivarPuntosPersonalizados()
    {
        usarPuntosPersonalizados = true;
        
        Debug.Log("🎯 Sistema de puntos de caída personalizados activado");
        Debug.Log($"- Puntos configurados: {(puntosDeCapPersonalizados?.Length ?? 0)}");
        
        if (puntosDeCapPersonalizados == null || puntosDeCapPersonalizados.Length == 0)
        {
            Debug.LogWarning("⚠️ No hay puntos personalizados asignados. Configúralos en el Inspector.");
        }
        else
        {
            for (int i = 0; i < puntosDeCapPersonalizados.Length; i++)
            {
                if (puntosDeCapPersonalizados[i] != null)
                {
                    Debug.Log($"  - Punto {i + 1}: {puntosDeCapPersonalizados[i].name} en {puntosDeCapPersonalizados[i].position}");
                }
                else
                {
                    Debug.LogWarning($"  - Punto {i + 1}: NULO - Asignar en el Inspector");
                }
            }
        }
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Método de context menu para desactivar el sistema de puntos personalizados
    /// </summary>
    [ContextMenu("Desactivar Puntos Personalizados - Usar Floor")]
    public void DesactivarPuntosPersonalizados()
    {
        usarPuntosPersonalizados = false;
        
        Debug.Log("🔄 Sistema de puntos personalizados desactivado - Volviendo a sistema Floor original");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Obtiene información sobre el estado actual del sistema de puntos de caída
    /// </summary>
    /// <returns>String con información del estado</returns>
    public string ObtenerEstadoSistema()
    {
        System.Text.StringBuilder info = new System.Text.StringBuilder();
        info.AppendLine("=== ESTADO DEL SISTEMA DE CAÍDA ===");
        info.AppendLine($"Puntos personalizados activos: {usarPuntosPersonalizados}");
        info.AppendLine($"Cantidad de puntos configurados: {puntosDeCapPersonalizados?.Length ?? 0}");
        
        if (puntosDeCapPersonalizados != null && puntosDeCapPersonalizados.Length > 0)
        {
            info.AppendLine("Puntos configurados:");
            for (int i = 0; i < puntosDeCapPersonalizados.Length; i++)
            {
                Transform punto = puntosDeCapPersonalizados[i];
                info.AppendLine($"  {i + 1}. {(punto != null ? punto.name : "NULO")} - {(punto != null ? punto.position.ToString() : "Sin posición")}");
            }
        }
        
        if (!usarPuntosPersonalizados)
        {
            info.AppendLine("Sistema actual: Búsqueda de plataformas Floor");
        }
        
        return info.ToString();
    }
    
    /// <summary>
    /// Método de context menu para mostrar el estado del sistema
    /// </summary>
    [ContextMenu("Mostrar Estado del Sistema")]
    public void MostrarEstadoSistema()
    {
        Debug.Log(ObtenerEstadoSistema());
    }
    
    /// <summary>
    /// Verifica y actualiza los valores para un empujón fuerte
    /// </summary>
    private void VerificarYActualizarValores()
    {
        bool valoresActualizados = false;
        
        // Verificar y actualizar fuerzaImpacto - valores más altos para empujón fuerte
        if (fuerzaImpacto < 2f) // Si tiene valor muy bajo
        {
            fuerzaImpacto = 3f;
            valoresActualizados = true;
            Debug.Log($"Actualizando fuerzaImpacto a {fuerzaImpacto}");
        }
        
        // Verificar y actualizar alturaVuelo
        if (alturaVuelo < 1.5f) // Si tiene valor muy bajo
        {
            alturaVuelo = 2f;
            valoresActualizados = true;
            Debug.Log($"Actualizando alturaVuelo a {alturaVuelo}");
        }
        
        // Verificar y actualizar distanciaLanzamiento
        if (distanciaLanzamiento < 2f) // Si tiene valor muy bajo
        {
            distanciaLanzamiento = 3f;
            valoresActualizados = true;
            Debug.Log($"Actualizando distanciaLanzamiento a {distanciaLanzamiento}");
        }
        
        // Verificar y actualizar tiempoEnAire
        if (tiempoEnAire < 0.8f) // Si tiene valor muy bajo
        {
            tiempoEnAire = 1f;
            valoresActualizados = true;
            Debug.Log($"Actualizando tiempoEnAire a {tiempoEnAire}");
        }
        
        if (valoresActualizados)
        {
            Debug.Log("✅ Valores actualizados para empujón fuerte");
        }
    }
    
    /// <summary>
    /// Método de context menu para resetear manualmente los valores a la configuración fuerte
    /// </summary>
    [ContextMenu("Resetear a Empujón Fuerte")]
    public void ResetearAValoresReducidos()
    {
        fuerzaImpacto = 3f;
        alturaVuelo = 2f;
        distanciaLanzamiento = 3f;
        tiempoEnAire = 1f;
        
        Debug.Log("🔄 Valores reseteados para empujón fuerte:");
        Debug.Log($"- Fuerza Impacto: {fuerzaImpacto} (FUERTE)");
        Debug.Log($"- Altura Vuelo: {alturaVuelo} (ALTA)");
        Debug.Log($"- Distancia Lanzamiento: {distanciaLanzamiento} (LARGA)");
        Debug.Log($"- Tiempo en Aire: {tiempoEnAire} (LARGO)");
        
        #if UNITY_EDITOR
        // Marcar el objeto como "dirty" para que Unity guarde los cambios
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Método de context menu para reconfigurar el sistema de ignorar materiales
    /// </summary>
    [ContextMenu("Reconfigurar Ignorar Materiales")]
    public void ReconfigurarIgnorarMateriales()
    {
        if (ignorarMaterialesJuego)
        {
            ConfigurarIgnorarMateriales();
            Debug.Log("🔄 Sistema de ignorar materiales reconfigurado");
        }
        else
        {
            RestaurarColisionesMateriales();
            Debug.Log("🔄 Colisiones con materiales restauradas");
        }
    }
    
    /// <summary>
    /// Restaura las colisiones con materiales del juego
    /// </summary>
    private void RestaurarColisionesMateriales()
    {
        Collider[] collidersVehiculo = GetComponentsInChildren<Collider>();
        int colisionesRestauradas = 0;
        
        foreach (string nombreMaterial in nombresMaterialesIgnorar)
        {
            GameObject[] materiales = GameObject.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.Contains(nombreMaterial))
                .ToArray();
            
            foreach (GameObject material in materiales)
            {
                Collider[] collidersMaterial = material.GetComponentsInChildren<Collider>();
                
                foreach (Collider colliderMaterial in collidersMaterial)
                {
                    if (colliderMaterial != null && !colliderMaterial.isTrigger)
                    {
                        foreach (Collider colliderVehiculo in collidersVehiculo)
                        {
                            if (colliderVehiculo != null && !colliderVehiculo.isTrigger)
                            {
                                Physics.IgnoreCollision(colliderVehiculo, colliderMaterial, false);
                                colisionesRestauradas++;
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log($"📊 Total de colisiones restauradas: {colisionesRestauradas}");
    }
    
    /// <summary>
    /// Método de context menu para configurar criterios de superficie caminable
    /// </summary>
    [ContextMenu("Configurar Criterios Superficie Caminable")]
    public void ConfigurarCriteriosSuperficieCaminable()
    {
        // Resetear a valores por defecto seguros
        tagsSuperficiesCaminables = new string[] {
            "Floor", "Platform", "Ground", "Walkable", "Terrain", "Base"
        };
        
        palabrasClaveSuperficiesCaminables = new string[] {
            "floor", "platform", "ground", "terrain", "surface", "base", 
            "plataforma", "suelo", "piso", "walkable", "caminable"
        };
        
        considerarObjetosEstaticosComoSuperficies = true;
        
        Debug.Log("🔄 Criterios de superficie caminable configurados:");
        Debug.Log($"- Tags: {string.Join(", ", tagsSuperficiesCaminables)}");
        Debug.Log($"- Palabras clave: {string.Join(", ", palabrasClaveSuperficiesCaminables)}");
        Debug.Log($"- Objetos estáticos: {considerarObjetosEstaticosComoSuperficies}");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Determina desde qué lado del vehículo ocurrió la colisión
    /// </summary>
    private string DeterminarLadoColision(Vector3 direccionColision)
    {
        // Determinar el lado basado en la dirección de la colisión
        float x = direccionColision.x;
        float z = direccionColision.z;
        
        if (Mathf.Abs(x) > Mathf.Abs(z))
        {
            return x > 0 ? "LADO DERECHO" : "LADO IZQUIERDO";
        }
        else
        {
            return z > 0 ? "FRENTE" : "PARTE TRASERA";
        }
    }
    
    /// <summary>
    /// Separa inmediatamente al jugador del vehículo para evitar que quede pegado
    /// de manera realista según la dirección del impacto
    /// </summary>
    private void SepararJugadorDelVehiculo(GameObject jugador)
    {
        // Obtener la dirección real del movimiento del vehículo
        Vector3 direccionMovimientoVehiculo = Vector3.right; // Dirección por defecto
        
        if (autoMovement != null)
        {
            var tipo = typeof(AutoMovement);
            var campoDireccion = tipo.GetField("direccionMovimiento", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campoDireccion != null)
            {
                direccionMovimientoVehiculo = (Vector3)campoDireccion.GetValue(autoMovement);
            }
        }
        
        // Calcular dónde debería estar el jugador DESPUÉS del impacto
        // Un poco adelante en la dirección del vehículo, no simplemente alejado del centro
        Vector3 posicionSeparacion = jugador.transform.position + direccionMovimientoVehiculo * 1.5f;
        posicionSeparacion.y = jugador.transform.position.y; // Mantener altura
        
        // Verificar que no haya obstáculos en esa posición (opcional)
        // Si hay obstáculos, usar separación lateral
        if (Physics.CheckSphere(posicionSeparacion, 0.5f))
        {
            // Separación lateral si hay obstáculo adelante
            Vector3 direccionLateral = Vector3.Cross(direccionMovimientoVehiculo, Vector3.up).normalized;
            posicionSeparacion = jugador.transform.position + direccionLateral * 2f;
            Debug.Log($"🔄 Obstáculo detectado, separación lateral aplicada");
        }
        
        // Aplicar la separación inmediatamente
        jugador.transform.position = posicionSeparacion;
        
        // Si el jugador tiene rigidbody, darle impulso en la dirección realista
        Rigidbody jugadorRb = jugador.GetComponent<Rigidbody>();
        if (jugadorRb != null)
        {
            // Detener cualquier movimiento previo
            jugadorRb.linearVelocity = Vector3.zero;
            jugadorRb.angularVelocity = Vector3.zero;
            
            // Aplicar impulso en la dirección del movimiento del vehículo
            Vector3 impulsoRealista = direccionMovimientoVehiculo * 7f;
            impulsoRealista.y = 0; // No impulso vertical aquí
            jugadorRb.AddForce(impulsoRealista, ForceMode.Impulse);
        }
        
        Debug.Log($"🚗� Separación realista: jugador empujado hacia {posicionSeparacion} siguiendo dirección del vehículo");
    }
    
    /// <summary>
    /// Configura el rigidbody del vehículo para física normal (sin interferir con AutoMovement)
    /// </summary>
    private void ConfigurarRigidbodyVehiculoNormal(Rigidbody rb)
    {
        // Configuración básica que NO interfiere con AutoMovement
        rb.useGravity = true;
        
        // Configuración mínima para mantener estabilidad pero permitir movimiento normal
        rb.mass = 1f; // Masa normal
    rb.linearDamping = 0f; // Sin resistencia lineal
    rb.angularDamping = 5f; // Un poco de resistencia angular para estabilidad
        
        // Solo congelar rotaciones problemáticas
        rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                        RigidbodyConstraints.FreezeRotationZ;
        
        Debug.Log($"⚙️ Rigidbody del vehículo configurado para física normal (compatible con AutoMovement)");
    }
    
    /// <summary>
    /// Configura el sistema para ignorar colisiones con materiales del juego
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void ConfigurarIgnorarMateriales()
    {
        Debug.Log("🚫 Configurando sistema para ignorar materiales del juego...");
        
        // Obtener todos los colliders del vehículo
        Collider[] collidersVehiculo = GetComponentsInChildren<Collider>();
        
        // Buscar y configurar materiales existentes en la escena
        ConfigurarMaterialesExistentes(collidersVehiculo);
        
        // Configurar sistema de monitoreo continuo para materiales que aparezcan durante el juego
        StartCoroutine(MonitorearNuevosMateriales(collidersVehiculo));
        
        Debug.Log($"✅ Sistema de ignorar materiales configurado para {nombresMaterialesIgnorar.Length} tipos de materiales");
    }
    
    /// <summary>
    /// Configura las colisiones ignoradas para materiales que ya existen en la escena
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void ConfigurarMaterialesExistentes(Collider[] collidersVehiculo)
    {
        int materialesIgnorados = 0;
        
        foreach (string nombreMaterial in nombresMaterialesIgnorar)
        {
            // Buscar todos los objetos con el nombre del material
            GameObject[] materiales = GameObject.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.Contains(nombreMaterial))
                .ToArray();
            
            foreach (GameObject material in materiales)
            {
                Collider[] collidersMaterial = material.GetComponentsInChildren<Collider>();
                
                foreach (Collider colliderMaterial in collidersMaterial)
                {
                    if (colliderMaterial != null && !colliderMaterial.isTrigger)
                    {
                        // Ignorar colisiones entre todos los colliders del vehículo y este material
                        foreach (Collider colliderVehiculo in collidersVehiculo)
                        {
                            if (colliderVehiculo != null && !colliderVehiculo.isTrigger)
                            {
                                Physics.IgnoreCollision(colliderVehiculo, colliderMaterial, true);
                                materialesIgnorados++;
                            }
                        }
                        
                        Debug.Log($"🚫 Colisiones ignoradas con material: {material.name}");
                    }
                }
            }
        }
        
        Debug.Log($"📊 Total de colisiones de materiales ignoradas: {materialesIgnorados}");
    }
    
    /// <summary>
    /// Monitorea continuamente la aparición de nuevos materiales en la escena
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private System.Collections.IEnumerator MonitorearNuevosMateriales(Collider[] collidersVehiculo)
    {
        HashSet<GameObject> materialesConocidos = new HashSet<GameObject>();
        
        // Agregar materiales existentes al conjunto conocido
        foreach (string nombreMaterial in nombresMaterialesIgnorar)
        {
            GameObject[] materialesExistentes = GameObject.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.Contains(nombreMaterial))
                .ToArray();
            
            foreach (GameObject material in materialesExistentes)
            {
                materialesConocidos.Add(material);
            }
        }
        
        while (gameObject != null && gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(0.5f); // Verificar cada 0.5 segundos
            
            // Buscar nuevos materiales
            foreach (string nombreMaterial in nombresMaterialesIgnorar)
            {
                GameObject[] materialesActuales = GameObject.FindObjectsOfType<GameObject>()
                    .Where(obj => obj.name.Contains(nombreMaterial))
                    .ToArray();
                
                foreach (GameObject material in materialesActuales)
                {
                    if (!materialesConocidos.Contains(material))
                    {
                        // Nuevo material detectado
                        ConfigurarIgnorarNuevoMaterial(material, collidersVehiculo);
                        materialesConocidos.Add(material);
                        Debug.Log($"🆕 Nuevo material detectado y configurado: {material.name}");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Configura ignorar colisiones para un material recién aparecido
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void ConfigurarIgnorarNuevoMaterial(GameObject nuevoMaterial, Collider[] collidersVehiculo)
    {
        Collider[] collidersMaterial = nuevoMaterial.GetComponentsInChildren<Collider>();
        
        foreach (Collider colliderMaterial in collidersMaterial)
        {
            if (colliderMaterial != null && !colliderMaterial.isTrigger)
            {
                // Ignorar colisiones entre todos los colliders del vehículo y este nuevo material
                foreach (Collider colliderVehiculo in collidersVehiculo)
                {
                    if (colliderVehiculo != null && !colliderVehiculo.isTrigger)
                    {
                        Physics.IgnoreCollision(colliderVehiculo, colliderMaterial, true);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Dibuja las direcciones de parábola según los lados de colisión
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void DibujarDireccionesParabola()
    {
        Vector3 direccionVehiculo = ObtenerDireccionMovimientoVehiculo();
        Vector3 posicionVehiculo = transform.position;
        float longitudFlecha = 3f;
        
        // Flecha FRENTE → Parábola ADELANTE (Verde)
        Gizmos.color = Color.green;
        Vector3 puntoFrente = posicionVehiculo + direccionVehiculo * 2f;
        Vector3 direccionParabolaFrente = direccionVehiculo * longitudFlecha;
        Gizmos.DrawRay(puntoFrente, direccionParabolaFrente);
        DibujarPuntaFlecha(puntoFrente + direccionParabolaFrente, direccionVehiculo, Color.green);
        
        // Flecha ATRÁS → Parábola ATRÁS (Rojo)
        Gizmos.color = Color.red;
        Vector3 puntoAtras = posicionVehiculo + (-direccionVehiculo) * 2f;
        Vector3 direccionParabolaAtras = (-direccionVehiculo) * longitudFlecha;
        Gizmos.DrawRay(puntoAtras, direccionParabolaAtras);
        DibujarPuntaFlecha(puntoAtras + direccionParabolaAtras, -direccionVehiculo, Color.red);
        
        // Flecha IZQUIERDA → Parábola DERECHA (Azul)
        Gizmos.color = Color.blue;
        Vector3 direccionIzquierda = Vector3.Cross(direccionVehiculo, Vector3.up).normalized;
        Vector3 puntoIzquierda = posicionVehiculo + direccionIzquierda * 2f;
        Vector3 direccionParabolaDerecha = Vector3.Cross(Vector3.up, direccionVehiculo).normalized * longitudFlecha;
        Gizmos.DrawRay(puntoIzquierda, direccionParabolaDerecha);
        DibujarPuntaFlecha(puntoIzquierda + direccionParabolaDerecha, Vector3.Cross(Vector3.up, direccionVehiculo).normalized, Color.blue);
        
        // Flecha DERECHA → Parábola IZQUIERDA (Magenta)
        Gizmos.color = Color.magenta;
        Vector3 direccionDerecha = Vector3.Cross(Vector3.up, direccionVehiculo).normalized;
        Vector3 puntoDerecha = posicionVehiculo + direccionDerecha * 2f;
        Vector3 direccionParabolaIzquierda = Vector3.Cross(direccionVehiculo, Vector3.up).normalized * longitudFlecha;
        Gizmos.DrawRay(puntoDerecha, direccionParabolaIzquierda);
        DibujarPuntaFlecha(puntoDerecha + direccionParabolaIzquierda, Vector3.Cross(direccionVehiculo, Vector3.up).normalized, Color.magenta);
        
        // Etiquetas de texto (solo en Scene View)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(puntoFrente + direccionParabolaFrente, "FRENTE→ADELANTE", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.green } });
        UnityEditor.Handles.Label(puntoAtras + direccionParabolaAtras, "ATRÁS→ATRÁS", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } });
        UnityEditor.Handles.Label(puntoIzquierda + direccionParabolaDerecha, "IZQ→DER", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.blue } });
        UnityEditor.Handles.Label(puntoDerecha + direccionParabolaIzquierda, "DER→IZQ", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.magenta } });
        #endif
    }
    
    /// <summary>
    /// Dibuja la punta de una flecha en los gizmos
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void DibujarPuntaFlecha(Vector3 punta, Vector3 direccion, Color color)
    {
        Gizmos.color = color;
        float tamanoPunta = 0.3f;
        
        // Crear vectores perpendiculares para la punta de la flecha
        Vector3 derecha = Vector3.Cross(direccion, Vector3.up).normalized * tamanoPunta;
        Vector3 arriba = Vector3.up * tamanoPunta;
        
        // Dibujar las líneas de la punta de la flecha
        Gizmos.DrawLine(punta, punta - direccion.normalized * tamanoPunta + derecha);
        Gizmos.DrawLine(punta, punta - direccion.normalized * tamanoPunta - derecha);
        Gizmos.DrawLine(punta, punta - direccion.normalized * tamanoPunta + arriba);
        Gizmos.DrawLine(punta, punta - direccion.normalized * tamanoPunta - arriba);
    }
    
    /// <summary>
    /// Determina si una superficie es caminable usando múltiples criterios configurables
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private bool EsSuperficieCaminable(Collider superficie)
    {
        // CRITERIO 1: Tags configurables de superficies caminables
        foreach (string tag in tagsSuperficiesCaminables)
        {
            if (superficie.CompareTag(tag))
            {
                Debug.Log($"📍 Superficie caminable por tag '{tag}': {superficie.name}");
                return true;
            }
        }
        
        // CRITERIO 2: Nombre del objeto contiene palabras clave configurables
        string nombreLower = superficie.name.ToLower();
        
        foreach (string palabra in palabrasClaveSuperficiesCaminables)
        {
            if (nombreLower.Contains(palabra.ToLower()))
            {
                Debug.Log($"📍 Superficie caminable por nombre '{superficie.name}' (contiene '{palabra}')");
                return true;
            }
        }
        
        // CRITERIO 3: Verificar si es un terreno de Unity
        if (superficie.GetComponent<Terrain>() != null)
        {
            Debug.Log($"📍 Superficie caminable: Terreno de Unity '{superficie.name}'");
            return true;
        }
        
        // CRITERIO 4: Objetos estáticos con renderer (si está habilitado)
        if (considerarObjetosEstaticosComoSuperficies && 
            superficie.GetComponent<MeshRenderer>() != null && 
            superficie.gameObject.isStatic)
        {
            Debug.Log($"📍 Superficie caminable: Objeto estático '{superficie.name}'");
            return true;
        }
        
        // CRITERIO 5: Verificar materiales conocidos NO caminables
        string[] materialesNoCaminables = { "water", "lava", "void", "kill", "death", "trap", "agua", "muerte" };
        
        foreach (string materialPeligroso in materialesNoCaminables)
        {
            if (nombreLower.Contains(materialPeligroso))
            {
                Debug.Log($"❌ Superficie NO caminable por material peligroso '{superficie.name}' (contiene '{materialPeligroso}')");
                return false;
            }
        }
        
        // Si llegamos aquí, la superficie no cumple criterios específicos
        Debug.Log($"❌ Superficie NO reconocida como caminable: '{superficie.name}' (Tag: {superficie.tag})");
        return false; // Ser más estricto: solo superficies reconocidas son caminables
    }
    
    /// <summary>
    /// Desactiva temporalmente las colisiones entre el jugador y el vehículo
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private void DesactivarColisionesConJugador(GameObject jugador)
    {
        Collider[] collidersJugador = jugador.GetComponentsInChildren<Collider>();
        Collider[] collidersVehiculo = GetComponentsInChildren<Collider>();
        
        int colisionesDesactivadas = 0;
        
        foreach (Collider colliderJugador in collidersJugador)
        {
            if (colliderJugador != null && !colliderJugador.isTrigger)
            {
                foreach (Collider colliderVehiculo in collidersVehiculo)
                {
                    if (colliderVehiculo != null && !colliderVehiculo.isTrigger)
                    {
                        Physics.IgnoreCollision(colliderJugador, colliderVehiculo, true);
                        colisionesDesactivadas++;
                    }
                }
            }
        }
        
        Debug.Log($"🚫 Colisiones desactivadas entre {jugador.name} y vehículo: {colisionesDesactivadas} pares");
    }
    
    /// <summary>
    /// Reactiva las colisiones entre el jugador y el vehículo después del vuelo
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private System.Collections.IEnumerator ReactivarColisionesConJugador(GameObject jugador, float tiempoEspera)
    {
        yield return new WaitForSeconds(tiempoEspera);
        
        // Verificar que los objetos aún existan
        if (jugador == null || gameObject == null)
        {
            Debug.LogWarning("⚠️ Jugador o vehículo destruido antes de reactivar colisiones");
            yield break;
        }
        
        Collider[] collidersJugador = jugador.GetComponentsInChildren<Collider>();
        Collider[] collidersVehiculo = GetComponentsInChildren<Collider>();
        
        int colisionesReactivadas = 0;
        
        foreach (Collider colliderJugador in collidersJugador)
        {
            if (colliderJugador != null && !colliderJugador.isTrigger)
            {
                foreach (Collider colliderVehiculo in collidersVehiculo)
                {
                    if (colliderVehiculo != null && !colliderVehiculo.isTrigger)
                    {
                        Physics.IgnoreCollision(colliderJugador, colliderVehiculo, false);
                        colisionesReactivadas++;
                    }
                }
            }
        }
        
        Debug.Log($"✅ Colisiones reactivadas entre {jugador.name} y vehículo: {colisionesReactivadas} pares");
    }
    
    /// <summary>
    /// Método de context menu para configurar colisiones durante vuelo
    /// </summary>
    [ContextMenu("Configurar Colisiones Durante Vuelo")]
    public void ConfigurarColisionesDuranteVuelo()
    {
        // Valores recomendados para evitar interferencia con el vehículo
        desactivarColisionesEnVuelo = true;
        tiempoExtraIgnorarColisiones = 0.5f; // Medio segundo extra después del aterrizaje
        
        Debug.Log("🔄 Configuración de colisiones durante vuelo:");
        Debug.Log($"- Desactivar colisiones en vuelo: {desactivarColisionesEnVuelo}");
        Debug.Log($"- Tiempo extra ignorar colisiones: {tiempoExtraIgnorarColisiones}s");
        Debug.Log("✅ Esto evitará que el jugador en el aire voltee el vehículo");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Verifica si un jugador está actualmente ignorando colisiones con el vehículo
    /// (Principio SRP - Single Responsibility)
    /// </summary>
    private bool EstaIgnorandoColisionesConVehiculo(GameObject jugador)
    {
        Collider jugadorCollider = jugador.GetComponent<Collider>();
        Collider vehiculoCollider = GetComponent<Collider>();
        
        if (jugadorCollider != null && vehiculoCollider != null)
        {
            // Unity no tiene un método directo para verificar si las colisiones están ignoradas
            // Usamos un pequeño truco: intentar verificar colisiones en una posición muy cercana
            return Physics.GetIgnoreCollision(jugadorCollider, vehiculoCollider);
        }
        
        return false;
    }
}
