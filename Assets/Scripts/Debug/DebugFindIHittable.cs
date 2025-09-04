using System.Linq;
using UnityEngine;

public class DebugFindIHittable : MonoBehaviour
{
    [ContextMenu("Log IHittable children")]
    private void LogIHittables()
    {
        var comps = GetComponentsInChildren<MonoBehaviour>(true);
        var hits = comps.Where(c => c != null && c.GetType().GetInterfaces()
                         .Any(i => i.Name == "IHittable" || i.Name == "IHitable"))
                        .ToList();

        if (hits.Count == 0)
        {
            Debug.Log("[DebugFindIHittable] No se encontró ningún hijo que implemente IHittable/IHitable.", this);
            return;
        }

        foreach (var c in hits)
            Debug.Log($"[IHittable] {GetPathFrom(transform, c.transform)} -> {c.GetType().Name}", c);
    }

    private static string GetPathFrom(Transform root, Transform t)
    {
        var path = t.name;
        while (t.parent != null && t.parent != root) { t = t.parent; path = t.name + "/" + path; }
        return (t.parent == root) ? root.name + "/" + path : t.name;
    }
}