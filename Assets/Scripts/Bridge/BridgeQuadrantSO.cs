using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBridgeQuadrant", menuName = "Bridge/Quadrant")]
public class BridgeQuadrantSO : ScriptableObject
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
    /// Enum para tipos de materiales compatible con MaterialTipo4
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
    public LayerInfo[] requiredLayers = new LayerInfo[4];
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

    [Header("Efectos Visuales")]
    public Material damagedMaterial;
    public Material destroyedMaterial;
    public GameObject destructionEffectPrefab;

    [Header("Sonidos")]
    public AudioClip constructionSound;
    public AudioClip damageSound;
    public AudioClip destructionSound;
    public AudioClip repairSound;

    public void Initialize()
    {
        hasCollision = false;
        lastLayerState = LastLayerState.Complete;
        
        foreach (var layer in requiredLayers)
        {
            layer.isCompleted = false;
        }

        ResetEraSpecificState();
    }

    private void ResetEraSpecificState()
    {
        switch (era)
        {
            case EraType.Prehistoric:
            case EraType.Medieval:
                currentUses = 0;
                break;
            case EraType.Industrial:
                currentTemperature = maxTemperature;
                break;
            case EraType.Contemporary:
                break;
            case EraType.Futuristic:
                batteryLife = 100f;
                break;
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
        hasCollision = requiredLayers[requiredLayers.Length - 1].isCompleted;
        
        if (hasCollision && lastLayerState != LastLayerState.Complete)
        {
            lastLayerState = LastLayerState.Complete;
        }
    }

    public void UpdateQuadrantState(float deltaTime)
    {
        if (!hasCollision) return;

        switch (era)
        {
            case EraType.Industrial:
                if (lastLayerState == LastLayerState.Complete)
                {
                    currentTemperature -= temperatureDecayRate * deltaTime;
                    if (currentTemperature < maxTemperature / 2)
                    {
                        lastLayerState = LastLayerState.Damaged;
                    }
                }
                break;
            
            case EraType.Futuristic:
                if (lastLayerState == LastLayerState.Complete)
                {
                    batteryLife -= batteryDrainRate * deltaTime;
                    if (batteryLife < 50)
                    {
                        lastLayerState = LastLayerState.Damaged;
                    }
                }
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

        switch (era)
        {
            case EraType.Prehistoric:
            case EraType.Medieval:
                currentUses++;
                if (currentUses >= maxUsesBeforeDamage / 2 && lastLayerState == LastLayerState.Complete)
                {
                    lastLayerState = LastLayerState.Damaged;
                }
                else if (currentUses >= maxUsesBeforeDamage && lastLayerState == LastLayerState.Damaged)
                {
                    lastLayerState = LastLayerState.Destroyed;
                    DestroyLastLayer();
                }
                break;
            
            case EraType.Contemporary:
                if (lastLayerState == LastLayerState.Complete)
                {
                    if (Random.value < damageChance)
                    {
                        lastLayerState = LastLayerState.Damaged;
                    }
                }
                else if (lastLayerState == LastLayerState.Damaged)
                {
                    if (Random.value < damageChance)
                    {
                        lastLayerState = LastLayerState.Destroyed;
                        DestroyLastLayer();
                    }
                }
                break;
        }
    }

    public void ApplyHeat()
    {
        if (era == EraType.Industrial && lastLayerState != LastLayerState.Destroyed)
        {
        }
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