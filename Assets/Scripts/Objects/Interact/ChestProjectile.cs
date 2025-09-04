using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChestProjectile : MonoBehaviour
{
    [SerializeField] private string bridgeQuadrantTag = "BridgeQuadrant";
    [SerializeField] private GameObject shaderPrefab;

    [Header("Interacción con Puente (OverlapSphere)")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask bridgeLayer; // capa de colliders del puente

    [Header("Auto detección")]
    [SerializeField] private bool autoCheckInFixedUpdate = true;

    // Buffers
    private readonly Collider[] overlap = new Collider[8];
    private readonly Dictionary<Transform, Coroutine> activeLaunches = new();

    private bool consumed; // evita múltiples triggers/destrucciones

    private void Awake()
    {
        if (interactionPoint == null) interactionPoint = transform;
    }

    private void FixedUpdate()
    {
        if (autoCheckInFixedUpdate && !consumed)
            ChestInteractLayer();
    }

    public void ChestInteractLayer()
    {
        if (consumed) return;

        int count = Physics.OverlapSphereNonAlloc(
            interactionPoint.position,
            interactionRadius,
            overlap,
            bridgeLayer,
            QueryTriggerInteraction.Collide
        );

        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            var col = overlap[i];
            if (col == null) continue;

            // Filtrado opcional por tag del cuadrante
            if (!string.IsNullOrEmpty(bridgeQuadrantTag) && !col.CompareTag(bridgeQuadrantTag))
                continue;

            // Comportamiento tipo VehicleChildCollider: reportar trigger al sistema del puente
            VehicleBridgeCollision.HandleTriggerFromChild(gameObject, col);

            if (shaderPrefab != null)
                Instantiate(shaderPrefab, transform.position, transform.rotation);

            consumed = true;
            Destroy(gameObject);
            return; // salir para no procesar múltiples colliders en el mismo frame
        }
    }

    private void OnDrawGizmosSelected()
    {
        var p = interactionPoint != null ? interactionPoint : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(p.position, interactionRadius);
    }
}