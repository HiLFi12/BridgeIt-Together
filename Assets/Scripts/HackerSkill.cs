using UnityEngine;

public class HackerSkill : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private Vector3 initialScale = Vector3.one;
    [SerializeField] private Vector3 maxScale = Vector3.one * 5f;
    [SerializeField] private float scaleSpeed = 2f;

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 2f;

    [Header("Material Color Settings")]
    [SerializeField] private Material hologramMaterial;
    [SerializeField] private float colorChangeInterval = 0.5f;
    [SerializeField] private bool randomizeColors = true;

    private Vector3 currentScale;
    private float currentCooldown;
    private bool isGrowing = true;
    private bool isInCooldown = false;

    private Material materialInstance;
    private float colorChangeTimer;

    // Nombres de las propiedades de color del shader
    private static readonly int Color1Property = Shader.PropertyToID("Color_5A29FF9E");
    private static readonly int Color2Property = Shader.PropertyToID("Color_9DC7EC12");
    private static readonly int Color3Property = Shader.PropertyToID("Color_BB588D2D");

    private void Start()
    {
        currentScale = initialScale;
        transform.localScale = currentScale;
        currentCooldown = 0f;
        colorChangeTimer = 0f;

        // Crear instancia del material si está asignado
        if (hologramMaterial != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                materialInstance = new Material(hologramMaterial);
                renderer.material = materialInstance;
            }
        }
    }

    private void Update()
    {
        UpdateScale();
        UpdateMaterialColors();
    }

    private void UpdateScale()
    {
        if (isInCooldown)
        {
            // Esperar en el tamaño máximo
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0f)
            {
                currentCooldown = 0f;
                isInCooldown = false;
                isGrowing = false;
            }
            return;
        }

        if (isGrowing)
        {
            // Crecer hacia maxScale
            currentScale = Vector3.MoveTowards(currentScale, maxScale, scaleSpeed * Time.deltaTime);
            
            if (currentScale == maxScale)
            {
                isInCooldown = true;
                currentCooldown = cooldownTime;
            }
        }
        else
        {
            // Achicarse hacia initialScale
            currentScale = Vector3.MoveTowards(currentScale, initialScale, scaleSpeed * Time.deltaTime);
            
            if (currentScale == initialScale)
            {
                Destroy(gameObject);
            }
        }

        transform.localScale = currentScale;
    }

    private void UpdateMaterialColors()
    {
        if (!randomizeColors || materialInstance == null) return;

        colorChangeTimer -= Time.deltaTime;
        
        if (colorChangeTimer <= 0f)
        {
            colorChangeTimer = colorChangeInterval;
            ChangeToRandomColors();
        }
    }

    private void ChangeToRandomColors()
    {
        // Generar colores aleatorios brillantes para el efecto holográfico
        Color randomColor1 = new Color(Random.value, Random.value, Random.value, 1f);
        Color randomColor2 = new Color(Random.value, Random.value, Random.value, 1f);
        
        // Color3 con valores HDR más altos para efecto de brillo
        Color randomColor3 = new Color(
            Random.Range(0f, 140f),
            Random.Range(0f, 140f),
            Random.Range(0f, 140f),
            1f
        );

        materialInstance.SetColor(Color1Property, randomColor1);
        materialInstance.SetColor(Color2Property, randomColor2);
        materialInstance.SetColor(Color3Property, randomColor3);
    }

    private void OnDestroy()
    {
        // Limpiar la instancia del material
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
