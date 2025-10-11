using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Altura máxima de flotación")]
    [SerializeField] private float amplitude = 0.5f;
    [Header("Velocidad de flotación")]
    [SerializeField] private float frequency = 1f;
    [Header("Desfase inicial")]
    [SerializeField] private float phaseOffset = 0f;
    [Header("Suavizado del movimiento")]
    [SerializeField] private float smooth = 5f;

    private Vector3 initialPosition;
    private float timeOffset;

    void Start()
    {
        initialPosition = transform.localPosition;
        timeOffset = Random.Range(0f, 100f) + phaseOffset;
    }

    void Update()
    {
        float targetY = initialPosition.y + Mathf.Sin((Time.time + timeOffset) * frequency) * amplitude;
        Vector3 targetPosition = new Vector3(initialPosition.x, targetY, initialPosition.z);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smooth);
    }
}

