using UnityEngine;

public class WheelRotation : MonoBehaviour
{
    [Header("Ruedas a rotar")]
    [SerializeField] private GameObject[] ruedas;
    [Header("Velocidad de rotación (grados/segundo)")]
    [SerializeField] private float velocidadRotacion = 360f;
    [Header("Eje de rotación")]
    [SerializeField] private Vector3 ejeRotacion = Vector3.right;

    private void Update()
    {
        if (ruedas == null || ruedas.Length == 0) return;
        float rotacion = velocidadRotacion * Time.deltaTime;
        foreach (var rueda in ruedas)
        {
            if (rueda != null)
            {
                rueda.transform.Rotate(ejeRotacion * rotacion, Space.Self);
            }
        }
    }
}

