using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BaseProbabilitySkill : MonoBehaviour
{
    [Header("Detection (OverlapSphereNonAlloc)")]
    [SerializeField] private Transform detectionPoint;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private LayerMask bridgeLayer;
    [SerializeField] private string bridgeQuadrantTag = "BridgeQuadrant";

    [Header("Probability")]
    [Range(0f, 1f)]
    [SerializeField] private float probability = 0.35f;

    [Header("Spawn")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.up * 0.25f;
    [SerializeField] private Transform spawnParent; // (unused after unparent, kept for backward compat)
    [SerializeField] private int ammo = 5;

    [Header("Behavior")]
    [SerializeField] private bool oneRollPerCollider = true;

    // Buffers & tracking
    private readonly Collider[] overlap = new Collider[32];
    private readonly HashSet<Collider> previousInside = new();
    private readonly HashSet<Collider> insideNow = new();
    private readonly HashSet<Collider> rolledLifetime = new();

    private void Awake()
    {
        if (!detectionPoint) detectionPoint = transform;
    }

    private void OnValidate()
    {
        if (!detectionPoint) detectionPoint = transform;
        detectionRadius = Mathf.Max(0.01f, detectionRadius);
        probability = Mathf.Clamp01(probability);
    }

    // Made virtual so derived classes can extend (e.g. extra logic before/after detection).
    protected virtual void FixedUpdate()
    {
        if (ammo <= 0) return;
        RunDetection();
    }

    protected void RunDetection()
    {
        insideNow.Clear();

        int count = Physics.OverlapSphereNonAlloc(
            detectionPoint.position,
            detectionRadius,
            overlap,
            bridgeLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            var col = overlap[i];
            if (!col) continue;
            if (!PassesFilters(col)) continue;

            insideNow.Add(col);

            bool isNewEntry = !previousInside.Contains(col);
            if (!isNewEntry) continue;

            if (oneRollPerCollider && rolledLifetime.Contains(col))
                continue;

            bool success = RollFor(col);
            if (oneRollPerCollider)
                rolledLifetime.Add(col);

            // If you wanted a "fail" hook you could add another virtual here.
            if (success && ammo <= 0)
                break; // optional early exit when out of ammo
        }

        previousInside.Clear();
        foreach (var c in insideNow)
            previousInside.Add(c);
    }

    private bool PassesFilters(Collider col)
    {
        if (!col) return false;
        if (!string.IsNullOrEmpty(bridgeQuadrantTag) && !col.CompareTag(bridgeQuadrantTag))
            return false;
        return true;
    }

    // Returns success; virtual so subclasses can override spawn logic or probability handling.
    protected virtual bool RollFor(Collider col)
    {
        if (ammo <= 0) return false;

        float roll = Random.value;
        bool success = roll <= probability;

        GameObject spawned = null;
        if (success)
        {
            if (prefab != null)
            {
                Vector3 pos = col.bounds.center + spawnOffset;
                Quaternion rot = prefab.transform.rotation;
                spawned = Instantiate(prefab, pos, rot);
                // Ensure detached
                if (spawned && spawned.transform.parent != null)
                    spawned.transform.SetParent(null, true);
            }

            ammo = Mathf.Max(0, ammo - 1);
            OnProbabilitySuccess(col, spawned);
        }

        return success;
    }

    // Success hook (empty by default).
    protected virtual void OnProbabilitySuccess(Collider col, GameObject spawnedInstance) { }

    // Public API
    public void AddAmmo(int amount) => ammo += amount;
    public int GetAmmo() => ammo;
    public void SetProbability(float p) => probability = Mathf.Clamp01(p);

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!detectionPoint) detectionPoint = transform;
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(detectionPoint.position, detectionRadius);
    }
#endif
}