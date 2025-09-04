using System.Collections;
using UnityEngine;

public class PropulsedCarGuardProbabilitySkill : BaseProbabilitySkill
{
    [Header("Guard Reference")]
    [SerializeField] private GameObject guardGO;

    [Header("Drop Zone Tags")]
    [SerializeField] private string rightDropZoneTag = "RightDropZone";
    [SerializeField] private string leftDropZoneTag = "LeftDropZone";

    [Header("Launch Settings (Parabola)")]
    [SerializeField, Range(10f, 80f)] private float launchAngleDeg = 45f;
    [SerializeField, Min(0.1f)] private float launchSpeed = 12f;
    [SerializeField, Min(0.1f)] private float launchGravity = 9.81f;
    [SerializeField] private bool kinematicDuringLaunch = true;
    [SerializeField] private float minLaunchDistance = 0.05f;

    private bool executed;

    protected override void OnProbabilitySuccess(Collider col, GameObject spawnedInstance)
    {
        if (executed) return;
        executed = true;
        if (!guardGO) return;

        bool toRight = Vector3.Dot(transform.forward, Vector3.right) >= 0f;
        string targetTag = toRight ? rightDropZoneTag : leftDropZoneTag;
        Transform dropT = FindNearestByTag(targetTag, transform.position);

        var guardT = guardGO.transform;
        if (guardT.IsChildOf(transform))
            guardT.SetParent(null, true); // Only detach

        Vector3 targetPos = dropT ? dropT.position : guardT.position;
        StartCoroutine(LaunchGuardRoutine(guardT, targetPos));
    }

    private Transform FindNearestByTag(string tag, Vector3 from)
    {
        if (string.IsNullOrEmpty(tag)) return null;
        GameObject[] gos;
        try { gos = GameObject.FindGameObjectsWithTag(tag); }
        catch { return null; }
        if (gos == null || gos.Length == 0) return null;

        Transform best = null;
        float bestD = float.MaxValue;
        for (int i = 0; i < gos.Length; i++)
        {
            var go = gos[i];
            if (!go) continue;
            float d = (go.transform.position - from).sqrMagnitude;
            if (d < bestD) { bestD = d; best = go.transform; }
        }
        return best;
    }

    private IEnumerator LaunchGuardRoutine(Transform target, Vector3 destino)
    {
        if (!target) yield break;

        Vector3 start = target.position;
        Vector3 flat = new Vector3(destino.x - start.x, 0f, destino.z - start.z);
        float distance = flat.magnitude;

        if (distance < minLaunchDistance)
        {
            target.position = destino;
            yield break;
        }

        Vector3 dir = flat / Mathf.Max(distance, 0.0001f);
        float angleRad = launchAngleDeg * Mathf.Deg2Rad;
        float v = Mathf.Max(0.01f, launchSpeed);
        float cos = Mathf.Cos(angleRad);
        float tan = Mathf.Tan(angleRad);
        float g = Mathf.Max(0.01f, launchGravity);
        float totalTime = distance / (v * cos);

        Rigidbody trb = target.GetComponent<Rigidbody>();
        bool hadRB = trb;
        bool prevKin = false;
        if (hadRB && kinematicDuringLaunch)
        {
            prevKin = trb.isKinematic;
            trb.isKinematic = true;
        }

        float t = 0f;
        while (t < totalTime && target)
        {
            t += Time.deltaTime;
            float ct = Mathf.Min(t, totalTime);
            float x = v * cos * ct;
            float yOffset = x * tan - (g * x * x) / (2f * v * v * cos * cos);

            Vector3 pos = start + dir * x;
            pos.y = start.y + yOffset;
            target.position = pos;
            yield return null;
        }

        if (target)
            target.position = destino;

        if (hadRB && kinematicDuringLaunch && trb)
            trb.isKinematic = prevKin;
        // No further guard configuration.
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        bool toRight = Vector3.Dot(transform.forward, Vector3.right) >= 0f;
        string tag = toRight ? rightDropZoneTag : leftDropZoneTag;
        var drop = FindNearestByTag(tag, transform.position);
        if (drop)
        {
            Gizmos.color = toRight ? new Color(0.2f, 1f, 0.2f, 0.9f) : new Color(1f, 0.2f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(drop.position, 0.25f);
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, drop.position + Vector3.up * 0.1f);
        }
        if (guardGO)
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.9f);
            Gizmos.DrawWireSphere(guardGO.transform.position, 0.15f);
        }
    }
#endif
}