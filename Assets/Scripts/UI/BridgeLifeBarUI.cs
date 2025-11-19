using UnityEngine;
using UnityEngine.UI;

public class BridgeLifeBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Imagen de la barra (Image.type = Filled, Fill Method = Vertical).")]
    [SerializeField] private Image lifeBarImage;

    [Tooltip("ScriptableObject del cuadrante cuya vida queremos mostrar.")]
    [SerializeField] private BridgeQuadrantSO quadrantSO;

    [Header("Colores por vida (umbrales serializables)")]
    [Tooltip("Color cuando la vida está entre vidaAltaMin y 100%.")]
    [SerializeField] private Color highLifeColor = Color.green;

    [Tooltip("Color cuando la vida está entre vidaMediaMin y vidaAltaMin.")]
    [SerializeField] private Color mediumLifeColor = Color.yellow;

    [Tooltip("Color cuando la vida está entre vidaBajaMin y vidaMediaMin.")]
    [SerializeField] private Color lowLifeColor = Color.red;

    [Header("Umbrales (0..1)")]
    [Tooltip("Mínimo ratio para vida alta (ej. 0.51 = 51%).")]
    [Range(0f, 1f)]
    [SerializeField] private float highLifeMin = 0.51f;

    [Tooltip("Mínimo ratio para vida media (ej. 0.26 = 26%). Por debajo de esto es baja.")]
    [Range(0f, 1f)]
    [SerializeField] private float mediumLifeMin = 0.26f;

    private void Awake()
    {
        if (lifeBarImage == null)
        {
            lifeBarImage = GetComponentInChildren<Image>();
        }
    }

    private void Update()
    {
        if (lifeBarImage == null || quadrantSO == null)
            return;

        float ratio = quadrantSO.GetLifeRatio(); // 0..1

        // Actualizar relleno (barra vertical)
        lifeBarImage.fillAmount = ratio;

        // Actualizar color según umbrales
        lifeBarImage.color = GetColorForRatio(ratio);
    }

    private Color GetColorForRatio(float ratio)
    {
        if (ratio >= highLifeMin)
            return highLifeColor;
        if (ratio >= mediumLifeMin)
            return mediumLifeColor;
        return lowLifeColor;
    }

    // Por si quieres asignar el SO desde código (al cargar la era, etc.)
    public void SetQuadrantSO(BridgeQuadrantSO so)
    {
        quadrantSO = so;
    }
}