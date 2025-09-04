using System.Collections.Generic;
using UnityEngine;

public class SoldierCaravanProbabilitySkill : BaseProbabilitySkill
{
    [Header("Guards to detach on success")]
    [SerializeField] private List<GameObject> guards = new();

    [Header("Destroy")]
    [SerializeField] private bool destroyWholeGameObject = true;

    private bool executed;

    protected override void OnProbabilitySuccess(Collider col, GameObject spawnedInstance)
    {
        if (executed) return;
        executed = true;

        if (guards != null)
        {
            for (int i = 0; i < guards.Count; i++)
            {
                var go = guards[i];
                if (!go) continue;
                var t = go.transform;
                if (t.IsChildOf(transform))
                    t.SetParent(null, true); // Only detach
            }
        }

        if (destroyWholeGameObject)
            Destroy(gameObject);
        else
            Destroy(this);
    }
}