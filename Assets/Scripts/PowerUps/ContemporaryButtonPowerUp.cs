using UnityEngine;

public class ContemporaryButtonPowerUp : MonoBehaviour
{
    [Header("Objeto visual a cambiar de color")]
    [SerializeField] private Renderer objetoVisual;
    [Header("Estado del botón")]
    public bool isPressed = false;

    private void Start()
    {
        SetColor(Color.red);
    }

    private void OnTriggerEnter(Collider other)
    {
        isPressed = true;
        SetColor(Color.green);
    }

    private void OnTriggerExit(Collider other)
    {
        isPressed = false;
        SetColor(Color.red);
    }

    private void SetColor(Color color)
    {
        if (objetoVisual != null)
        {
            if (objetoVisual.material != null)
                objetoVisual.material.color = color;
        }
    }
}

