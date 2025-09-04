using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeConstructionGrid : MonoBehaviour
{
    [Header("Configuración de la Grilla")]
    public int gridWidth = 5;
    public int gridLength = 10;
    public float quadrantSize = 1f;

    [Header("Referencias")]
    public BridgeQuadrantSO defaultQuadrantSO;
    public GameObject quadrantPrefab;
    public Transform quadrantParent;    [Header("Configuración de Capas")]
    [Tooltip("Alturas Y específicas para cada capa del puente (Base, Soporte, Estructura, Superficie)")]
    public float[] layerHeights = new float[] { 0.0f, 0.5f, 1.0f, 1.5f };
    
    [Tooltip("Escalas individuales para cada capa del puente (Base, Soporte, Estructura, Superficie)")]
    public Vector3[] layerScales = new Vector3[] { 
        Vector3.one,        // Capa 0: Base - escala normal
        Vector3.one,        // Capa 1: Soporte - escala normal  
        Vector3.one,        // Capa 2: Estructura - escala normal
        Vector3.one         // Capa 3: Superficie - escala normal
    };

    [Header("Visualización de Depuración")]
    public bool showDebugGrid = true;
    public Color completeColor = Color.green;
    public Color incompleteColor = Color.red;
    public Color damagedColor = Color.yellow;

    [Header("Sistema de Power Ups")]
    public float powerUpEffectMultiplier = 1.5f;
    public bool isPowerUpActive = false;
    private PowerUpBase activePowerUp;

    // Estructura para almacenar información del cuadrante en la grilla
    private class QuadrantInfo
    {
        public GameObject quadrantObject;
        public BridgeQuadrantSO quadrantSO;
        public Renderer[] layerRenderers = new Renderer[4];
        public Collider quadrantCollider;
        public Vector3 worldPosition;
    }

    // Matriz que representa la grilla de construcción
    private QuadrantInfo[,] constructionGrid;    private void Awake()
    {
        InitializeGrid();
        
        // Aplicar escalas configuradas a las capas existentes después de la inicialización
        ApplyConfiguredScalesAfterInit();

        // Suscribirse al evento de activación de PowerUps
        PowerUpBase.OnPowerUpActivated += HandlePowerUpActivated;
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar memory leaks
        PowerUpBase.OnPowerUpActivated -= HandlePowerUpActivated;
    }

    private void Start()
    {
        // Verificar que existe el tag necesario para las colisiones
        bool tagExists = false;

        // Esta es una forma de verificar si un tag existe, pero no garantiza que funcione en todas las versiones de Unity
        try
        {
            GameObject testObj = new GameObject();
            testObj.tag = "BridgeQuadrant";
            Destroy(testObj);
            tagExists = true;
        }
        catch (UnityException)
        {
            tagExists = false;
        }

        if (!tagExists)
        {
            Debug.LogError("¡ATENCIÓN! El tag 'BridgeQuadrant' no existe en el proyecto. " +
                          "Por favor, añádelo en Edit > Project Settings > Tags and Layers. " +
                          "Sin este tag, las colisiones de vehículos con el puente NO FUNCIONARÁN.");
        }
    }

    private void Update()
    {
        // Actualizar el estado de cada cuadrante según el transcurso del tiempo
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                if (constructionGrid[x, z] != null && constructionGrid[x, z].quadrantSO != null)
                {
                    // Si hay un power-up activo, esto se considera en UpdateQuadrantState
                    constructionGrid[x, z].quadrantSO.UpdateQuadrantState(Time.deltaTime * (isPowerUpActive ? powerUpEffectMultiplier : 1f));
                    UpdateQuadrantVisuals(x, z);
                }
            }
        }
    }

    // Manejador de PowerUps activados
    private void HandlePowerUpActivated(PowerUpBase powerUp)
    {
        activePowerUp = powerUp;
        isPowerUpActive = true;

        // Ejecutar comportamiento específico según el tipo de PowerUp
        if (powerUp is PowerUpRitualGranFuego)
        {
            StartCoroutine(HandleRitualGranFuego());
        }
        else if (powerUp is PowerUpConstructorHolografico)
        {
            StartCoroutine(HandleConstructorHolografico());
        }
        else if (powerUp is PowerUpCalorHumano)
        {
            StartCoroutine(HandleCalorHumano());
        }

        // Configurar un temporizador para cuando termine el efecto
        StartCoroutine(PowerUpEffectTimer(powerUp.duration));
    }

    private IEnumerator PowerUpEffectTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        isPowerUpActive = false;
        activePowerUp = null;
    }

    // Comportamientos específicos para cada PowerUp
    private IEnumerator HandleRitualGranFuego()
    {
        Debug.Log("PowerUp Ritual de Gran Fuego activado - Construyendo capas automáticamente");
        // Construir automáticamente todos los cuadrantes hasta la capa 3
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                var so = GetQuadrantSO(x, z);
                if (so != null)
                {
                    // Construir capas 0, 1 y 2 en orden
                    for (int layer = 0; layer <= 2; layer++)
                    {
                        // Mientras la capa no esté completa, intenta construirla
                        if (!so.requiredLayers[layer].isCompleted)
                        {
                            bool result = so.TryAddLayer(layer, null);
                            UpdateQuadrantVisuals(x, z);
                            Debug.Log($"Construida capa {layer} en cuadrante [{x},{z}]: {result}");
                            yield return new WaitForSeconds(0.05f); // Pequeña espera para efecto visual
                        }
                    }
                }
            }
        }
    }

    private IEnumerator HandleConstructorHolografico()
    {
        Debug.Log("PowerUp Constructor Holográfico activado - Acelerando construcción");
        // Este power-up acelera la construcción (ya implementado con el multiplicador)
        yield return null;
    }

    private IEnumerator HandleCalorHumano()
    {
        Debug.Log("PowerUp Calor Humano activado - Aplicando calor a todos los cuadrantes");
        // Aplicar calor a todos los cuadrantes del puente
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                ApplyHeat(x, z);
            }        }
        yield return null;
    }

    // Validación de propiedades en el editor
    private void OnValidate()
    {
        // Validar que el array de alturas tenga el tamaño correcto (4 capas)
        if (layerHeights == null || layerHeights.Length != 4)
        {
            layerHeights = new float[] { 0.0f, 0.5f, 1.0f, 1.5f };
            Debug.LogWarning("Array de alturas de capas resetado a valores por defecto. Ajústalo según tus necesidades.", this);
        }

        // Validar que el array de escalas tenga el tamaño correcto (4 capas)
        if (layerScales == null || layerScales.Length != 4)
        {
            layerScales = new Vector3[] { Vector3.one, Vector3.one, Vector3.one, Vector3.one };
            Debug.LogWarning("Array de escalas de capas resetado a valores por defecto. Ajústalo según tus necesidades.", this);
        }

        if (quadrantParent == null)
        {
            // Intentar crear automáticamente el padre para los cuadrantes
            Transform existingParent = transform.Find("QuadrantContainer");

            if (existingParent != null)
            {
                quadrantParent = existingParent;
            }
            else
            {
                GameObject container = new GameObject("QuadrantContainer");
                container.transform.SetParent(transform);
                container.transform.localPosition = Vector3.zero;
                quadrantParent = container.transform;

                Debug.Log("Se ha creado automáticamente el contenedor de cuadrantes. Asígnalo en el inspector para que persista.");
            }
        }        // Si estamos en tiempo de edición y hay cambios en quadrantSize, actualizar la grilla
        if (!Application.isPlaying && constructionGrid != null)
        {
            // Llamar al reescalado solo si la grilla ya está inicializada
            RescaleGrid();
        }
        // Si estamos en tiempo de ejecución, aplicar escalas automáticamente
        else if (Application.isPlaying && constructionGrid != null)
        {
            ApplyConfiguredScalesAfterInit();
        }
    }

    private void InitializeGrid()
    {
        // Asegurar que tengamos un padre para los cuadrantes
        if (quadrantParent == null)
        {
            GameObject container = new GameObject("QuadrantContainer");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            quadrantParent = container.transform;

            Debug.LogWarning("No se encontró el padre de cuadrantes. Se ha creado uno automáticamente.", this);
        }

        constructionGrid = new QuadrantInfo[gridWidth, gridLength];

        // Crear los objetos de cuadrante en la escena
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                Vector3 position = transform.position + new Vector3(x * quadrantSize, 0, z * quadrantSize);

                constructionGrid[x, z] = new QuadrantInfo();
                constructionGrid[x, z].worldPosition = position;

                // Crear una instancia del SO por cuadrante
                if (defaultQuadrantSO != null)
                {
                    BridgeQuadrantSO newQuadrantSO = Instantiate(defaultQuadrantSO);
                    newQuadrantSO.Initialize();
                    constructionGrid[x, z].quadrantSO = newQuadrantSO;

                    // Crear el objeto físico del cuadrante
                    if (quadrantPrefab != null)
                    {
                        GameObject quadrantObj = Instantiate(quadrantPrefab, position, Quaternion.identity, quadrantParent);
                        quadrantObj.name = $"Quadrant_{x}_{z}";

                        // IMPORTANTE: Asignar el tag "BridgeQuadrant" para que VehicleBridgeCollision lo detecte
                        quadrantObj.tag = "BridgeQuadrant";

                        constructionGrid[x, z].quadrantObject = quadrantObj;

                        // Obtener el collider y guardarlo
                        constructionGrid[x, z].quadrantCollider = quadrantObj.GetComponent<Collider>();
                        if (constructionGrid[x, z].quadrantCollider != null)
                        {
                            constructionGrid[x, z].quadrantCollider.enabled = false; // Inicialmente desactivado

                            // Asegurarse de que el collider NO es un trigger
                            constructionGrid[x, z].quadrantCollider.isTrigger = false;

                            // Si es un BoxCollider, ajustar su tamaño para que coincida con el cuadrante
                            BoxCollider boxCol = constructionGrid[x, z].quadrantCollider as BoxCollider;
                            if (boxCol != null)
                            {
                                boxCol.size = new Vector3(quadrantSize, 0.5f, quadrantSize);
                                boxCol.center = new Vector3(quadrantSize / 2, 0.25f, quadrantSize / 2);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"El prefab del cuadrante no tiene un Collider. Añadiendo BoxCollider automáticamente a {quadrantObj.name}");
                            BoxCollider boxCol = quadrantObj.AddComponent<BoxCollider>();
                            constructionGrid[x, z].quadrantCollider = boxCol;
                            constructionGrid[x, z].quadrantCollider.enabled = false;
                            boxCol.isTrigger = false;
                            boxCol.size = new Vector3(quadrantSize, 0.5f, quadrantSize);
                            boxCol.center = new Vector3(quadrantSize / 2, 0.25f, quadrantSize / 2);
                        }

                        // Preparar los contenedores para los renderizadores de las capas
                        constructionGrid[x, z].layerRenderers = new Renderer[4];
                    }
                    else
                    {
                        Debug.LogError("¡Falta el prefab del cuadrante! Por favor asignarlo en el inspector.", this);
                    }
                }
                else
                {
                    Debug.LogError("¡Falta el ScriptableObject del cuadrante! Por favor asignarlo en el inspector.", this);
                }
            }
        }

        ApplyConfiguredScalesAfterInit();
    }

    /// <summary>
    /// Aplica las escalas configuradas a todas las capas existentes después de la inicialización
    /// </summary>
    private void ApplyConfiguredScalesAfterInit()
    {
        if (constructionGrid == null)
            return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                if (constructionGrid[x, z] != null && constructionGrid[x, z].quadrantSO != null)
                {
                    // Verificar si hay capas completadas que necesiten escalas aplicadas
                    for (int i = 0; i < constructionGrid[x, z].quadrantSO.requiredLayers.Length; i++)
                    {
                        if (constructionGrid[x, z].quadrantSO.requiredLayers[i].isCompleted)
                        {
                            // Buscar el objeto de la capa en la jerarquía
                            string nombreCapa = $"Layer_{i}_{constructionGrid[x, z].quadrantSO.requiredLayers[i].layerName}";
                            Transform layerTransform = constructionGrid[x, z].quadrantObject.transform.Find(nombreCapa);
                            
                            if (layerTransform != null)
                            {
                                // Aplicar la escala configurada
                                Vector3 baseScale = new Vector3(quadrantSize, 1f, quadrantSize);
                                Vector3 layerScale = (i < layerScales.Length) ? layerScales[i] : Vector3.one;
                                Vector3 finalScale = Vector3.Scale(baseScale, layerScale);
                                layerTransform.localScale = finalScale;

                                // Actualizar la posición también por si cambió la altura
                                float layerHeight = (i < layerHeights.Length) ? layerHeights[i] : (0.5f * i);
                                Vector3 posicionCorrecta = constructionGrid[x, z].worldPosition + new Vector3(
                                    quadrantSize / 2,
                                    layerHeight,
                                    quadrantSize / 2
                                );
                                layerTransform.position = posicionCorrecta;

                                Debug.Log($"[Init] Escalas aplicadas a capa {i} en cuadrante [{x},{z}]: {finalScale}");
                            }
                        }
                    }
                }
            }
        }
    }

    // Método público para intentar construir una capa en un cuadrante específico
    public bool TryBuildLayer(int x, int z, int layerIndex, GameObject layerObject)
    {
        // VERIFICACIONES PREVIAS CRÍTICAS

        // 1. Verificar coordenadas válidas
        if (!IsValidQuadrant(x, z))
        {
            Debug.LogError($"COORDENADAS INVÁLIDAS: Cuadrante [{x},{z}] no es válido. Límites: [0-{gridWidth - 1}, 0-{gridLength - 1}]");
            return false;
        }

        // 2. Verificar ScriptableObject existente
        if (constructionGrid[x, z].quadrantSO == null)
        {
            Debug.LogError($"ERROR: El ScriptableObject para el cuadrante [{x},{z}] es nulo.");
            return false;
        }

        // 3. Índice de capa válido
        if (layerIndex < 0 || layerIndex >= constructionGrid[x, z].quadrantSO.requiredLayers.Length)
        {
            Debug.LogError($"ERROR: Índice de capa {layerIndex} fuera de rango [0-{constructionGrid[x, z].quadrantSO.requiredLayers.Length - 1}]");
            return false;
        }

        // 4. GameObject válido (ignoramos esto si viene de un PowerUp)
        if (layerObject == null && !isPowerUpActive)
        {
            Debug.LogError("ERROR: Se está intentando construir con un GameObject nulo");
            return false;
        }

        // DEPURACIÓN - Estado actual del cuadrante
        string estadoCapas = "";
        for (int i = 0; i < constructionGrid[x, z].quadrantSO.requiredLayers.Length; i++)
        {
            bool completada = constructionGrid[x, z].quadrantSO.requiredLayers[i].isCompleted;
            estadoCapas += $"Capa {i}: {(completada ? "Completada" : "Incompleta")}, ";
        }
        Debug.Log($"[GRID] Estado del cuadrante [{x},{z}] ANTES: {estadoCapas}");

        // VERIFICACIÓN CRÍTICA DE SECUENCIA DE CONSTRUCCIÓN
        int primerCapaIncompleta = -1;
        for (int i = 0; i < constructionGrid[x, z].quadrantSO.requiredLayers.Length; i++)
        {
            if (!constructionGrid[x, z].quadrantSO.requiredLayers[i].isCompleted)
            {
                primerCapaIncompleta = i;
                break;
            }
        }

        // Si todas las capas están completas, no se puede construir más
        if (primerCapaIncompleta == -1)
        {
            Debug.LogError($"ERROR: El cuadrante [{x},{z}] ya tiene todas sus capas completas. No se puede construir más.");
            return false;
        }

        // Si intenta construir una capa que no es la primera disponible, rechazar
        // Ignoramos esta verificación si hay un PowerUp activo de tipo constructor
        if (layerIndex != primerCapaIncompleta &&
            !(isPowerUpActive && (activePowerUp is PowerUpRitualGranFuego || activePowerUp is PowerUpConstructorHolografico)))
        {
            Debug.LogError($"ERROR DE SECUENCIA EN GRID: Debes construir primero la capa {primerCapaIncompleta}, no la capa {layerIndex}");
            return false;
        }

        // LLAMADA AL SCRIPTABLE OBJECT PARA INTENTAR CONSTRUIR
        bool success = constructionGrid[x, z].quadrantSO.TryAddLayer(layerIndex, layerObject);

        // PROCESAMIENTO DEL RESULTADO
        if (success)
        {
            // Actualizar visuales y estado físico
            UpdateQuadrantVisuals(x, z);
            PlayConstructionSound(x, z);

            // DEPURACIÓN - Estado actual del cuadrante
            string estadoCapasDespues = "";
            for (int i = 0; i < constructionGrid[x, z].quadrantSO.requiredLayers.Length; i++)
            {
                bool completada = constructionGrid[x, z].quadrantSO.requiredLayers[i].isCompleted;
                estadoCapasDespues += $"Capa {i}: {(completada ? "Completada" : "Incompleta")}, ";
            }
            Debug.Log($"[GRID] Estado del cuadrante [{x},{z}] DESPUÉS: {estadoCapasDespues}");

            Debug.Log($"ÉXITO: Capa {layerIndex} construida en cuadrante [{x},{z}]");
        }
        else
        {
            // Diagnóstico detallado del fallo
            bool layerCompleted = constructionGrid[x, z].quadrantSO.requiredLayers[layerIndex].isCompleted;
            Debug.LogError($"FALLO EN CONSTRUCCIÓN: No se pudo construir la capa {layerIndex} en cuadrante [{x},{z}]. " +
                           $"Estado de esta capa: {(layerCompleted ? "Ya estaba completada" : "Incompleta")}, " +
                           $"LastLayerState: {constructionGrid[x, z].quadrantSO.lastLayerState}");
        }

        return success;
    }

    // Método para simular el impacto de un vehículo en un cuadrante
    public void OnVehicleImpact(int x, int z)
    {
        if (!IsValidQuadrant(x, z) || constructionGrid[x, z].quadrantSO == null)
            return;

        // Guardar el estado anterior para comparar
        bool estabaCompleto = constructionGrid[x, z].quadrantSO.requiredLayers[constructionGrid[x, z].quadrantSO.requiredLayers.Length - 1].isCompleted;

        // Guardar referencias a los renderizadores antes de la destrucción
        Renderer[] renderizadoresAntiguos = new Renderer[constructionGrid[x, z].layerRenderers.Length];
        System.Array.Copy(constructionGrid[x, z].layerRenderers, renderizadoresAntiguos, constructionGrid[x, z].layerRenderers.Length);

        // Procesar el impacto
        constructionGrid[x, z].quadrantSO.OnVehicleImpact();

        // Si el cuadrante no está completo, destruir los objetos visuales y limpiar las referencias
        bool estaIncompletoDespues = !constructionGrid[x, z].quadrantSO.requiredLayers[constructionGrid[x, z].quadrantSO.requiredLayers.Length - 1].isCompleted;

        if (!estabaCompleto || estaIncompletoDespues)
        {
            // Destruir todos los objetos visuales existentes
            for (int i = 0; i < renderizadoresAntiguos.Length; i++)
            {
                if (renderizadoresAntiguos[i] != null && renderizadoresAntiguos[i].gameObject != null)
                {
                    GameObject layerObj = renderizadoresAntiguos[i].gameObject;
                    Destroy(layerObj);

                    // Limpiar la referencia en el array
                    constructionGrid[x, z].layerRenderers[i] = null;

                    Debug.Log($"Destruido objeto visual de capa {i} en cuadrante [{x},{z}]");
                }
            }

            // Si el cuadrante estaba incompleto y se destruyó, reproducir efecto de colapso
            if (!estabaCompleto && constructionGrid[x, z].quadrantSO.destructionEffectPrefab != null)
            {
                Vector3 posicionEfecto = constructionGrid[x, z].worldPosition + new Vector3(quadrantSize / 2, 0, quadrantSize / 2);
                Instantiate(constructionGrid[x, z].quadrantSO.destructionEffectPrefab, posicionEfecto, Quaternion.identity);
                PlayDestructionSound(x, z);
            }
        }

        // Actualizar visuales
        UpdateQuadrantVisuals(x, z);

        // Si el cuadrante estaba completo, reproducir sonido según el estado
        if (estabaCompleto && !estaIncompletoDespues)
        {
            switch (constructionGrid[x, z].quadrantSO.lastLayerState)
            {
                case BridgeQuadrantSO.LastLayerState.Damaged:
                    PlayDamageSound(x, z);
                    break;
                case BridgeQuadrantSO.LastLayerState.Destroyed:
                    PlayDestructionSound(x, z);
                    break;
            }
        }
    }

    // Métodos para interacciones específicas de era
    public void ApplyHeat(int x, int z)
    {
        if (!IsValidQuadrant(x, z) || constructionGrid[x, z].quadrantSO == null)
            return;

        constructionGrid[x, z].quadrantSO.ApplyHeat();
    }

    public void ReplaceBattery(int x, int z)
    {
        if (!IsValidQuadrant(x, z) || constructionGrid[x, z].quadrantSO == null)
            return;

        constructionGrid[x, z].quadrantSO.ReplaceBattery();
        UpdateQuadrantVisuals(x, z);
        PlayRepairSound(x, z);
    }

    // Método para aplicar power-up a todos los cuadrantes (útil para algunos power-ups)
    public void ApplyEffectToAllQuadrants(System.Action<BridgeQuadrantSO> effect)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                BridgeQuadrantSO so = GetQuadrantSO(x, z);
                if (so != null)
                {
                    effect(so);
                    UpdateQuadrantVisuals(x, z);
                }
            }
        }
    }

    // Actualizar los aspectos visuales de un cuadrante
    private void UpdateQuadrantVisuals(int x, int z)
    {
        if (!IsValidQuadrant(x, z) || constructionGrid[x, z].quadrantSO == null)
            return;

        QuadrantInfo info = constructionGrid[x, z];

        // Verificar si el cuadrante está completo (tiene la última capa)
        bool cuadranteCompleto = info.quadrantSO.requiredLayers[info.quadrantSO.requiredLayers.Length - 1].isCompleted;

        // Actualizar la colisión del cuadrante principal
        if (info.quadrantCollider != null)
        {
            // El collider principal solo se activa si el cuadrante está completo
            info.quadrantCollider.enabled = cuadranteCompleto;
            info.quadrantCollider.isTrigger = false; // No es trigger para soportar el auto
        }

        // Actualizar las visuales de cada capa
        for (int i = 0; i < info.quadrantSO.requiredLayers.Length; i++)
        {
            if (info.quadrantSO.requiredLayers[i].isCompleted)
            {
                // Si aún no hay un renderizador para esta capa, crearlo
                if (info.layerRenderers[i] == null && info.quadrantSO.requiredLayers[i].visualPrefab != null)                {                    // Calcular la posición correcta para la visualización usando las alturas configurables
                    float layerHeight = (i < layerHeights.Length) ? layerHeights[i] : (0.5f * i);
                    Vector3 posicionCorrecta = info.worldPosition + new Vector3(
                        quadrantSize / 2,  // Centrado en X
                        layerHeight,       // Altura específica para esta capa
                        quadrantSize / 2   // Centrado en Z
                    );

                    // Verificar si hay algún objeto antiguo de la misma capa y destruirlo
                    string nombreCapa = $"Layer_{i}_{info.quadrantSO.requiredLayers[i].layerName}";
                    Transform existingLayer = info.quadrantObject.transform.Find(nombreCapa);
                    if (existingLayer != null)
                    {
                        Debug.Log($"Eliminando objeto de capa antiguo: {nombreCapa}");
                        Destroy(existingLayer.gameObject);
                        info.layerRenderers[i] = null; // Limpiar la referencia
                    }
                    GameObject layerObj = Instantiate(info.quadrantSO.requiredLayers[i].visualPrefab,
                      posicionCorrecta, Quaternion.identity, info.quadrantObject.transform);                    layerObj.name = nombreCapa;

                    // Calcular escala combinando el quadrantSize con la escala específica de la capa
                    Vector3 baseScale = new Vector3(quadrantSize, 1f, quadrantSize);
                    Vector3 layerScale = (i < layerScales.Length) ? layerScales[i] : Vector3.one;
                    Vector3 finalScale = Vector3.Scale(baseScale, layerScale);
                    layerObj.transform.localScale = finalScale;

                    Debug.Log($"Escalando capa {i} con escala final {finalScale} (base: {baseScale}, específica: {layerScale}) para quadrantSize {quadrantSize}");// Si es la capa 4 (índice 3), asignar la layer "BridgeLayer4"
                    if (i == 3)
                    {
                        int bridgeLayer = LayerMask.NameToLayer("BridgeLayer4");
                        if (bridgeLayer != -1)
                        {
                            layerObj.layer = bridgeLayer;
                        }
                        else
                        {
                            Debug.LogWarning("La Layer 'BridgeLayer4' no existe en el proyecto. Asegúrate de crearla en Edit > Project Settings > Tags and Layers.");
                        }
                    }

                    // Configurar colliders según si es la última capa o no
                    bool esUltimaCapa = (i == info.quadrantSO.requiredLayers.Length - 1);

                    // Asegurarnos de que la capa tenga un collider
                    Collider layerCollider = layerObj.GetComponent<Collider>();
                    if (layerCollider == null)
                    {
                        // Añadir collider si no existe
                        BoxCollider boxCol = layerObj.AddComponent<BoxCollider>();
                        // Como el objeto ya está escalado, usar tamaño unitario (1x1x1)
                        boxCol.size = new Vector3(1f, 0.2f, 1f);
                        boxCol.center = Vector3.zero;
                        layerCollider = boxCol;
                    }
                    // NOTA: Si ya existe un BoxCollider, no lo redimensionamos porque 
                    // el escalado del objeto ya se encarga de ajustar su tamaño visual

                    // Si es la última capa y el cuadrante está completo, el collider no es trigger
                    // Si NO es la última capa o el cuadrante está incompleto, el collider es trigger
                    if (esUltimaCapa && cuadranteCompleto)
                    {
                        layerCollider.isTrigger = false; // Collider sólido para la última capa completa
                        layerCollider.enabled = true;
                    }
                    else
                    {
                        // IMPORTANTE: Usar triggers para detectar colisiones en capas incompletas
                        layerCollider.isTrigger = true;
                        layerCollider.enabled = true; // Siempre activado para detectar autos
                    }

                    info.layerRenderers[i] = layerObj.GetComponent<Renderer>();

                    if (info.layerRenderers[i] == null)
                    {
                        // Intenta buscar un Renderer en los hijos
                        info.layerRenderers[i] = layerObj.GetComponentInChildren<Renderer>();

                        if (info.layerRenderers[i] == null)
                        {
                            Debug.LogWarning($"No se encontró ningún Renderer en el prefab de la capa {i}. Asegúrate de que el prefab tenga un componente Renderer.", this);
                        }
                    }                    // Log para depuración
                    Debug.Log($"Creada visualización para capa {i} en posición {posicionCorrecta}");
                }
            }
            else if (info.layerRenderers[i] != null && info.layerRenderers[i].gameObject != null)
            {
                // Actualizar el estado del collider para una capa existente
                Collider[] layerColliders = info.layerRenderers[i].gameObject.GetComponentsInChildren<Collider>();
                bool esUltimaCapa = (i == info.quadrantSO.requiredLayers.Length - 1);

                foreach (Collider col in layerColliders)
                {
                    if (esUltimaCapa && cuadranteCompleto)
                    {
                        col.isTrigger = false;
                        col.enabled = true;
                    }
                    else
                    {
                        col.isTrigger = true;
                        col.enabled = true;
                    }
                }
            }
            else if (info.layerRenderers[i] != null)
            {
                // Si la capa no está completa pero hay un renderizador, desactivarlo
                info.layerRenderers[i].gameObject.SetActive(false);
            }

            // Si es la última capa, aplicar el material según el estado
            if (i == info.quadrantSO.requiredLayers.Length - 1 && info.layerRenderers[i] != null)
            {
                switch (info.quadrantSO.lastLayerState)
                {
                    case BridgeQuadrantSO.LastLayerState.Complete:
                        info.layerRenderers[i].material = info.quadrantSO.requiredLayers[i].material;
                        break;
                    case BridgeQuadrantSO.LastLayerState.Damaged:
                        info.layerRenderers[i].material = info.quadrantSO.damagedMaterial;
                        break;
                    case BridgeQuadrantSO.LastLayerState.Destroyed:
                        // Si está destruida, desactivamos el renderizador
                        info.layerRenderers[i].gameObject.SetActive(false);
                        break;
                }
            }
        }
    }

    // Método de depuración para dibujar la grilla
    void OnDrawGizmos()
    {
        if (!showDebugGrid) return;

        // Si estamos en modo de edición, dibujamos la grilla en base a las propiedades configuradas
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridLength; z++)
                {
                    Vector3 position = transform.position + new Vector3(x * quadrantSize, 0, z * quadrantSize);
                    Vector3 center = position + new Vector3(quadrantSize / 2, 0, quadrantSize / 2);

                    // Dibujar el wireframe del cuadrante
                    Gizmos.DrawWireCube(center, new Vector3(quadrantSize, 0.1f, quadrantSize));

                    // Dibujar un punto en la posición de origen (esquina) del cuadrante
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(position, 0.05f);

                    // Dibujar un punto en el centro del cuadrante
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(center, 0.05f);

                    // Restaurar color
                    Gizmos.color = Color.cyan;
                }
            }
            return;
        }

        // En modo de juego, mostrar el estado de los cuadrantes
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                if (constructionGrid != null && constructionGrid[x, z] != null && constructionGrid[x, z].quadrantSO != null)
                {
                    // Determinar color según estado
                    Color debugColor;
                    if (!constructionGrid[x, z].quadrantSO.hasCollision)
                    {
                        debugColor = incompleteColor;
                    }
                    else if (constructionGrid[x, z].quadrantSO.lastLayerState == BridgeQuadrantSO.LastLayerState.Damaged)
                    {
                        debugColor = damagedColor;
                    }
                    else
                    {
                        debugColor = completeColor;
                    }

                    Gizmos.color = debugColor;

                    // Obtener la posición del cuadrante
                    Vector3 position = constructionGrid[x, z].worldPosition;
                    Vector3 center = position + new Vector3(quadrantSize / 2, 0, quadrantSize / 2);

                    // Dibujar el wireframe del cuadrante con el color según su estado
                    Gizmos.DrawWireCube(center, new Vector3(quadrantSize, 0.1f, quadrantSize));

                    // Dibujar puntos para visualizar mejor las posiciones
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(position, 0.05f); // Posición de origen

                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(center, 0.05f); // Posición central
                }
            }
        }
    }

    // Métodos de utilidad
    public bool IsValidQuadrant(int x, int z)
    {
        return x >= 0 && x < gridWidth && z >= 0 && z < gridLength;
    }

    // Métodos para reproducir sonidos
    private void PlayConstructionSound(int x, int z)
    {
        if (!IsValidQuadrant(x, z) || constructionGrid[x, z].quadrantSO == null ||
            constructionGrid[x, z].quadrantSO.constructionSound == null)
            return;

        AudioSource.PlayClipAtPoint(constructionGrid[x, z].quadrantSO.constructionSound,
            constructionGrid[x, z].worldPosition);
    }

    private void PlayDamageSound(int x, int z)
    {
        if (!IsValidQuadrant(x, z) || constructionGrid[x, z].quadrantSO == null ||
            constructionGrid[x, z].quadrantSO.damageSound == null)
            return;

        AudioSource.PlayClipAtPoint(constructionGrid[x, z].quadrantSO.damageSound,
            constructionGrid[x, z].worldPosition);
    }

    private void PlayDestructionSound(int x, int z)
    {
        if (!IsValidQuadrant(x, z) || constructionGrid[x, z].quadrantSO == null ||
            constructionGrid[x, z].quadrantSO.destructionSound == null)
            return;

        AudioSource.PlayClipAtPoint(constructionGrid[x, z].quadrantSO.destructionSound,
            constructionGrid[x, z].worldPosition);
    }

    private void PlayRepairSound(int x, int z)
    {
        if (!IsValidQuadrant(x, z) || constructionGrid[x, z].quadrantSO == null ||
            constructionGrid[x, z].quadrantSO.repairSound == null)
            return;

        AudioSource.PlayClipAtPoint(constructionGrid[x, z].quadrantSO.repairSound,
            constructionGrid[x, z].worldPosition);
    }

    // Método público para obtener el ScriptableObject de un cuadrante
    public BridgeQuadrantSO GetQuadrantSO(int x, int z)
    {
        // Verifica que las coordenadas sean válidas
        if (x < 0 || x >= gridWidth || z < 0 || z >= gridLength)
            return null;

        // Devuelve el ScriptableObject del cuadrante
        if (constructionGrid != null && constructionGrid[x, z] != null)
            return constructionGrid[x, z].quadrantSO;

        return null;
    }    [Header("Debug Tools")]
    [SerializeField] private bool showDebugTools = false;    /// <summary>
    /// Configura alturas predefinidas para un puente bajo y compacto
    /// </summary>
    [ContextMenu("Preset: Puente Bajo")]
    public void SetLowBridgeHeights()
    {
        layerHeights = new float[] { 0.0f, 0.3f, 0.6f, 0.9f };
        layerScales = new Vector3[] { 
            new Vector3(1.0f, 0.8f, 1.0f),  // Base: más aplastada
            new Vector3(0.9f, 1.0f, 0.9f),  // Soporte: ligeramente más pequeño
            new Vector3(1.0f, 0.7f, 1.0f),  // Estructura: más baja
            new Vector3(1.1f, 0.5f, 1.1f)   // Superficie: más ancha pero baja
        };
        Debug.Log("Configurado puente bajo: alturas compactas y escalas optimizadas");
    }

    /// <summary>
    /// Configura alturas predefinidas para un puente estándar
    /// </summary>
    [ContextMenu("Preset: Puente Estándar")]
    public void SetStandardBridgeHeights()
    {
        layerHeights = new float[] { 0.0f, 0.5f, 1.0f, 1.5f };
        layerScales = new Vector3[] { Vector3.one, Vector3.one, Vector3.one, Vector3.one };
        Debug.Log("Configurado puente estándar: alturas y escalas balanceadas");
    }

    /// <summary>
    /// Configura alturas predefinidas para un puente alto e imponente
    /// </summary>
    [ContextMenu("Preset: Puente Alto")]
    public void SetHighBridgeHeights()
    {
        layerHeights = new float[] { 0.0f, 0.8f, 1.6f, 2.4f };
        layerScales = new Vector3[] { 
            new Vector3(1.2f, 1.0f, 1.2f),  // Base: más ancha para estabilidad
            new Vector3(1.0f, 1.5f, 1.0f),  // Soporte: más alto
            new Vector3(1.1f, 1.2f, 1.1f),  // Estructura: robusta
            new Vector3(1.0f, 0.8f, 1.0f)   // Superficie: normal pero más delgada
        };        Debug.Log("Configurado puente alto: alturas elevadas y escalas robustas");
    }

    /// <summary>
    /// Preset para un puente prehistórico robusto con capas gruesas
    /// </summary>
    [ContextMenu("Preset: Puente Prehistórico Robusto")]
    public void SetPrehistoricBridgeScales()
    {
        layerHeights = new float[] { 0.0f, 0.6f, 1.2f, 1.8f };
        layerScales = new Vector3[] { 
            new Vector3(1.3f, 1.5f, 1.3f),  // Base: muy gruesa y ancha
            new Vector3(1.1f, 2.0f, 1.1f),  // Soporte: pilares altos y robustos
            new Vector3(1.2f, 1.0f, 1.2f),  // Estructura: ancha para estabilidad
            new Vector3(1.0f, 0.6f, 1.0f)   // Superficie: plana pero resistente
        };
        Debug.Log("Configurado puente prehistórico: estructuras robustas y gruesas");
    }

    /// <summary>
    /// Preset para escalas uniformes (resetear a normal)
    /// </summary>
    [ContextMenu("Preset: Escalas Uniformes")]
    public void SetUniformScales()
    {
        layerScales = new Vector3[] { Vector3.one, Vector3.one, Vector3.one, Vector3.one };
        Debug.Log("Todas las escalas resetadas a Vector3.one (uniforme)");
    }

    /// <summary>
    /// Preset para un puente delgado y elegante
    /// </summary>
    [ContextMenu("Preset: Puente Elegante")]
    public void SetElegantBridgeScales()
    {
        layerHeights = new float[] { 0.0f, 0.4f, 0.8f, 1.2f };
        layerScales = new Vector3[] { 
            new Vector3(0.9f, 0.7f, 0.9f),  // Base: más delgada
            new Vector3(0.8f, 1.8f, 0.8f),  // Soporte: pilares altos y delgados
            new Vector3(0.9f, 0.8f, 0.9f),  // Estructura: elegante
            new Vector3(1.0f, 0.4f, 1.0f)   // Superficie: muy plana
        };        Debug.Log("Configurado puente elegante: estructuras delgadas y estilizadas");
    }    /// <summary>
    /// Aplica las escalas actuales a todas las capas existentes en el puente
    /// Útil cuando cambias las escalas en tiempo de ejecución
    /// </summary>
    [ContextMenu("Aplicar Escalas Actuales")]
    public void ApplyCurrentScales()
    {
        if (constructionGrid == null)
        {
            Debug.LogWarning("La grilla no está inicializada. No se pueden aplicar escalas.");
            return;
        }

        Debug.Log("Aplicando escalas actuales a todas las capas existentes...");
        
        // Usar el nuevo método consistente para aplicar escalas
        ApplyConfiguredScalesAfterInit();
    }

    /// <summary>
    /// Método de utilidad para forzar la aplicación de escalas desde el inspector
    /// Funciona tanto en modo editor como en tiempo de ejecución
    /// </summary>
    [ContextMenu("🔧 Forzar Aplicar Escalas (Debug)")]
    public void ForceApplyScales()
    {
        if (constructionGrid == null)
        {
            Debug.LogWarning("La grilla no está inicializada. Inicializando primero...");
            if (Application.isPlaying)
            {
                InitializeGrid();
            }
            else
            {
                Debug.LogError("No se puede inicializar la grilla en modo editor. Por favor ejecuta el juego primero.");
                return;
            }
        }

        Debug.Log("🔧 Forzando aplicación de escalas desde inspector...");
        ApplyConfiguredScalesAfterInit();
    }

    /// <summary>
    /// Reescala dinámicamente toda la grilla según el nuevo quadrantSize
    /// Útil cuando se cambia el tamaño en el inspector durante la edición
    /// </summary>
    [ContextMenu("Reescalar Grilla")]
    public void RescaleGrid()
    {
        if (constructionGrid == null)
        {
            Debug.LogWarning("La grilla no está inicializada. No se puede reescalar.");
            return;
        }

        Debug.Log($"Reescalando grilla con nuevo quadrantSize: {quadrantSize}");

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                if (constructionGrid[x, z] != null && constructionGrid[x, z].quadrantObject != null)
                {
                    // Actualizar posición del cuadrante
                    Vector3 newPosition = transform.position + new Vector3(x * quadrantSize, 0, z * quadrantSize);
                    constructionGrid[x, z].quadrantObject.transform.position = newPosition;
                    constructionGrid[x, z].worldPosition = newPosition;

                    // Reescalar el collider del cuadrante principal
                    if (constructionGrid[x, z].quadrantCollider is BoxCollider quadrantBoxCol)
                    {
                        quadrantBoxCol.size = new Vector3(quadrantSize, quadrantBoxCol.size.y, quadrantSize);
                        quadrantBoxCol.center = new Vector3(quadrantSize / 2, quadrantBoxCol.center.y, quadrantSize / 2);
                    }

                    // Reescalar todas las capas visuales existentes
                    for (int i = 0; i < constructionGrid[x, z].layerRenderers.Length; i++)
                    {
                        if (constructionGrid[x, z].layerRenderers[i] != null &&
                            constructionGrid[x, z].layerRenderers[i].gameObject != null)
                        {
                            GameObject layerObj = constructionGrid[x, z].layerRenderers[i].gameObject;                            // Reposicionar la capa usando las alturas configurables
                            float layerHeight = (i < layerHeights.Length) ? layerHeights[i] : (0.5f * i);
                            Vector3 newLayerPosition = newPosition + new Vector3(
                                quadrantSize / 2,  // Centrado en X
                                layerHeight,       // Altura específica para esta capa
                                quadrantSize / 2   // Centrado en Z
                            );                            layerObj.transform.position = newLayerPosition;

                            // Reescalar la capa usando escalas configurables
                            Vector3 baseScale = new Vector3(quadrantSize, 1f, quadrantSize);
                            Vector3 layerScale = (i < layerScales.Length) ? layerScales[i] : Vector3.one;
                            Vector3 finalScale = Vector3.Scale(baseScale, layerScale);
                            layerObj.transform.localScale = finalScale;

                            // Ajustar colliders de la capa
                            Collider layerCollider = layerObj.GetComponent<Collider>();
                            if (layerCollider is BoxCollider layerBoxCol)
                            {
                                Vector3 originalSize = layerBoxCol.size;
                                layerBoxCol.size = new Vector3(quadrantSize, originalSize.y, quadrantSize);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("Reescalado de grilla completado.");    }    /// <summary>
    /// Método de debug para rellenar todas las capas de todos los cuadrantes del puente
    /// Llamado desde el editor personalizado con un botón
    /// </summary>
    public void DebugRellenarTodoPuente()
    {
        Debug.Log("=== INICIANDO DEBUG: RELLENADO COMPLETO DEL PUENTE ===");
        
        int cuadrantesRellenados = 0;
        int capasRellenadas = 0;
        
        // Crear un GameObject temporal para usar en la construcción de debug
        GameObject debugObject = new GameObject("DebugTempObject");
        
        // Recorrer toda la grilla del puente
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                // Verificar si es un cuadrante válido
                if (!IsValidQuadrant(x, z))
                    continue;
                
                Debug.Log($"Rellenando cuadrante [{x},{z}]...");
                
                // Obtener el ScriptableObject del cuadrante
                BridgeQuadrantSO quadrantSO = GetQuadrantSO(x, z);
                if (quadrantSO == null)
                {
                    Debug.LogWarning($"No se pudo obtener el ScriptableObject del cuadrante [{x},{z}]");
                    continue;
                }
                
                // Rellenar todas las capas del cuadrante (0, 1, 2, 3)
                bool cuadranteCompletado = true;
                for (int capa = 0; capa < quadrantSO.requiredLayers.Length; capa++)
                {
                    // Solo intentar rellenar si la capa no está ya completada
                    if (!quadrantSO.requiredLayers[capa].isCompleted)
                    {
                        // Usar TryBuildLayer del BridgeConstructionGrid
                        // Pasamos el objeto temporal para que pase la validación
                        bool exito = TryBuildLayer(x, z, capa, debugObject);
                        
                        if (exito)
                        {
                            capasRellenadas++;
                            Debug.Log($"  ✓ Capa {capa} completada en cuadrante [{x},{z}]");
                        }
                        else
                        {
                            Debug.LogWarning($"  ✗ No se pudo completar capa {capa} en cuadrante [{x},{z}]");
                            cuadranteCompletado = false;
                        }
                    }
                    else
                    {
                        Debug.Log($"  - Capa {capa} ya estaba completada en cuadrante [{x},{z}]");
                    }
                }
                
                if (cuadranteCompletado)
                {
                    cuadrantesRellenados++;
                }
            }
        }
        
        // Limpiar el objeto temporal
        if (debugObject != null)
        {
            DestroyImmediate(debugObject);
        }
          Debug.Log($"=== DEBUG COMPLETADO ===");
        Debug.Log($"Cuadrantes procesados: {cuadrantesRellenados}");
        Debug.Log($"Capas rellenadas: {capasRellenadas}");
        Debug.Log($"Puente completo: {cuadrantesRellenados} cuadrantes x 4 capas = {cuadrantesRellenados * 4} capas totales");
    }
}