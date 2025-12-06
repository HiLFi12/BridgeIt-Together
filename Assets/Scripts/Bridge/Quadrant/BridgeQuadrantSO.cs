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

    // Deprecated (Industrial ahora usa vida unificada maxLife/currentLife)
    [HideInInspector] public float maxTemperature = 100f; // mantenido para compatibilidad si otros scripts aún lo consultan
    [HideInInspector] public float currentTemperature = 100f; // ya no se modifica en Industrial
    [Tooltip("Tasa de decaimiento de vida para era Industrial (antes temperatureDecayRate).")]
    public float temperatureDecayRate = 5f; // reutilizado como decay de currentLife en Industrial

    public float damageChance = 0.3f;

    public float batteryLife = 100f;
    public float batteryDrainRate = 5f;

    [Header("Calor / Agua")]
    [Tooltip("Indica si actualmente el cuadrante recibe calor (true) o no (false). Afecta al decaimiento.")]
    public bool isTurned { get; private set; } // Estado visible para otros sistemas, controlado por ITurnable/agua
    [Tooltip("Marcador interno de si hay una fuente de calor aplicando (ignora agua).")]
    public bool heatActive = false;
    [Tooltip("Indica si el cuadrante está actualmente mojado por agua.")]
    public bool isWet = false;

    [Header("Daño por Agua + Calor (Industrial)")]
    [Tooltip("Daño por segundo cuando el cuadrante está mojado y recibiendo calor.")]
    public float wetHeatDamagePerSecond = 1f;
    [Tooltip("Tiempo en segundos que permanece mojado después del último contacto con agua.")]
    public float wetDurationAfterWater = 3f;

    // Temporizador interno para limpiar el estado mojado
    private float _wetTimer = 0f;

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

    [Header("Nuevos Materiales de Daño (por vida)")]
    [Tooltip("Material para daño leve: vida entre ~30% y ~40%.")]
    public Material damageMaterial_30to40;
    [Tooltip("Material para daño medio: vida entre ~20% y ~30%.")]
    public Material damageMaterial_20to30;
    [Tooltip("Material para daño severo: vida en ~10% o menos.")]
    public Material damageMaterial_0to10;

    [Header("Audio - Construcción (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir al completar la capa 0 (Base). -1 desactiva.")]
    [SerializeField] private int buildLayer0SfxIndex = -1;
    [Tooltip("Índice en AudioManager.soundEffects para reproducir al completar la capa 1 (Soporte). -1 desactiva.")]
    [SerializeField] private int buildLayer1SfxIndex = -1;
    [Tooltip("Índice en AudioManager.soundEffects para reproducir al completar la capa 2 (Superficie/Estructura). -1 desactiva.")]
    [SerializeField] private int buildLayer2SfxIndex = -1;

    [Header("Audio - Reparación (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir cuando se repara un cuadrante dañado. -1 desactiva.")]
    [SerializeField] private int repairSfxIndex = -1;

    [Header("Audio - Destrucción (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir cuando el cuadrante se destruye. -1 desactiva.")]
    [SerializeField] private int destroySfxIndex = -1;

    [Header("Audio - Daño (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir cuando el cuadrante queda dañado. -1 desactiva.")]
    [SerializeField] private int damageSfxIndex = -1;

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

        // Estado de humedad
        isWet = false;
        _wetTimer = 0f;
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
                // Ahora Industrial usa vida unificada
                currentLife = maxLife;
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
                return maxLife > 0f ? Mathf.Clamp01(currentLife / maxLife) : 0f;
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
                currentLife = maxLife * ratio01;
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
                currentLife = Mathf.Max(0f, currentLife - amount);
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
            // Si la vida llega a 0, destruir todo el cuadrante por completo
            DestroyQuadrant();
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
            // Reparación directa de la última capa SOLO debe hacerse vía TryAddLayer(MaterialType,...)
            // aquí rechazamos cualquier intento genérico para evitar que "cualquier material" la repare.
            Debug.LogError($"ERROR: Capa {layerIndex} ya está completada. Usa TryAddLayer(MaterialType, ...) para reparaciones válidas.");
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
            // Importante para Industrial/Futuristic: restaurar depósitos (temperatura/batería) y vida unificada
            ResetEraSpecificState();
        }

        requiredLayers[layerIndex].isCompleted = true;
        Debug.Log($"ÉXITO: Capa {layerIndex} marcada como completada.");

    // Reproducir SFX de construcción para esta capa (vía AudioManager, como en Campfire)
    PlayBuildSfxForLayer(layerIndex);

        CheckIfAllLayersCompleted();

        string estadoPosterior = "";
        for (int i = 0; i < requiredLayers.Length; i++)
        {
            estadoPosterior += $"Capa {i}: {(requiredLayers[i].isCompleted ? "Completada" : "Incompleta")}, ";
        }
        Debug.Log($"[TryAddLayer] Estado posterior: {estadoPosterior}");
        
        return true;
    }

    private void PlayBuildSfxForLayer(int layerIndex)
    {
        int sfxIndex = -1;
        switch (layerIndex)
        {
            case 0: sfxIndex = buildLayer0SfxIndex; break;
            case 1: sfxIndex = buildLayer1SfxIndex; break;
            case 2: sfxIndex = buildLayer2SfxIndex; break;
        }

        if (sfxIndex < 0) return; // desactivado

        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(sfxIndex);
        }
    }

    private void PlayRepairSfx()
    {
        if (repairSfxIndex < 0) return;
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(repairSfxIndex);
        }
    }

    private void PlayDestroySfx()
    {
        if (destroySfxIndex < 0) return; // desactivado
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(destroySfxIndex);
        }
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
                case EraType.Industrial: currentLife = maxLife; break;
                case EraType.Futuristic: batteryLife = 100f; break;
                default: currentLife = maxLife; break;
            }
        }
    }

    // Helper interno para saber si TODAS las capas (0..n-1) están construidas.
    // Necesario para gating de sistemas de vida (Industrial) y otros efectos tras refactors.
    private bool AllLayersBuilt()
    {
        if (requiredLayers == null || requiredLayers.Length == 0) return false;
        for (int i = 0; i < requiredLayers.Length; i++)
        {
            var layer = requiredLayers[i];
            if (layer == null || !layer.isCompleted) return false;
        }
        return true;
    }

    public void UpdateQuadrantState(float deltaTime)
    {
        // Actualizar temporizador de humedad: si está mojado, cuenta hacia atrás y se seca solo
        if (isWet)
        {
            if (wetDurationAfterWater <= 0f)
            {
                // Si la duración configurada es 0 o negativa, se seca inmediatamente
                isWet = false;
            }
            else
            {
                _wetTimer -= deltaTime;
                if (_wetTimer <= 0f)
                {
                    isWet = false;
                }
            }
        }

        if (!hasCollision) return;

        switch (era)
        {
            case EraType.Industrial:
                if (lastLayerState != LastLayerState.Destroyed)
                {
                    // Vida industrial sólo decae cuando TODAS las capas están construidas
                    if (!AllLayersBuilt())
                    {
                        break;
                    }

                    // 1) Daño por falta de calor (si no está recibiendo calor)
                    if (!isTurned)
                    {
                        currentLife -= temperatureDecayRate * deltaTime; // reutilizamos temperatureDecayRate como decay de vida
                        if (currentLife < 0f) currentLife = 0f;
                    }

                    // 2) Daño adicional por estar mojado + recibiendo calor
                    if (isWet && isTurned && wetHeatDamagePerSecond > 0f)
                    {
                        currentLife -= wetHeatDamagePerSecond * deltaTime;
                        if (currentLife < 0f) currentLife = 0f;
                    }

                    if (lastLayerState == LastLayerState.Complete && GetLifeRatio() < damagedThreshold01)
                        lastLayerState = LastLayerState.Damaged;

                    if (currentLife <= 0f)
                    {
                        lastLayerState = LastLayerState.Destroyed;
                        DestroyQuadrant();
                    }
                }

                if (debugLife && maxLife > 0f)
                {
                    _debugLifeTimer -= deltaTime;
                    if (_debugLifeTimer <= 0f)
                    {
                        float ratio = Mathf.Clamp01(currentLife / maxLife);
                        // Debug.Log($"[BridgeQuadrantSO] '{name}' Vida: {currentLife:F1}/{maxLife:F1} ({ratio:P0}) | isTurned={isTurned} | heatActive={heatActive} | state={lastLayerState}");
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
                            // Al agotarse la batería, destruir todo el cuadrante
                            DestroyQuadrant();
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
            // Si la vida llegó a 0 antes y estamos reconstruyendo, restaurar a máximo al aplicar calor (evita destrucción inmediata tras reconstrucción parcial)
            if (currentLife <= 0f)
            {
                currentLife = maxLife;
            }
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
        // Nuevo comportamiento: marcar cuadrante como mojado. El daño real se aplica en UpdateQuadrantState.
        isWet = true;
        _wetTimer = wetDurationAfterWater;
        RecalculateTurned();
    }

    /// <summary>
    /// Llamar cuando agua deja de tocar el cuadrante.
    /// </summary>
    public void RemoveWaterBlocker()
    {
        // Al dejar de tocar agua, simplemente dejamos que el temporizador agote el estado mojado.
        // (isWet se pondrá en false automáticamente cuando _wetTimer llegue a 0 en UpdateQuadrantState).
        RecalculateTurned();
    }

    private void RecalculateTurned()
    {
        // isTurned true solo si hay calor activo y no está bloqueado por agua.
        bool newTurned = heatActive; // Agua ya no bloquea el calor; solo agrega daño si isWet
        isTurned = newTurned;
    }

    public void ReplaceBattery()
    {
        if (era == EraType.Futuristic && lastLayerState != LastLayerState.Destroyed)
        {
            batteryLife = 100f;
            lastLayerState = LastLayerState.Complete;
            PlayRepairSfx();
        }
    }

    private void DestroyLastLayer()
    {
        if (requiredLayers.Length > 0)
        {
            requiredLayers[requiredLayers.Length - 1].isCompleted = false;
        }
        
        CheckIfAllLayersCompleted();
        
        // IMPORTANTE: Forzar actualización de visuales ANTES de refrescar las UIs
        // Esto asegura que currentLayer esté actualizado cuando se evalúe CanBuildLayer
        BridgeConstructionGrid.ForceUpdateAllQuadrantVisuals();
        
        // Refrescar las UIs de todos los jugadores cuando se destruye la última capa
        PlayerUIManager.RefreshAllActiveQuadrantUIs();
    }

    private void DestroyQuadrant()
    {
        bool wasAlreadyDestroyed = lastLayerState == LastLayerState.Destroyed;
        foreach (var layer in requiredLayers)
        {
            layer.isCompleted = false;
        }
        hasCollision = false;
        lastLayerState = LastLayerState.Destroyed;
        
        if (!wasAlreadyDestroyed)
        {
            PlayDestroySfx();
        }
        
        if (destructionEffectPrefab != null)
        {
            Debug.Log("Efecto de colapso disponible para reproducir");
        }
        
        // IMPORTANTE: Forzar actualización de visuales ANTES de refrescar las UIs
        // Esto asegura que currentLayer esté actualizado cuando se evalúe CanBuildLayer
        BridgeConstructionGrid.ForceUpdateAllQuadrantVisuals();
        
        // Refrescar las UIs de todos los jugadores cuando se destruye un cuadrante
        PlayerUIManager.RefreshAllActiveQuadrantUIs();
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
    /// Indica si el cuadrante tiene vida/batería por debajo del máximo y podría ser reparado.
    /// No cuenta como reparable si está destruido.
    /// </summary>
    public bool NeedsRepair()
    {
        if (lastLayerState == LastLayerState.Destroyed) return false;
        switch (era)
        {
            case EraType.Futuristic:
                return batteryLife < 100f;
            default:
                return currentLife < maxLife;
        }
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
        // REPARACIÓN: solo se permite reparar la ÚLTIMA capa y únicamente con el material tipo 3 (Adoquín).
        int lastIndex = requiredLayers != null && requiredLayers.Length > 0 ? requiredLayers.Length - 1 : -1;

        if (lastIndex >= 0 && requiredLayers[lastIndex].isCompleted && materialType == MaterialType.Adoquin && NeedsRepair())
        {
            Debug.Log($"Reparando última capa con material tipo 3 (Adoquín). Cantidad: {cantidad}");
            lastLayerState = LastLayerState.Complete;
            ResetEraSpecificState();
            PlayRepairSfx();
            return true;
        }

        // CONSTRUCCIÓN normal: solo cuando aún hay capas incompletas.
        for (int i = 0; i < requiredLayers.Length; i++)
        {
            if (!requiredLayers[i].isCompleted)
            {
                return TryAddLayer(i, null);
            }
        }

        Debug.Log("TryAddLayer(MaterialType): todas las capas están completas y no se cumplen condiciones de reparación.");
        return false;
    }
}

