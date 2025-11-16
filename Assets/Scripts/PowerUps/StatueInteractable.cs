using System;
using System.Collections;
using UnityEngine;

public class StatueInteractable : PowerUpBase, IInteractable, IUIActivatable
{
    private bool isCarried = false;

    [Header("Lifetime / Despawn")]
    [Tooltip("Tiempo en segundos que la estatua permanece en el mapa antes de desaparecer si no se activó.")]
    public float lifeDuration = 60f;
    [Tooltip("Prefab de efecto que se instancia al morir (opcional).")]
    public GameObject dieEffectPrefab;
    [Tooltip("Si true el efecto se parenta a la estatua antes de destruirla.")]
    public bool attachDieEffect = false;

    [Header("Construcción Automática")]
    [Tooltip("Grid de puente sobre el que aplicar construcción automática cuando la estatua se activa y muere.")]
    [SerializeField] private BridgeConstructionGrid bridgeGrid;
    [Tooltip("Índice de capa máximo a construir (0=Base, 1=Soporte, 2=Superficie). Igual que RitualGranFuego.")]
    [SerializeField, Range(0, 2)] private int buildUpToLayer = 2;
    
    [Header("UI Configuration")]
    [SerializeField] private int uiIndex = 3;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject shadow;
    
    public int UIIndex => uiIndex;

    private float lifeTimer;

    public InteractPriority InteractPriority => InteractPriority.High;

    protected override void Start()
    {
        base.Start();
        shadow.SetActive(false);
    }

    public void Interact(GameObject interactor)
    {
        if (isCarried) return;
        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder != null)
        {
            holder.PickUpExistingInstance(gameObject);
            isCarried = true;
            
            // Suscribirse al evento OnDropped para detectar cuando se suelta
            holder.OnDropped += OnStatueDropped;
            
            // Feedback visual/sonoro opcional aquí
        }
    }
    
    private void OnStatueDropped(GameObject droppedObject)
    {
        // Verificar que el objeto soltado sea esta estatua
        if (droppedObject == gameObject)
        {
            isCarried = false;
            
            // Desuscribirse del evento para evitar memory leaks
            PlayerObjectHolder[] holders = FindObjectsByType<PlayerObjectHolder>(FindObjectsSortMode.None);
            foreach (var holder in holders)
            {
                holder.OnDropped -= OnStatueDropped;
            }
        }
    }
    
    public void SetUIIndex(int index)
    {
        uiIndex = index;
    }

    private void Update()
    {
        // Contador de vida específico de la estatua (independiente del TTL de PowerUpBase)
        // Countdown de vida
        if (!isActive && lifeDuration > 0f)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifeDuration)
            {
                // Si expira por tiempo sin ser activada, solo se destruye sin efecto
                Despawn();
            }
        }

        if (isCarried)
        {
            canvas.SetActive(false);
        }
        else
        {
            canvas.SetActive(true);
        }
    }

    // Detección de Arrow para activar la construcción y luego morir
    private void OnCollisionEnter(Collision collision)
    {
        if (isActive) return;
        if (collision.collider && collision.collider.GetComponent<Arrow>() != null)
        {
            TryActivate(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive) return;
        if (other.GetComponent<Arrow>() != null)
        {
            TryActivate(other.gameObject);
        }
    }

    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }

    /// <summary>
    /// Construye automáticamente los cuadrantes del puente hasta la capa indicada en buildUpToLayer,
    /// reutilizando la misma lógica que PowerUpRitualGranFuego.
    /// </summary>
    private void ConstructBridgeAutomatically()
    {
        if (bridgeGrid == null) return;

        int maxGridLayer = (bridgeGrid.layerHeights != null)
            ? Mathf.Max(0, bridgeGrid.layerHeights.Length - 1)
            : 2;

        int targetMax = Mathf.Clamp(buildUpToLayer, 0, maxGridLayer);

        for (int x = 0; x < bridgeGrid.gridWidth; x++)
        {
            for (int z = 0; z < bridgeGrid.gridLength; z++)
            {
                var so = bridgeGrid.GetQuadrantSO(x, z);
                if (so == null || so.requiredLayers == null) continue;

                for (int layerIndex = 0; layerIndex <= targetMax && layerIndex < so.requiredLayers.Length; layerIndex++)
                {
                    if (!so.requiredLayers[layerIndex].isCompleted)
                    {
                        // marcar la capa como completada directamente
                        so.requiredLayers[layerIndex].isCompleted = true;
                    }
                }

                // actualizar visuales/colisionadores del cuadrante después de modificar el SO
                bridgeGrid.RefreshQuadrantVisuals(x, z);
            }
        }
    }

    /// <summary>
    /// Efecto del power-up: construir el puente y luego destruir la estatua.
    /// </summary>
    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        ConstructBridgeAutomatically();
        Despawn();
        yield break;
    }
}