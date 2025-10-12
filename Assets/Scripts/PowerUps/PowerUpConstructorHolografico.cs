using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BatteryVisualEmission
{
    public List<Renderer> renderers;
}

public class PowerUpConstructorHolografico : PowerUpBase
{
    [Header("Referencias de baterías")]
    [SerializeField] private BatterySystem[] batterySystems; // Array de referencias a BatterySystem

    [Header("Configuración")]
    [SerializeField] private float activationDelay = 2f; // Segundos de espera tras cargar todas las baterías
    [SerializeField] private BridgeConstructionGrid bridgeGrid; // Referencia al sistema de puentes

    [Header("Visuales de baterías")]
    [SerializeField] private GameObject[] batteryVisuals; // Visuales para cada batería
    
    [Header("Emisión de visuales de baterías")]
    [SerializeField] private List<BatteryVisualEmission> batteryEmissionVisuals; // Visuales con renderers para cada batería
    [SerializeField] private Color colorEmisionCargado = Color.green;
    [SerializeField] private Color colorEmisionDescargado = Color.red;
    [SerializeField] private float intensidadEmisionCargado = 2f;
    [SerializeField] private float intensidadEmisionDescargado = 0.5f;
    [SerializeField] private float velocidadCambioColor = 2f;

    private bool isActivating = false;
    private float activationTimer = 0f;
    private Material[][] materialesInstanciados; // Materiales instanciados por visual
    private Color[][] coloresEmisionActuales; // Color actual por material
    private float[] intensidadesEmisionActuales; // Intensidad actual por visual

    protected override void Start()
    {
        base.Start();
        // Opcional: buscar BridgeConstructionGrid si no está asignado
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
        }
    }

    private void Awake()
    {
        int n = batteryEmissionVisuals != null ? batteryEmissionVisuals.Count : 0;
        materialesInstanciados = new Material[n][];
        coloresEmisionActuales = new Color[n][];
        intensidadesEmisionActuales = new float[n];
        for (int i = 0; i < n; i++)
        {
            var visual = batteryEmissionVisuals[i];
            List<Material> mats = new List<Material>();
            if (visual != null && visual.renderers != null)
            {
                foreach (var rend in visual.renderers)
                {
                    if (rend != null)
                    {
                        var origMats = rend.materials;
                        Material[] instMats = new Material[origMats.Length];
                        for (int j = 0; j < origMats.Length; j++)
                        {
                            instMats[j] = new Material(origMats[j]);
                        }
                        rend.materials = instMats;
                        mats.AddRange(instMats);
                    }
                }
            }
            materialesInstanciados[i] = mats.ToArray();
            coloresEmisionActuales[i] = new Color[mats.Count];
            for (int j = 0; j < mats.Count; j++)
            {
                if (mats[j] != null && mats[j].HasProperty("_EmissionColor"))
                    coloresEmisionActuales[i][j] = mats[j].GetColor("_EmissionColor");
                else
                    coloresEmisionActuales[i][j] = Color.black;
            }
            intensidadesEmisionActuales[i] = intensidadEmisionDescargado;
        }
    }

    private void Update()
    {
        // Visuales: activar/desactivar según estado de cada batería
        if (batterySystems != null && batteryVisuals != null)
        {
            int count = Mathf.Min(batterySystems.Length, batteryVisuals.Length);
            for (int i = 0; i < count; i++)
            {
                if (batteryVisuals[i] != null && batterySystems[i] != null)
                {
                    batteryVisuals[i].SetActive(batterySystems[i].IsCharged);
                }
            }
        }
        // Emisión: cambiar color/intensidad según estado de cada batería, usando materiales instanciados
        if (batterySystems != null && materialesInstanciados != null)
        {
            int count = Mathf.Min(batterySystems.Length, materialesInstanciados.Length);
            for (int i = 0; i < count; i++)
            {
                bool isCharged = batterySystems[i] != null && batterySystems[i].IsCharged;
                Color objetivo = isCharged ? colorEmisionCargado : colorEmisionDescargado;
                float intensidadObjetivo = isCharged ? intensidadEmisionCargado : intensidadEmisionDescargado;
                intensidadesEmisionActuales[i] = Mathf.Lerp(intensidadesEmisionActuales[i], intensidadObjetivo, Time.deltaTime * velocidadCambioColor);
                for (int j = 0; j < materialesInstanciados[i].Length; j++)
                {
                    coloresEmisionActuales[i][j] = Color.Lerp(coloresEmisionActuales[i][j], objetivo, Time.deltaTime * velocidadCambioColor);
                    var mat = materialesInstanciados[i][j];
                    if (mat != null && mat.HasProperty("_EmissionColor"))
                    {
                        mat.SetColor("_EmissionColor", coloresEmisionActuales[i][j] * intensidadesEmisionActuales[i]);
                        if (coloresEmisionActuales[i][j].maxColorComponent > 0.01f)
                            mat.EnableKeyword("_EMISSION");
                        else
                            mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        if (!isAvailable || isActivating) return;
        if (batterySystems == null || batterySystems.Length == 0) return;

        // Verificar si todas las baterías están cargadas
        bool allCharged = true;
        foreach (var battery in batterySystems)
        {
            if (battery == null || !battery.IsCharged)
            {
                allCharged = false;
                break;
            }
        }

        if (allCharged)
        {
            // Iniciar cuenta regresiva para activar el powerup
            isActivating = true;
            activationTimer = activationDelay;
        }
    }

    private void LateUpdate()
    {
        if (!isActivating) return;
        activationTimer -= Time.deltaTime;
        if (activationTimer <= 0f)
        {
            isActivating = false;
            TryActivate(null); // Activar el powerup
        }
    }

    protected override IEnumerator EffectCoroutine(GameObject activator)
    {
        // Descargar todas las baterías
        foreach (var battery in batterySystems)
        {
            if (battery != null)
            {
                battery.ForzarDescarga();
            }
        }

        // Construir automáticamente todos los cuadrantes hasta la capa 3
        if (bridgeGrid != null)
        {
            ConstructBridgeAutomatically();
            yield return new WaitForSeconds(duration);
        }
        else
        {
            Debug.LogError("PowerUpConstructorHolografico: BridgeConstructionGrid no está asignado.");
            yield return new WaitForSeconds(1f);
        }

        Despawn();
    }

    private void ConstructBridgeAutomatically()
    {
        if (bridgeGrid == null) return;
        for (int x = 0; x < bridgeGrid.gridWidth; x++)
        {
            for (int z = 0; z < bridgeGrid.gridLength; z++)
            {
                for (int layerIndex = 0; layerIndex <= 2; layerIndex++)
                {
                    bridgeGrid.TryBuildLayer(x, z, layerIndex, null);
                }
            }
        }
    }
}
