using UnityEngine;
using System.Collections.Generic;

public class HeatSphere : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask detectionLayer = -1;

    [Header("Visual")]
    [SerializeField] private Material heatSphereMaterial;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 10f;
    private float currentCooldown = 0f;

    // Aumentado para abarcar más colliders en escenas densas (rocas, decoraciones, capas visuales)
    private Collider[] detectedColliders = new Collider[128];
    private HashSet<ITurnable> activeTurnables = new HashSet<ITurnable>();
    private GameObject visualSphere;

    private void Start()
    {
        CreateVisualSphere();
        currentCooldown = cooldown;
    }

    private void CreateVisualSphere()
    {
        visualSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualSphere.transform.SetParent(transform);
        visualSphere.transform.localPosition = Vector3.zero;
        visualSphere.transform.localScale = Vector3.one * (detectionRadius * 2f);

        var collider = visualSphere.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        var renderer = visualSphere.GetComponent<MeshRenderer>();

        if (heatSphereMaterial != null)
        {
            renderer.material = heatSphereMaterial;
        }

        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private void Update()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }
        // Nuevo: apagar automáticamente aunque el cooldown haya sido forzado a 0 desde fuera.
        if (currentCooldown <= 0f && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }

        DetectTurnables();
    }

    private void DetectTurnables()
    {
        int detected = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, detectedColliders, detectionLayer, QueryTriggerInteraction.Collide);

        HashSet<ITurnable> currentlyDetected = new HashSet<ITurnable>();

        for (int i = 0; i < detected; i++)
        {
            var col = detectedColliders[i];
            if (col == null) continue;

            var turnable = col.GetComponentInParent<ITurnable>();
            if (turnable != null)
            {
                currentlyDetected.Add(turnable);

                if (!activeTurnables.Contains(turnable))
                {
                    turnable.TurnOn();
                    activeTurnables.Add(turnable);
                }
            }
        }

        var toRemove = new List<ITurnable>();
        foreach (var turnable in activeTurnables)
        {
            if (!currentlyDetected.Contains(turnable))
            {
                turnable.TurnOff();
                toRemove.Add(turnable);
            }
        }

        foreach (var turnable in toRemove)
        {
            activeTurnables.Remove(turnable);
        }
    }

    private void OnDisable()
    {
        foreach (var turnable in activeTurnables)
        {
            if (turnable != null)
            {
                turnable.TurnOff();
            }
        }
        activeTurnables.Clear();
    }

    public void ResetCooldown()
    {
        currentCooldown = cooldown;
    }

    public bool IsOnCooldown()
    {
        return currentCooldown > 0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsOnCooldown() ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public void CooldownOff()
    {
        // Solo ponemos el cooldown a 0; el Update se encargará de desactivar en el siguiente frame.
        currentCooldown = 0f;
    }
}