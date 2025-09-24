using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.AutoControllers; // Heredar de AutoController

public class RoyalCar : AutoController
{
    [Header("RoyalCar - Detección extra (además de la base)")]
    [SerializeField] private bool extraDetectionEnabled = true;

    [Header("Detección Extra 1")] 
    [SerializeField] private Transform extraPoint1;
    [SerializeField] private float extraRadius1 = 1.5f;
    [SerializeField] private LayerMask extraBridgeLayer1; // capa(s) de colliders del puente para el modo 1
    [SerializeField, Min(0f)] private float extraCooldown1 = 0.2f;

    [Header("Detección Extra 2")] 
    [SerializeField] private Transform extraPoint2;
    [SerializeField] private float extraRadius2 = 1.5f;
    [SerializeField] private LayerMask extraBridgeLayer2; // capa(s) de colliders del puente para el modo 2
    [SerializeField, Min(0f)] private float extraCooldown2 = 0.2f;

    [Header("Filtros y Debug (extra)")]
    [SerializeField] private string extraBridgeQuadrantTag = "BridgeQuadrant"; // filtro opcional por tag, similar al base
    [SerializeField] private bool debugExtra = false;

    private float extraTimer1;
    private float extraTimer2;

    private readonly Collider[] extraOverlap = new Collider[8];

    private void Start()
    {
        if (extraPoint1 == null) extraPoint1 = transform;
        if (extraPoint2 == null) extraPoint2 = transform;

        extraTimer1 = 0f;
        extraTimer2 = 0f;
    }

    private void Update()
    {
        if (!extraDetectionEnabled) return;

        extraTimer1 -= Time.deltaTime;
        if (extraTimer1 <= 0f)
        {
            DoExtraInteract(extraPoint1, extraRadius1, extraBridgeLayer1);
            extraTimer1 = extraCooldown1;
        }

        extraTimer2 -= Time.deltaTime;
        if (extraTimer2 <= 0f)
        {
            DoExtraInteract(extraPoint2, extraRadius2, extraBridgeLayer2);
            extraTimer2 = extraCooldown2;
        }
    }

    private void DoExtraInteract(Transform point, float radius, LayerMask layer)
    {
        if (point == null || radius <= 0f) return;

        int count = Physics.OverlapSphereNonAlloc(
            point.position,
            radius,
            extraOverlap,
            layer,
            QueryTriggerInteraction.Collide
        );

        if (count <= 0) return;

        var legacy = GetComponent<VehicleBridgeCollision>();
        if (legacy == null)
        {
            if (debugExtra)
                Debug.LogWarning("[RoyalCar] VehicleBridgeCollision no encontrado en el vehículo.", this);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var col = extraOverlap[i];
            if (col == null) continue;

            if (!string.IsNullOrEmpty(extraBridgeQuadrantTag) && !col.CompareTag(extraBridgeQuadrantTag))
                continue;

            VehicleBridgeCollision.HandleTriggerFromChild(gameObject, col);

            if (debugExtra)
                Debug.Log($"[RoyalCar] Extra interact -> {col.name} (tag: {col.tag})", col);
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject != gameObject) return;
#endif
        if (!extraDetectionEnabled) return;

        var p1 = extraPoint1 != null ? extraPoint1 : transform;
        var p2 = extraPoint2 != null ? extraPoint2 : transform;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f); // Naranja
        Gizmos.DrawWireSphere(p1.position, Mathf.Max(0f, extraRadius1));

        Gizmos.color = new Color(1f, 0f, 1f, 0.9f); // Magenta
        Gizmos.DrawWireSphere(p2.position, Mathf.Max(0f, extraRadius2));
    }
}
