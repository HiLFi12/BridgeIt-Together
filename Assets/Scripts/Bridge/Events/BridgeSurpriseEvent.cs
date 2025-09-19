using UnityEngine;

/// <summary>
/// Configuración para un evento sorpresa que modifica el estado del puente
/// </summary>
[System.Serializable]
public class EventoSorpresa
{
    [Header("Configuración del Evento")]
    public string nombreEvento = "Evento Sorpresa";
    
    [Header("Timing del Evento")]
    [Tooltip("Después de qué ronda se ejecuta este evento (1-based). Por ejemplo: 2 = después de completar la ronda 2")]
    public int despuesDeRonda = 1;
    
    [Header("Destrucción del Puente")]
    [Range(0, 4), Tooltip("Número de capas que permanecerán después del evento:\n• 0 = destrucción completa (destruye todas las capas 0-3)\n• 1 = solo permanece la base (destruye capas 1-3)\n• 2 = permanecen base + soporte (destruye capas 2-3)\n• 3 = permanecen base + soporte + estructura (destruye capa 3)\n• 4 = puente completo (no se destruye nada)")]
    public int capaObjetivo = 1;
    
    [Header("Configuración de Cuadrantes")]
    public bool afectarTodosLosCuadrantes = true;
    [Tooltip("Si 'afectarTodosLosCuadrantes' es false, especifica qué cuadrantes serán afectados")]
    public bool[] cuadrantesEspecificos;
    
    [Header("Efectos Adicionales")]
    [Tooltip("Tiempo de duración del efecto visual del evento (en segundos)")]
    public float duracionEfectoVisual = 2f;
    
    [Header("Debug")]
    public bool mostrarMensajesDebug = true;
}

/// <summary>
/// Script para manejar eventos sorpresa que modifican el estado del puente
/// </summary>
public class BridgeSurpriseEvent : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BridgeConstructionGrid bridgeGrid;
    [SerializeField] private BridgeInitialConstructor bridgeConstructor;
    
    [Header("Configuración de Eventos Predefinidos")]
    [SerializeField] private bool usarEventosPredefinidos = true;
    [SerializeField] private EventoSorpresa[] eventosPredefinidos = new EventoSorpresa[]
    {
        new EventoSorpresa
        {
            nombreEvento = "Colapso Parcial",
            despuesDeRonda = 2,
            capaObjetivo = 1,
            afectarTodosLosCuadrantes = true,
            duracionEfectoVisual = 2f,
            mostrarMensajesDebug = true
        },
        new EventoSorpresa
        {
            nombreEvento = "Destrucción Mayor",
            despuesDeRonda = 4,
            capaObjetivo = 0,
            afectarTodosLosCuadrantes = true,
            duracionEfectoVisual = 3f,
            mostrarMensajesDebug = true
        }
    };
    
    [Header("Debug")]
    [SerializeField] private bool mostrarDebugInfo = true;
    
    /// <summary>
    /// Ejecuta un evento sorpresa específico
    /// </summary>
    public void EjecutarEventoSorpresa(EventoSorpresa evento)
    {
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
            if (bridgeGrid == null)
            {
                Debug.LogError("BridgeSurpriseEvent: No se encontró BridgeConstructionGrid en la escena");
                return;
            }
        }
        
        if (bridgeConstructor == null)
        {
            bridgeConstructor = FindObjectOfType<BridgeInitialConstructor>();
            if (bridgeConstructor == null)
            {
                Debug.LogError("BridgeSurpriseEvent: No se encontró BridgeInitialConstructor en la escena");
                return;
            }
        }
        
        if (evento.mostrarMensajesDebug || mostrarDebugInfo)
        {
            Debug.Log($"🎭 EVENTO SORPRESA: '{evento.nombreEvento}' - Destruyendo puente hasta la capa {evento.capaObjetivo}");
        }
        
        // Ejecutar la destrucción del puente
        DestruirPuenteHastaCapa(evento);
        
        if (evento.mostrarMensajesDebug || mostrarDebugInfo)
        {
            Debug.Log($"🎭 Evento sorpresa '{evento.nombreEvento}' completado");
        }
    }
    
    /// <summary>
    /// Destruye el puente hasta la capa especificada
    /// </summary>
    private void DestruirPuenteHastaCapa(EventoSorpresa evento)
    {
        int cuadrantesAfectados = 0;
        int capasDestruidas = 0;
        
        // Recorrer toda la grilla del puente
        for (int x = 0; x < bridgeGrid.gridWidth; x++)
        {
            for (int z = 0; z < bridgeGrid.gridLength; z++)
            {
                // Verificar si debemos afectar este cuadrante específico
                if (!DebeAfectarCuadrante(evento, x, z))
                    continue;
                
                // Verificar si es un cuadrante válido
                if (!EsCuadranteValido(x, z))
                    continue;
                
                bool cuadranteAfectado = false;
                
                // Obtener el ScriptableObject del cuadrante
                BridgeQuadrantSO quadrantSO = bridgeGrid.GetQuadrantSO(x, z);
                if (quadrantSO != null)
                {
                    // Destruir capas desde la capa superior hacia abajo según capaObjetivo
                    // capaObjetivo = número de capas que PERMANECEN (0=destruir todo, 1=solo base, 2=base+soporte, etc.)
                    // Ejemplo: capaObjetivo=0 significa destruir todas (capas 3,2,1,0), capaObjetivo=1 significa que solo queda la capa 0 (destruir capas 3,2,1)
                    
                    if (evento.mostrarMensajesDebug)
                    {
                        Debug.Log($"  🔍 Cuadrante [{x},{z}]: capaObjetivo={evento.capaObjetivo}, destruyendo capas desde 3 hasta {evento.capaObjetivo}");
                    }
                    
                    for (int layer = 3; layer > evento.capaObjetivo - 1; layer--)
                    {
                        if (quadrantSO.requiredLayers[layer].isCompleted)
                        {
                            // Marcar la capa como no completada en el ScriptableObject
                            quadrantSO.requiredLayers[layer].isCompleted = false;
                            capasDestruidas++;
                            cuadranteAfectado = true;
                            
                            // NUEVO: Desactivar/destruir el GameObject visual de la capa
                            DestruirCapaVisual(x, z, layer);
                            
                            if (evento.mostrarMensajesDebug)
                            {
                                Debug.Log($"  ✗ Capa {layer} destruida en cuadrante [{x},{z}] (objetivo: conservar {evento.capaObjetivo} capas)");
                            }
                        }
                        else if (evento.mostrarMensajesDebug)
                        {
                            Debug.Log($"  ⚪ Capa {layer} en cuadrante [{x},{z}] ya estaba incompleta");
                        }
                    }
                    
                    // Si se destruyeron capas, actualizar el estado del cuadrante
                    if (cuadranteAfectado)
                    {
                        // Verificar si todavía hay colisión después de la destrucción
                        bool tieneCapasRestantes = false;
                        
                        // Si capaObjetivo = 0, no deben quedar capas
                        // Si capaObjetivo > 0, verificar si las capas que deberían permanecer están completadas
                        if (evento.capaObjetivo > 0)
                        {
                            for (int i = 0; i < evento.capaObjetivo; i++)
                            {
                                if (quadrantSO.requiredLayers[i].isCompleted)
                                {
                                    tieneCapasRestantes = true;
                                    break;
                                }
                            }
                        }
                        // Si capaObjetivo = 0, tieneCapasRestantes debe ser false (ya está inicializado así)
                        
                        if (evento.mostrarMensajesDebug)
                        {
                            Debug.Log($"  🔍 Estado post-destrucción en cuadrante [{x},{z}]: tieneCapasRestantes={tieneCapasRestantes}");
                            for (int debugLayer = 0; debugLayer < 4; debugLayer++)
                            {
                                bool estaCompleta = quadrantSO.requiredLayers[debugLayer].isCompleted;
                                Debug.Log($"    📊 Capa {debugLayer}: {(estaCompleta ? "✅ Completa" : "❌ Destruida")}");
                            }
                        }
                        
                        // Actualizar estado de colisión
                        quadrantSO.hasCollision = tieneCapasRestantes;
                        
                        // Si no quedan capas, marcar como destruido
                        if (!tieneCapasRestantes)
                        {
                            quadrantSO.lastLayerState = BridgeQuadrantSO.LastLayerState.Destroyed;
                        }
                        else
                        {
                            // Si quedan capas, verificar el estado de la última capa restante
                            quadrantSO.lastLayerState = BridgeQuadrantSO.LastLayerState.Complete;
                        }
                        
                        cuadrantesAfectados++;
                    }
                }
            }
        }
        
        // Forzar actualización visual del puente
        bridgeGrid.ApplyCurrentScales();
        
        if (evento.mostrarMensajesDebug || mostrarDebugInfo)
        {
            string descripcionObjetivo = evento.capaObjetivo switch
            {
                0 => "destrucción completa (todas las capas eliminadas)",
                1 => "solo permanece la base (capa 0)",
                2 => "permanecen base + soporte (capas 0-1)",
                3 => "permanecen base + soporte + estructura (capas 0-2)",
                4 => "puente completo (sin destrucción)",
                _ => $"{evento.capaObjetivo} capas permanecen"
            };
            
            Debug.Log($"🎭 Destrucción completada: {cuadrantesAfectados} cuadrantes afectados, {capasDestruidas} capas destruidas. Resultado: {descripcionObjetivo}");
        }
    }
    
    /// <summary>
    /// Destruye o desactiva el GameObject visual de una capa específica
    /// </summary>
    private void DestruirCapaVisual(int x, int z, int layer)
    {
        try
        {
            // Calcular la posición mundial del cuadrante (igual que en BridgeConstructionGrid)
            Vector3 worldPosition = bridgeGrid.transform.position + new Vector3(x * bridgeGrid.quadrantSize, 0, z * bridgeGrid.quadrantSize);
            
            // MÉTODO 1: Buscar directamente en la jerarquía del cuadrante (más preciso)
            bool objetoEncontrado = DestruirCapaEnJerarquia(x, z, layer);
            
            if (!objetoEncontrado)
            {
                // MÉTODO 2: Buscar por posición como método de respaldo
                GameObject[] objetosEnPosicion = EncontrarObjetosEnPosicion(worldPosition, layer);
                
                foreach (GameObject obj in objetosEnPosicion)
                {
                    if (obj != null && EsObjetoDeCapaEspecifica(obj, layer))
                    {
                        if (mostrarDebugInfo)
                        {
                            Debug.Log($"    🗑️ [Respaldo] Destruyendo objeto visual: {obj.name} en posición [{x},{z}] capa {layer}");
                        }
                        
                        // Desactivar el objeto (más seguro que destruir inmediatamente)
                        obj.SetActive(false);
                        
                        // Destruir después de un pequeño delay para efectos visuales
                        if (Application.isPlaying)
                        {
                            Destroy(obj, 0.1f);
                        }
                        objetoEncontrado = true;
                    }
                }
            }
            
            if (!objetoEncontrado && mostrarDebugInfo)
            {
                Debug.LogWarning($"    ⚠️ No se encontró objeto visual para la capa {layer} en cuadrante [{x},{z}]");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error al destruir capa visual en [{x},{z}] capa {layer}: {e.Message}");
        }
    }
    
    /// <summary>
    /// Busca y destruye la capa específica directamente en la jerarquía del cuadrante
    /// </summary>
    private bool DestruirCapaEnJerarquia(int x, int z, int layer)
    {
        try
        {
            // Usar reflexión para acceder al constructionGrid (similar a como lo hacen otros scripts)
            System.Type gridType = bridgeGrid.GetType();
            System.Reflection.FieldInfo gridField = gridType.GetField("constructionGrid", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (gridField != null)
            {
                System.Array grid = gridField.GetValue(bridgeGrid) as System.Array;
                if (grid != null)
                {
                    // Obtener el objeto QuadrantInfo en [x,z]
                    object quadrantInfo = grid.GetValue(x, z);
                    if (quadrantInfo != null)
                    {
                        System.Type quadrantInfoType = quadrantInfo.GetType();
                        
                        // Obtener el quadrantObject
                        System.Reflection.FieldInfo quadrantObjectField = quadrantInfoType.GetField("quadrantObject");
                        GameObject quadrantObject = quadrantObjectField?.GetValue(quadrantInfo) as GameObject;
                        
                        if (quadrantObject != null)
                        {
                            // Buscar la capa específica por nombre
                            string[] layerNames = { "base", "support", "structure", "surface" };
                            if (layer >= 0 && layer < layerNames.Length)
                            {
                                // Intentar múltiples variaciones del nombre
                                string[] possibleNames = {
                                    $"Layer_{layer}_{layerNames[layer]}",
                                    $"layer_{layer}_{layerNames[layer]}",
                                    $"Layer{layer}_{layerNames[layer]}",
                                    $"Layer_{layer}",
                                    layerNames[layer],
                                    $"{layerNames[layer]}_{layer}"
                                };
                                
                                Transform layerTransform = null;
                                string foundName = "";
                                
                                foreach (string possibleName in possibleNames)
                                {
                                    layerTransform = quadrantObject.transform.Find(possibleName);
                                    if (layerTransform != null)
                                    {
                                        foundName = possibleName;
                                        break;
                                    }
                                }
                                
                                // Si no se encuentra por nombre exacto, buscar por contenido de nombre
                                if (layerTransform == null)
                                {
                                    for (int childIndex = 0; childIndex < quadrantObject.transform.childCount; childIndex++)
                                    {
                                        Transform child = quadrantObject.transform.GetChild(childIndex);
                                        string childName = child.name.ToLower();
                                        
                                        if (childName.Contains(layerNames[layer]) || childName.Contains($"layer_{layer}") || childName.Contains($"layer{layer}"))
                                        {
                                            layerTransform = child;
                                            foundName = child.name;
                                            break;
                                        }
                                    }
                                }
                                
                                if (layerTransform != null)
                                {
                                    if (mostrarDebugInfo)
                                    {
                                        Debug.Log($"    🗑️ [Jerarquía] Destruyendo capa específica: {foundName} en cuadrante [{x},{z}]");
                                    }
                                    
                                    // Desactivar la capa específica
                                    layerTransform.gameObject.SetActive(false);
                                    
                                    // Destruir después de un pequeño delay
                                    if (Application.isPlaying)
                                    {
                                        Destroy(layerTransform.gameObject, 0.1f);
                                    }
                                    
                                    // IMPORTANTE: Limpiar la referencia en layerRenderers
                                    LimpiarReferenciaRenderer(quadrantInfo, layer);
                                    
                                    return true;
                                }
                                else if (mostrarDebugInfo)
                                {
                                    Debug.LogWarning($"    ⚠️ No se encontró ninguna variación del nombre para la capa {layer} ({layerNames[layer]}) en cuadrante [{x},{z}]");
                                    Debug.Log($"    🔍 Hijos disponibles en quadrantObject:");
                                    for (int childIndex = 0; childIndex < quadrantObject.transform.childCount; childIndex++)
                                    {
                                        Transform child = quadrantObject.transform.GetChild(childIndex);
                                        Debug.Log($"      - {child.name}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error al buscar en jerarquía para cuadrante [{x},{z}] capa {layer}: {e.Message}");
        }
        
        return false;
    }
    
    /// <summary>
    /// Limpia la referencia en el array layerRenderers
    /// </summary>
    private void LimpiarReferenciaRenderer(object quadrantInfo, int layer)
    {
        try
        {
            System.Type quadrantInfoType = quadrantInfo.GetType();
            System.Reflection.FieldInfo layerRenderersField = quadrantInfoType.GetField("layerRenderers");
            
            if (layerRenderersField != null)
            {
                Renderer[] layerRenderers = layerRenderersField.GetValue(quadrantInfo) as Renderer[];
                if (layerRenderers != null && layer >= 0 && layer < layerRenderers.Length)
                {
                    layerRenderers[layer] = null;
                    if (mostrarDebugInfo)
                    {
                        Debug.Log($"    🧹 Referencia de renderer limpiada para capa {layer}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error al limpiar referencia de renderer para capa {layer}: {e.Message}");
        }
    }
    
    /// <summary>
    /// Encuentra GameObjects en una posición específica que correspondan a una capa
    /// </summary>
    private GameObject[] EncontrarObjetosEnPosicion(Vector3 posicion, int layer)
    {
        System.Collections.Generic.List<GameObject> objetosEncontrados = new System.Collections.Generic.List<GameObject>();
        
        // Buscar en un radio pequeño alrededor de la posición
        float radio = 0.5f;
        Collider[] collidersEnArea = Physics.OverlapSphere(posicion, radio);
        
        foreach (Collider col in collidersEnArea)
        {
            GameObject obj = col.gameObject;
            
            // Verificar si el objeto pertenece al puente
            if (EsObjetoDelPuente(obj, layer))
            {
                objetosEncontrados.Add(obj);
            }
        }
        
        // Si no encontramos nada con OverlapSphere, buscar por nombre/tag
        if (objetosEncontrados.Count == 0)
        {
            objetosEncontrados.AddRange(BuscarObjetosPorNombreYPosicion(posicion, layer));
        }
        
        return objetosEncontrados.ToArray();
    }
    
    /// <summary>
    /// Verifica si un GameObject pertenece al puente y a una capa específica
    /// </summary>
    private bool EsObjetoDelPuente(GameObject obj, int layer)
    {
        // Verificar por tag (solo si los tags existen)
        try 
        {
            if (obj.CompareTag("BridgePiece") || obj.CompareTag("Bridge"))
            {
                return true;
            }
        }
        catch (UnityEngine.UnityException)
        {
            // Los tags no existen, continuar con otras verificaciones
        }
        
        // Verificar por nombre (buscar patrones comunes)
        string nombreObj = obj.name.ToLower();
        
        // Patrones de nombres según la capa
        string[] patronesPorCapa = {
            "base", // Capa 0
            "support", // Capa 1  
            "structure", // Capa 2
            "surface" // Capa 3
        };
        
        if (layer >= 0 && layer < patronesPorCapa.Length)
        {
            if (nombreObj.Contains(patronesPorCapa[layer]))
            {
                return true;
            }
        }
        
        // Verificar patrones generales de puente
        if (nombreObj.Contains("bridge") || nombreObj.Contains("puente") || 
            nombreObj.Contains("layer") || nombreObj.Contains("capa"))
        {
            return true;
        }
        
        // Verificar si el objeto tiene un componente relacionado con el puente
        if (obj.GetComponent<BridgeMaterialInfo>() != null)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Verifica si un GameObject es específicamente una capa del puente (más selectivo)
    /// </summary>
    private bool EsObjetoDeCapaEspecifica(GameObject obj, int layer)
    {
        string nombreObj = obj.name.ToLower();
        
        // Evitar destruir objetos contenedores principales
        if (nombreObj.Contains("quadrant") || nombreObj.Contains("cuadrante") || 
            nombreObj.Contains("container") || nombreObj.Contains("grid"))
        {
            return false;
        }
        
        // Verificar que sea específicamente un objeto de capa
        string[] patronesPorCapa = {
            "base", // Capa 0
            "support", // Capa 1  
            "structure", // Capa 2
            "surface" // Capa 3
        };
        
        if (layer >= 0 && layer < patronesPorCapa.Length)
        {
            // Buscar el patrón específico de la capa
            if (nombreObj.Contains($"layer_{layer}") || nombreObj.Contains(patronesPorCapa[layer]))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Busca objetos por nombre y proximidad a una posición
    /// </summary>
    private GameObject[] BuscarObjetosPorNombreYPosicion(Vector3 posicion, int layer)
    {
        System.Collections.Generic.List<GameObject> objetosEncontrados = new System.Collections.Generic.List<GameObject>();
        
        // Buscar todos los GameObjects que podrían ser del puente
        GameObject[] todosLosObjetos = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in todosLosObjetos)
        {
            // Verificar si está cerca de la posición
            if (Vector3.Distance(obj.transform.position, posicion) <= 1.0f)
            {
                if (EsObjetoDelPuente(obj, layer))
                {
                    objetosEncontrados.Add(obj);
                }
            }
        }
        
        return objetosEncontrados.ToArray();
    }

    /// <summary>
    /// Verifica si debemos afectar un cuadrante específico según la configuración del evento
    /// </summary>
    private bool DebeAfectarCuadrante(EventoSorpresa evento, int x, int z)
    {
        if (evento.afectarTodosLosCuadrantes)
            return true;
        
        if (evento.cuadrantesEspecificos == null || evento.cuadrantesEspecificos.Length == 0)
            return true;
        
        // Calcular índice lineal del cuadrante
        int index = x * bridgeGrid.gridLength + z;
        
        if (index >= 0 && index < evento.cuadrantesEspecificos.Length)
            return evento.cuadrantesEspecificos[index];
        
        return false;
    }
    
    /// <summary>
    /// Verifica si un cuadrante es válido
    /// </summary>
    private bool EsCuadranteValido(int x, int z)
    {
        // Verificación básica de límites
        if (x < 0 || x >= bridgeGrid.gridWidth || z < 0 || z >= bridgeGrid.gridLength)
            return false;
        
        // Usar el método público IsValidQuadrant del BridgeConstructionGrid
        return bridgeGrid.IsValidQuadrant(x, z);
    }
    
    /// <summary>
    /// Método de debug para probar un evento sorpresa
    /// </summary>
    [ContextMenu("Probar Evento Sorpresa")]
    public void ProbarEventoSorpresa()
    {
        EventoSorpresa eventoTest = new EventoSorpresa
        {
            nombreEvento = "Evento de Prueba",
            capaObjetivo = 1,
            afectarTodosLosCuadrantes = true,
            duracionEfectoVisual = 2f,
            mostrarMensajesDebug = true
        };
        
        EjecutarEventoSorpresa(eventoTest);
    }
    
    /// <summary>
    /// Obtiene un evento predefinido por su índice
    /// </summary>
    public EventoSorpresa ObtenerEventoPredefinido(int indice)
    {
        if (!usarEventosPredefinidos || eventosPredefinidos == null || indice < 0 || indice >= eventosPredefinidos.Length)
        {
            return null;
        }
        
        return eventosPredefinidos[indice];
    }
    
    /// <summary>
    /// Obtiene el número de eventos predefinidos disponibles
    /// </summary>
    public int GetCantidadEventosPredefinidos()
    {
        if (!usarEventosPredefinidos || eventosPredefinidos == null)
        {
            return 0;
        }
        
        return eventosPredefinidos.Length;
    }
    
    /// <summary>
    /// Busca un evento configurado para ejecutarse después de una ronda específica
    /// </summary>
    public EventoSorpresa BuscarEventoPorRonda(int rondaCompletada)
    {
        if (!usarEventosPredefinidos || eventosPredefinidos == null)
        {
            return null;
        }
        
        // Buscar el primer evento que coincida con la ronda
        for (int i = 0; i < eventosPredefinidos.Length; i++)
        {
            if (eventosPredefinidos[i] != null && eventosPredefinidos[i].despuesDeRonda == rondaCompletada)
            {
                return eventosPredefinidos[i];
            }
        }
        
        return null; // No se encontró ningún evento para esta ronda
    }
    
    /// <summary>
    /// Método para probar destrucción completa del puente
    /// </summary>
    [ContextMenu("Destruir Puente Completamente")]
    public void DestruirPuenteCompleto()
    {
        EventoSorpresa eventoDestruccionTotal = new EventoSorpresa
        {
            nombreEvento = "Destrucción Total",
            capaObjetivo = 0,
            afectarTodosLosCuadrantes = true,
            duracionEfectoVisual = 3f,
            mostrarMensajesDebug = true
        };
        
        EjecutarEventoSorpresa(eventoDestruccionTotal);
    }
    
    /// <summary>
    /// Método para probar destrucción parcial del puente
    /// </summary>
    [ContextMenu("Destruir Hasta Capa Base")]
    public void DestruirHastaCapaBase()
    {
        EventoSorpresa eventoDestruccionParcial = new EventoSorpresa
        {
            nombreEvento = "Destrucción Hasta Base",
            capaObjetivo = 1,
            afectarTodosLosCuadrantes = true,
            duracionEfectoVisual = 2f,
            mostrarMensajesDebug = true
        };
        
        EjecutarEventoSorpresa(eventoDestruccionParcial);
    }
}
