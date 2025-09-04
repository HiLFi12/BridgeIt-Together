using System.Collections;
using UnityEngine;

/// <summary>
/// Power Up de la era prehistórica: "Ritual del Gran Fuego"
/// Los jugadores deben usar PaloIgnifugo encendidos para encender ambas antorchas del tótem
/// casi simultáneamente para activar el efecto que construye automáticamente los cuadrantes
/// del puente hasta la capa 3.
/// </summary>
public class PowerUpRitualGranFuego : PowerUpBase
{
    [Header("Referencias del Tótem")]
    [SerializeField] private GameObject leftTorchCollider; // Collider de la antorcha izquierda
    [SerializeField] private GameObject rightTorchCollider; // Collider de la antorcha derecha
    public BridgeConstructionGrid bridgeGrid; // Referencia al sistema de puentes (pública para PowerUpSpawner)
    
    [Header("Configuración del Ritual")]
    [SerializeField] private float torchActiveTime = 1f; // Tiempo que permanece encendida cada antorcha
    [SerializeField] private GameObject torchFireEffectPrefab; // Efecto visual del fuego en las antorchas
    
    // Estado de las antorchas
    private bool leftTorchLit = false;
    private bool rightTorchLit = false;
    private float leftTorchTimer = 0f;
    private float rightTorchTimer = 0f;
    
    // Efectos visuales de las antorchas
    private GameObject leftTorchFireEffect;
    private GameObject rightTorchFireEffect;

    protected override void Start()
    {
        base.Start();
        
        // Configurar los componentes TorchInteractable en los colliders
        SetupTorchInteractables();
    }

    private void SetupTorchInteractables()
    {
        // Configurar antorcha izquierda
        if (leftTorchCollider != null)
        {
            TorchInteractable leftInteractable = leftTorchCollider.GetComponent<TorchInteractable>();
            if (leftInteractable == null)
            {
                leftInteractable = leftTorchCollider.AddComponent<TorchInteractable>();
            }
            leftInteractable.SetupTorch(TorchInteractable.TorchSide.Left, this);
        }

        // Configurar antorcha derecha
        if (rightTorchCollider != null)
        {
            TorchInteractable rightInteractable = rightTorchCollider.GetComponent<TorchInteractable>();
            if (rightInteractable == null)
            {
                rightInteractable = rightTorchCollider.AddComponent<TorchInteractable>();
            }
            rightInteractable.SetupTorch(TorchInteractable.TorchSide.Right, this);
        }
    }

    private void Update()
    {
        if (!isAvailable) return;

        // Manejar temporizador de antorcha izquierda
        if (leftTorchLit)
        {
            leftTorchTimer += Time.deltaTime;
            if (leftTorchTimer >= torchActiveTime)
            {
                ExtinguishLeftTorch();
            }
        }

        // Manejar temporizador de antorcha derecha
        if (rightTorchLit)
        {
            rightTorchTimer += Time.deltaTime;
            if (rightTorchTimer >= torchActiveTime)
            {
                ExtinguishRightTorch();
            }
        }

        // Verificar si ambas antorchas están encendidas simultáneamente
        if (leftTorchLit && rightTorchLit)
        {
            TryActivate(null); // Activación cooperativa sin activador específico
        }
    }

    /// <summary>
    /// Enciende la antorcha izquierda del tótem
    /// </summary>
    public void LightLeftTorch()
    {
        if (!isAvailable) return;

        leftTorchLit = true;
        leftTorchTimer = 0f;
        
        // Activar efecto visual
        if (torchFireEffectPrefab != null && leftTorchCollider != null)
        {
            if (leftTorchFireEffect != null)
            {
                Destroy(leftTorchFireEffect);
            }
            leftTorchFireEffect = Instantiate(torchFireEffectPrefab, leftTorchCollider.transform);
        }
    }

    /// <summary>
    /// Enciende la antorcha derecha del tótem
    /// </summary>
    public void LightRightTorch()
    {
        if (!isAvailable) return;

        rightTorchLit = true;
        rightTorchTimer = 0f;
        
        // Activar efecto visual
        if (torchFireEffectPrefab != null && rightTorchCollider != null)
        {
            if (rightTorchFireEffect != null)
            {
                Destroy(rightTorchFireEffect);
            }
            rightTorchFireEffect = Instantiate(torchFireEffectPrefab, rightTorchCollider.transform);
        }
    }

    /// <summary>
    /// Apaga la antorcha izquierda
    /// </summary>
    private void ExtinguishLeftTorch()
    {
        leftTorchLit = false;
        leftTorchTimer = 0f;
        
        if (leftTorchFireEffect != null)
        {
            Destroy(leftTorchFireEffect);
            leftTorchFireEffect = null;
        }
    }

    /// <summary>
    /// Apaga la antorcha derecha
    /// </summary>
    private void ExtinguishRightTorch()
    {
        rightTorchLit = false;
        rightTorchTimer = 0f;
        
        if (rightTorchFireEffect != null)
        {
            Destroy(rightTorchFireEffect);
            rightTorchFireEffect = null;
        }
    }

    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        // Apagar las antorchas ya que el ritual se completó
        ExtinguishLeftTorch();
        ExtinguishRightTorch();

        if (bridgeGrid != null)
        {
            // Construir automáticamente todos los cuadrantes hasta la capa 3
            // (según el spec: capas 0, 1 y 2, no la capa 3 que sería la 4ta capa)
            ConstructBridgeAutomatically();
            
            // Esperar la duración del efecto
            yield return new WaitForSeconds(duration);
        }
        else
        {
            Debug.LogError("PowerUpRitualGranFuego: BridgeConstructionGrid no está asignado.");
            yield return new WaitForSeconds(1f);
        }

        Despawn();
    }

    /// <summary>
    /// Construye automáticamente los cuadrantes del puente hasta la capa 3 (índice 2)
    /// según las especificaciones del documento
    /// </summary>
    private void ConstructBridgeAutomatically()
    {
        for (int x = 0; x < bridgeGrid.gridWidth; x++)
        {
            for (int z = 0; z < bridgeGrid.gridLength; z++)
            {
                // Construir capas 0, 1 y 2 (hasta la capa 3 según el spec)
                for (int layerIndex = 0; layerIndex <= 2; layerIndex++)
                {
                    // Solo construir si el cuadrante no está ya completo en esta capa
                    // o no ha alcanzado dicha capa
                    bridgeGrid.TryBuildLayer(x, z, layerIndex, null);
                }
            }
        }
    }

    protected override void Despawn()
    {
        // Limpiar efectos visuales antes de destruir
        if (leftTorchFireEffect != null)
        {
            Destroy(leftTorchFireEffect);
        }
        if (rightTorchFireEffect != null)
        {
            Destroy(rightTorchFireEffect);
        }

        base.Despawn();
    }

    // Métodos para validación en el inspector
    private void OnValidate()
    {
        if (leftTorchCollider == null || rightTorchCollider == null)
        {
            Debug.LogWarning("PowerUpRitualGranFuego: Asigna los colliders de las antorchas en el inspector.");
        }
        
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
        }
    }
} 