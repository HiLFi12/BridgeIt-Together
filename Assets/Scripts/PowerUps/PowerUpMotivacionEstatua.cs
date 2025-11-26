using System.Collections;
using UnityEngine;

public class PowerUpMotivacionEstatua : PowerUpBase
{
    // Evento estático para notificar cuando el power-up es activado
    public static event System.Action<PowerUpMotivacionEstatua> OnEstatuaActivated;
    
    [Header("Referencias Específicas")]
    public GameObject statueObject;
    public Transform destinationPoint;
    public BridgeConstructionGrid bridgeGrid;
    
    [Header("Construcción Automática")]
    [Tooltip("Índice de capa máximo a construir (0=Base, 1=Soporte, 2=Superficie).")]
    [SerializeField, Range(0, 2)] private int buildUpToLayer = 1; // Por defecto construye hasta capa 1 (layers 0 y 1)
    
    public float effectDuration = 20f;

    private bool isAtDestination = false;
    private bool isBallistaTriggered = false;

    // Llamar cuando la estatua llegue al destino
    public void OnStatueArrived()
    {
        isAtDestination = true;
    }

    // Llamar cuando la ballesta golpee la estatua
    public void OnBallistaHit()
    {
        if (isAtDestination && !isActive)
        {
            isBallistaTriggered = true;
            TryActivate(null);
        }
    }

    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        // Construir automáticamente el puente (igual que PowerUpCalorHumano y PowerUpRitualGranFuego)
        if (bridgeGrid != null)
        {
            Debug.Log($"[PowerUpMotivacionEstatua] ===== INICIO CONSTRUCCIÓN =====");
            Debug.Log($"[PowerUpMotivacionEstatua] buildUpToLayer = {buildUpToLayer}");
            
            // Topes seguros según la grilla
            int maxGridLayer = (bridgeGrid.layerHeights != null)
                ? Mathf.Max(0, bridgeGrid.layerHeights.Length - 1)
                : 2;
            
            int targetMax = Mathf.Clamp(buildUpToLayer, 0, maxGridLayer);
            Debug.Log($"[PowerUpMotivacionEstatua] targetMax = {targetMax} (maxGridLayer={maxGridLayer})");
            
            // Verificar estado ANTES de construir (solo primer cuadrante como muestra)
            var testQuadrant = GameObject.Find("Quadrant_0_0")?.GetComponent<BridgeQuadrant>();
            if (testQuadrant != null)
            {
                Debug.Log($"[PowerUpMotivacionEstatua] ANTES - Quadrant_0_0.currentLayer = {testQuadrant.GetCurrentLayer()}");
            }
            
            for (int x = 0; x < bridgeGrid.gridWidth; x++)
            {
                for (int z = 0; z < bridgeGrid.gridLength; z++)
                {
                    for (int layerIndex = 0; layerIndex <= targetMax; layerIndex++)
                    {
                        bool success = bridgeGrid.TryBuildLayer(x, z, layerIndex, null);
                        if (!success && layerIndex == 0)
                        {
                            Debug.LogWarning($"[PowerUpMotivacionEstatua] FALLO construyendo layer {layerIndex} en [{x},{z}]");
                        }
                    }
                }
            }
            
            // Verificar estado DESPUÉS de construir
            if (testQuadrant != null)
            {
                Debug.Log($"[PowerUpMotivacionEstatua] DESPUÉS - Quadrant_0_0.currentLayer = {testQuadrant.GetCurrentLayer()}");
                Debug.Log($"[PowerUpMotivacionEstatua] DESPUÉS - Quadrant_0_0.CanBuildLayer(0) = {testQuadrant.CanBuildLayer(0)}");
                Debug.Log($"[PowerUpMotivacionEstatua] DESPUÉS - Quadrant_0_0.CanBuildLayer(1) = {testQuadrant.CanBuildLayer(1)}");
                Debug.Log($"[PowerUpMotivacionEstatua] DESPUÉS - Quadrant_0_0.CanBuildLayer(2) = {testQuadrant.CanBuildLayer(2)}");
            }
            
            Debug.Log($"[PowerUpMotivacionEstatua] ===== FIN CONSTRUCCIÓN =====");
        }
        
        // Feedback visual/sonoro de activación
        yield return new WaitForSeconds(effectDuration);
        Despawn();
    }

    // Entrada directa si el collider está en el mismo GameObject
    private void OnCollisionEnter(Collision collision)
    {
        if (isActive) return;        
        if (collision.collider && collision.collider.GetComponent<Arrow>() != null)
        {
            HandleArrowHit(collision.collider.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive) return;
        if (other.GetComponent<Arrow>() != null)
        {
            HandleArrowHit(other.gameObject);
        }
    }

    // Método usado también por el relay en hijos
    public void HandleArrowHit(GameObject arrow)
    {
        if (isActive) return;
        TryActivate(arrow);
    }

    public override void TryActivate(GameObject activator)
    {
        base.TryActivate(activator);
        
        // Notificar que la estatua fue activada (para tutoriales)
        OnEstatuaActivated?.Invoke(this);
    }
} 