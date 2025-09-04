using System.Collections;
using UnityEngine;

public class PowerUpMotivacionEstatua : PowerUpBase
{
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
} 