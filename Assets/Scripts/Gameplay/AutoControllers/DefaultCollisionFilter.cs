using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

namespace BridgeItTogether.Gameplay.AutoControllers
{
    /// <summary>
    /// Filtro por defecto: ignora CollidableNT y/o cualquier MonoBehaviour que implemente ICollidableNT.
    /// </summary>
    public class DefaultCollisionFilter : ICollisionFilter
    {
        private GameObject owner;
        private Collider[] ownerCols;

        public void Initialize(GameObject owner)
        {
            this.owner = owner;
            ownerCols = owner != null ? owner.GetComponentsInChildren<Collider>() : new Collider[0];
        }

        public bool ShouldIgnore(Collider other)
        {
            if (other == null) return false;
            return FindMarkedRootFromCollider(other) != null;
        }

        public void ApplyIgnore(Collider other)
        {
            if (other == null || ownerCols == null) return;
            var markedRoot = FindMarkedRootFromCollider(other);
            Collider[] otros;
            if (markedRoot != null)
            {
                otros = markedRoot.GetComponentsInChildren<Collider>();
            }
            else
            {
                // Fallback: solo el collider impactado
                otros = new Collider[] { other };
            }
            foreach (var mc in ownerCols)
            {
                if (mc == null) continue;
                foreach (var oc in otros)
                {
                    if (oc == null) continue;
                    Physics.IgnoreCollision(mc, oc, true);
                }
            }
        }

        private Transform FindMarkedRootFromCollider(Collider other)
        {
            if (other == null) return null;
            Transform t = other.transform;
            while (t != null)
            {
                if (t.GetComponent<CollidableNT>() != null) return t;
                var mbs = t.GetComponents<MonoBehaviour>();
                foreach (var mb in mbs) if (mb is ICollidableNT) return t;
                t = t.parent;
            }
            return null;
        }

        public void ApplyIgnoreToAllMarkedInScene()
        {
            if (owner == null) return;
            // 1) CollidableNT explícitos
            var marcados = Object.FindObjectsByType<CollidableNT>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var m in marcados)
            {
                if (m == null) continue;
                var cols = m.GetComponentsInChildren<Collider>();
                foreach (var c in cols) ApplyIgnore(c);
            }
            // 2) Cualquier MB que implemente la interfaz
            var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var mb in behaviours)
            {
                if (mb == null) continue;
                if (mb is ICollidableNT)
                {
                    var cols = mb.GetComponentsInChildren<Collider>();
                    foreach (var c in cols) ApplyIgnore(c);
                }
            }
        }
    }
}
