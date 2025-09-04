using UnityEngine;

namespace BridgeItTogether.Gameplay.SafeZones
{
    [DisallowMultipleComponent]
    public class SafeZoneArea : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(6f, 6f);
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color fillColor = new Color(0f, 1f, 0f, 0.08f);
        [SerializeField] private Color wireColor = new Color(0f, 1f, 0f, 0.6f);

        public Vector2 Size
        {
            get => size;
            set => size = new Vector2(Mathf.Max(0f, value.x), Mathf.Max(0f, value.y));
        }

        public Vector3 GetRandomPointInside()
        {
            Vector2 half = size * 0.5f;
            float rx = Random.Range(-half.x, half.x);
            float rz = Random.Range(-half.y, half.y);
            return new Vector3(transform.position.x + rx, transform.position.y, transform.position.z + rz);
        }

        public float SqrDistanceTo(Vector3 worldPos)
        {
            return (transform.position - worldPos).sqrMagnitude;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            Vector3 center = transform.position;
            Vector3 sz = new Vector3(size.x, 0.02f, size.y);
            Gizmos.color = fillColor;
            Gizmos.DrawCube(center, sz);
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(center, sz);
        }
#endif
    }
}
