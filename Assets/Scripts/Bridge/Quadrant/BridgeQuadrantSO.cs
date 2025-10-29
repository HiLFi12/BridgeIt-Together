using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBridgeQuadrant", menuName = "Bridge/Quadrant")]
public class BridgeQuadrantSO : ScriptableObject, ITurnable
{
    [System.Serializable]
    public class LayerInfo
    {
        public string layerName;
        public GameObject visualPrefab;
        public Material material;
        [HideInInspector] public bool isCompleted = false;
    }

    [System.Serializable]
    public enum LastLayerState
    {
        Complete,
        Damaged,
        Destroyed
    }

    [System.Serializable]
    public enum EraType
    {
        Prehistoric,
        Medieval,
        Industrial,
        Contemporary,
        Futuristic
    }

    /// <summary>
    /// Enum para tipos de materiales compatible con el material de superficie
    /// </summary>
    [System.Serializable]
    public enum MaterialType
    {
        Adoquin,
        Wood,
        Stone,
        Metal
    }

    [Header("Configuración General")]
    public EraType era;
    public LayerInfo[] requiredLayers = new LayerInfo[3];
    public bool hasCollision = false;
    public LastLayerState lastLayerState = LastLayerState.Complete;

    [Header("Estado de Última Capa - Específico de Era")]
    public int maxUsesBeforeDamage = 10;
    public int currentUses = 0;

    public float maxTemperature = 100f;
    public float currentTemperature = 100f;
    public float temperatureDecayRate = 5f;

    public float damageChance = 0.3f;

    public float batteryLife = 100f;
    public float batteryDrainRate = 5f;

    [Header("Calor / Agua")]
    [Tooltip("Indica si actualmente el cuadrante recibe calor (true) o no (false). Afecta al decaimiento.")]
    public bool isTurned { get; private set; } // Estado visible para otros sistemas, controlado por ITurnable/agua
    [Tooltip("Marcador interno de si hay una fuente de calor aplicando (ignora agua).")]
    public bool heatActive = false;

    [Header("Debug Vida (Industrial)")]
    [Tooltip("Si está activo, loggea periódicamente la vida del puente como currentTemperature/maxTemperature.")]
    public bool debugLife = false;
    [Tooltip("Intervalo entre logs de debug en segundos.")]
    public float debugLifeInterval = 0.5f;
    private float _debugLifeTimer = 0f;

    [Header("Efectos Visuales")]
    public Material damagedMaterial;
    public Material destroyedMaterial;
    public GameObject destructionEffectPrefab;

    [Header("Sonidos")]
    public AudioClip constructionSound;
    public AudioClip damageSound;
    public AudioClip destructionSound;
    public AudioClip repairSound;

    [Header("Vida Unificada")]
    [Tooltip("Vida máxima normalizada (default 100). Para Futurista/Industrial se mapea a battery/temperature.")]
    public float maxLife = 100f;
    [Tooltip("Vida actual para eras no Futurista/Industrial.")]
    public float currentLife = 100f;
    [Range(0f, 1f)]
    [Tooltip("Umbral (fracción) a partir del cual el cuadrante entra en estado Damaged.")]
    public float damagedThreshold01 = 0.40f;

    [Header("Daño por impacto (fallback)")]
    [Tooltip("Daño aplicado por impacto cuando se usa el camino antiguo (grid.OnVehicleImpact).")]
    public float damagePerImpact = 5f;

    public void Initialize()
    {
        hasCollision = false;
        lastLayerState = LastLayerState.Complete;

        foreach (var layer in requiredLayers)
            layer.isCompleted = false;

        ResetEraSpecificState();

        // Inicializar vida unificada
        currentLife = maxLife;
        // Futurista/Industrial usan sus propios depósitos (battery/temperature), pero el ratio de vida se obtiene de ellos.
    }

    private void ResetEraSpecificState()
    {
        switch (era)
        {
            case EraType.Prehistoric:
            case EraType.Medieval:
                currentUses = 0;
                // Vida completa al iniciar
                currentLife = maxLife;
                break;
            case EraType.Industrial:
                currentTemperature = maxTemperature;
                break;
            case EraType.Contemporary:
                currentLife = maxLife;
                break;
            case EraType.Futuristic:
                batteryLife = 100f;
                break;
        }
        heatActive = false;
        RecalculateTurned();
    }

    // Vida unificada (ratio 0..1)
    public float GetLifeRatio()
    {
        switch (era)
        {
            case EraType.Industrial:
                return maxTemperature > 0f ? Mathf.Clamp01(currentTemperature / maxTemperature) : 0f;
            case EraType.Futuristic:
                return Mathf.Clamp01(batteryLife / 100f);
            default:
                return maxLife > 0f ? Mathf.Clamp01(currentLife / maxLife) : 0f;
        }
    }

    private void SetLifeByRatio(float ratio01)
    {
        ratio01 = Mathf.Clamp01(ratio01);
        switch (era)
        {
            case EraType.Industrial:
                currentTemperature = maxTemperature * ratio01;
                break;
            case EraType.Futuristic:
                batteryLife = 100f * ratio01;
                break;
            default:
                currentLife = maxLife * ratio01;
                break;
        }
    }

    // Daño genérico en puntos de “vida” (mismo rango que battery/temperature: 100 = vida completa)
    public void ApplyGenericDamage(float amount)
    {
        if (amount <= 0f) return;

        switch (era)
        {
            case EraType.Industrial:
                currentTemperature = Mathf.Max(0f, currentTemperature - amount);
                break;
            case EraType.Futuristic:
                batteryLife = Mathf.Max(0f, batteryLife - amount);
                break;
            default:
                currentLife = Mathf.Max(0f, currentLife - amount);
                break;
        }

        EvaluateStateFromLife();
    }

    // Aplica estados Damaged/Destroyed según el ratio de vida
    private void EvaluateStateFromLife()
    {
        float r = GetLifeRatio();

        if (r <= 0f)
        {
            lastLayerState = LastLayerState.Destroyed;
            DestroyLastLayer();
            return;
        }

        // Cambiar a "< 40%" (antes usaba <=)
        if (r < damagedThreshold01 && lastLayerState == LastLayerState.Complete)
        {
            lastLayerState = LastLayerState.Damaged;
        }
        else if (r > damagedThreshold01 && lastLayerState == LastLayerState.Damaged)
        {
            lastLayerState = LastLayerState.Complete;
        }
    }

    public bool TryAddLayer(int layerIndex, GameObject layerObject)
    {
        if (layerIndex < 0 || layerIndex >= requiredLayers.Length)
        {
            Debug.LogError($"Índice de capa {layerIndex} fuera de rango [0-{requiredLayers.Length-1}]");
            return false;
        }

        string estadoActual = "";
        for (int i = 0; i < requiredLayers.Length; i++)
        {
            estadoActual += $"Capa {i}: {(requiredLayers[i].isCompleted ? "Completada" : "Incompleta")}, ";
        }
        Debug.Log($"[TryAddLayer] Estado actual del cuadrante: {estadoActual}");

        if (requiredLayers[layerIndex].isCompleted)
        {
            if (layerIndex == requiredLayers.Length - 1 && lastLayerState == LastLayerState.Damaged)
            {
                Debug.Log($"Reparando última capa dañada (capa {layerIndex})");
                lastLayerState = LastLayerState.Complete;
                ResetEraSpecificState();
                return true;
            }
            
            Debug.LogError($"ERROR: Capa {layerIndex} ya está completada y no necesita reparación.");
            return false;
        }
        
        int primerCapaIncompleta = -1;
        for (int i = 0; i < requiredLayers.Length; i++)
        {
            if (!requiredLayers[i].isCompleted)
            {
                primerCapaIncompleta = i;
                break;
            }
        }
        
        if (layerIndex != primerCapaIncompleta)
        {
            Debug.LogError($"ERROR DE SECUENCIA: Debes construir primero la capa {primerCapaIncompleta}, no la capa {layerIndex}");
            return false;
        }
        
        for (int i = 0; i < layerIndex; i++)
        {
            if (!requiredLayers[i].isCompleted)
            {
                Debug.LogError($"ERROR: No se puede construir capa {layerIndex} porque la capa {i} no está completada.");
                return false;
            }
        }

        if (layerIndex == 0 && lastLayerState == LastLayerState.Destroyed)
        {
            Debug.Log("Reconstruyendo cuadrante después de destrucción. Reseteando estado.");
            lastLayerState = LastLayerState.Complete;
        }

        requiredLayers[layerIndex].isCompleted = true;
        Debug.Log($"ÉXITO: Capa {layerIndex} marcada como completada.");

        CheckIfAllLayersCompleted();

        string estadoPosterior = "";
        for (int i = 0; i < requiredLayers.Length; i++)
        {
            estadoPosterior += $"Capa {i}: {(requiredLayers[i].isCompleted ? "Completada" : "Incompleta")}, ";
        }
        Debug.Log($"[TryAddLayer] Estado posterior: {estadoPosterior}");
        
        return true;
    }

    private void CheckIfAllLayersCompleted()
    {
        bool firstLayerCompleted = requiredLayers.Length > 0 && requiredLayers[0].isCompleted;
        hasCollision = firstLayerCompleted;

        bool allLayersCompleted = requiredLayers[requiredLayers.Length - 1].isCompleted;
        if (allLayersCompleted && lastLayerState != LastLayerState.Complete)
        {
            lastLayerState = LastLayerState.Complete;
        }

        // Al completar todas las capas, reestablecer vida completa
        if (allLayersCompleted)
        {
            switch (era)
            {
                case EraType.Industrial: currentTemperature = maxTemperature; break;
                case EraType.Futuristic: batteryLife = 100f; break;
                default: currentLife = maxLife; break;
            }
        }
    }

    public void UpdateQuadrantState(float deltaTime)
    {
        if (!hasCollision) return;

        switch (era)
        {
            case EraType.Industrial:
                if (lastLayerState != LastLayerState.Destroyed)
                {
                    if (!isTurned)
                    {
                        currentTemperature -= temperatureDecayRate * deltaTime;
                        if (currentTemperature < 0f) currentTemperature = 0f;
                    }

                    // Umbral de dañado al 40%
                    if (lastLayerState == LastLayerState.Complete && GetLifeRatio() < damagedThreshold01)
                        lastLayerState = LastLayerState.Damaged;

                    if (currentTemperature <= 0f)
                    {
                        lastLayerState = LastLayerState.Destroyed;
                        DestroyQuadrant();
                    }
                }

                // Mini debug de vida (Industrial): current/max
                if (debugLife && maxTemperature > 0f)
                {
                    _debugLifeTimer -= deltaTime;
                    if (_debugLifeTimer <= 0f)
                    {
                        float ratio = Mathf.Clamp01(currentTemperature / maxTemperature);
                    //    Debug.Log($"[BridgeQuadrantSO] '{name}' Vida: {currentTemperature:F1}/{maxTemperature:F1} ({ratio:P0}) | isTurned={isTurned} | waterBlockers={waterBlockers} | heatActive={heatActive} | state={lastLayerState}");
                        _debugLifeTimer = Mathf.Max(0.1f, debugLifeInterval);
                    }
                }
                break;
            
            case EraType.Futuristic:
                if (lastLayerState != LastLayerState.Destroyed)
                {
                    if (lastLayerState == LastLayerState.Complete || lastLayerState == LastLayerState.Damaged)
                    {
                        batteryLife -= batteryDrainRate * deltaTime;
                        batteryLife = Mathf.Max(0f, batteryLife);

                        if (lastLayerState == LastLayerState.Complete && GetLifeRatio() < damagedThreshold01)
                            lastLayerState = LastLayerState.Damaged;

                        if (batteryLife <= 0f)
                        {
                            lastLayerState = LastLayerState.Destroyed;
                            DestroyLastLayer();
                        }
                    }
                }
                break;

            default:
                // Prehistoric/Medieval/Contemporary no tienen decay pasivo por defecto
                break;
        }
    }

    public void OnVehicleImpact()
    {
        if (!requiredLayers[requiredLayers.Length - 1].isCompleted)
        {
            Debug.Log("Vehículo cayó en cuadrante incompleto. Destruyendo todas las capas.");
            DestroyQuadrant();
            return;
        }

        // Unificar daño: aplicar daño genérico por impacto
        ApplyGenericDamage(damagePerImpact);
    }

    public void ApplyHeat()
    {
        if (era == EraType.Industrial && lastLayerState != LastLayerState.Destroyed)
        {
            heatActive = true;
            RecalculateTurned();
        }
    }

    /// <summary>
    /// Indica que ya no se aplica calor (por ejemplo fuente removida)
    /// </summary>
    public void RemoveHeat()
    {
        heatActive = false;
        RecalculateTurned();
    }

    // Implementación ITurnable para que HeatSphere pueda controlar el calor sin modificar su script
    public void TurnOn()
    {
        ApplyHeat();
    }

    public void TurnOff()
    {
        RemoveHeat();
    }

    /// <summary>
    /// Llamar cuando agua (script Water) comienza a tocar el cuadrante.
    /// </summary>
    public void AddWaterBlocker()
    {
        // Deprecated: mantenido para compatibilidad si otros scripts aún llaman.
        RemoveHeat(); // fuerza apagado si agua antigua llama este método
    }

    /// <summary>
    /// Llamar cuando agua deja de tocar el cuadrante.
    /// </summary>
    public void RemoveWaterBlocker()
    {
        // Deprecated: si se usaba, al salir agua re-evaluará turned (ya gestionado fuera)
        RecalculateTurned();
    }

    private void RecalculateTurned()
    {
        // isTurned true solo si hay calor activo y no está bloqueado por agua.
        bool newTurned = heatActive; // Sin waterBlockers; agua se maneja externamente
        isTurned = newTurned;
    }

    public void ReplaceBattery()
    {
        if (era == EraType.Futuristic && lastLayerState != LastLayerState.Destroyed)
        {
            batteryLife = 100f;
            lastLayerState = LastLayerState.Complete;
        }
    }

    private void DestroyLastLayer()
    {
        if (requiredLayers.Length > 0)
        {
            requiredLayers[requiredLayers.Length - 1].isCompleted = false;
        }
        
        CheckIfAllLayersCompleted();
    }

    private void DestroyQuadrant()
    {
        foreach (var layer in requiredLayers)
        {
            layer.isCompleted = false;
        }
        hasCollision = false;
        lastLayerState = LastLayerState.Destroyed;
        
        if (destructionEffectPrefab != null)
        {
            Debug.Log("Efecto de colapso disponible para reproducir");
        }
    }

    /// <summary>
    /// Forzar la destrucción completa del cuadrante desde código externo (ej. vehículo).
    /// Envuelve el método privado DestroyQuadrant para que otros componentes puedan invocarlo.
    /// </summary>
    public void ForceDestroyQuadrant()
    {
        Debug.Log($"[BridgeQuadrantSO] ForceDestroyQuadrant called on '{name}'");
        DestroyQuadrant();
    }

    /// <summary>
    /// Verifica si el cuadrante está dañado
    /// </summary>
    /// <returns>True si está dañado</returns>
    public bool IsDamaged()
    {
        return lastLayerState == LastLayerState.Damaged;
    }

    /// <summary>
    /// Obtiene el estado actual de la última capa
    /// </summary>
    /// <returns>Estado de la última capa</returns>
    public LastLayerState GetLastLayerState()
    {
        return lastLayerState;
    }

    /// <summary>
    /// Establece el estado de la última capa (para pruebas)
    /// </summary>
    /// <param name="state">Nuevo estado</param>
    public void SetLastLayerState(LastLayerState state)
    {
        lastLayerState = state;
    }

    /// <summary>
    /// Sobrecarga del método TryAddLayer que acepta MaterialType
    /// </summary>
    /// <param name="materialType">Tipo de material</param>
    /// <param name="cantidad">Cantidad de material</param>
    /// <returns>True si se pudo agregar</returns>
    public bool TryAddLayer(MaterialType materialType, int cantidad)
    {
        // Para la compatibilidad con el sistema de reparación
        if (materialType == MaterialType.Adoquin && lastLayerState == LastLayerState.Damaged)
        {
            Debug.Log($"Reparando con material adoquín - cantidad: {cantidad}");
            lastLayerState = LastLayerState.Complete;
            ResetEraSpecificState();
            return true;
        }

        // Para construcción normal, usar el método existente
        // Buscar la primera capa incompleta
        for (int i = 0; i < requiredLayers.Length; i++)
        {
            if (!requiredLayers[i].isCompleted)
            {
                return TryAddLayer(i, null);
            }
        }

        return false;
    }
}

