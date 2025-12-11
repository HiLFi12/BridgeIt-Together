using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class RotationMeshLockSizer : MonoBehaviour
{
    [Header("Fuente del tamaño")]
    [Tooltip("Si está vacío, se busca en este GO o en hijos.")]
    [SerializeField] private MeshFilter targetMeshFilter;

    [Header("Actualización")]
    [SerializeField] private bool updateOnScaleChange = true;
    [SerializeField] private bool updateEveryFrame = false;

    private BoxCollider _bc;
    private Vector3 _lastLossyScale;

    private void Awake()
    {
        _bc = GetComponent<BoxCollider>();

        if (targetMeshFilter == null)
            targetMeshFilter = GetComponent<MeshFilter>();

        if (targetMeshFilter == null)
            targetMeshFilter = GetComponentInChildren<MeshFilter>();

        _lastLossyScale = transform.lossyScale;
        FitToMeshLocalBounds();
    }

    private void LateUpdate()
    {
        if (updateEveryFrame)
        {
            FitToMeshLocalBounds();
            return;
        }

        if (updateOnScaleChange && transform.lossyScale != _lastLossyScale)
        {
            _lastLossyScale = transform.lossyScale;
            FitToMeshLocalBounds();
        }
    }

    private void FitToMeshLocalBounds()
    {
        if (_bc == null || targetMeshFilter == null || targetMeshFilter.sharedMesh == null)
            return;

        // Bounds en espacio LOCAL del mesh
        Bounds meshLocal = targetMeshFilter.sharedMesh.bounds;

        // Convertir ese bounds desde el espacio del MeshFilter al espacio LOCAL de este GameObject (collider)
        Bounds b = TransformBounds(targetMeshFilter.transform, transform, meshLocal);

        _bc.center = b.center;
        _bc.size = b.size;
    }

    private static Bounds TransformBounds(Transform from, Transform to, Bounds b)
    {
        // 8 corners del bounds en el espacio local de 'from'
        Vector3 c = b.center;
        Vector3 e = b.extents;

        Vector3[] corners =
        {
            c + new Vector3(+e.x, +e.y, +e.z),
            c + new Vector3(+e.x, +e.y, -e.z),
            c + new Vector3(+e.x, -e.y, +e.z),
            c + new Vector3(+e.x, -e.y, -e.z),
            c + new Vector3(-e.x, +e.y, +e.z),
            c + new Vector3(-e.x, +e.y, -e.z),
            c + new Vector3(-e.x, -e.y, +e.z),
            c + new Vector3(-e.x, -e.y, -e.z),
        };

        // Transformar corners a mundo, luego a local de 'to'
        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 world = from.TransformPoint(corners[i]);
            Vector3 local = to.InverseTransformPoint(world);
            min = Vector3.Min(min, local);
            max = Vector3.Max(max, local);
        }

        Bounds result = new Bounds();
        result.SetMinMax(min, max);
        return result;
    }
}
