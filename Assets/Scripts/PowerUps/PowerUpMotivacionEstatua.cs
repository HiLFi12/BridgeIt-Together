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
    public int layerToBuild = 0; // Capa que se construye con el efecto
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
        // Permitir construir una hilera completa del ancho del puente con un solo material
        if (bridgeGrid != null)
        {
            for (int x = 0; x < bridgeGrid.gridWidth; x++)
            {
                for (int z = 0; z < bridgeGrid.gridLength; z++)
                {
                    var so = bridgeGrid.GetQuadrantSO(x, z);
                    if (so != null)
                    {
                        // Solo construir si la capa es válida
                        if (!so.requiredLayers[layerToBuild].isCompleted)
                        {
                            so.TryAddLayer(layerToBuild, null);
                        }
                    }
                }
            }
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