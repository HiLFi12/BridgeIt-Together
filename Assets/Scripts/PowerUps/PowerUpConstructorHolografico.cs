using System.Collections;
using UnityEngine;

public class PowerUpConstructorHolografico : PowerUpBase
{
    [Header("Referencias de baterías")]
    [SerializeField] private BatterySystem[] batterySystems; // Array de referencias a BatterySystem

    [Header("Configuración")]
    [SerializeField] private float activationDelay = 2f; // Segundos de espera tras cargar todas las baterías
    [SerializeField] private BridgeConstructionGrid bridgeGrid; // Referencia al sistema de puentes

    [Header("Visuales de baterías")]
    [SerializeField] private GameObject[] batteryVisuals; // Visuales para cada batería

    private bool isActivating = false;
    private float activationTimer = 0f;

    protected override void Start()
    {
        base.Start();
        // Opcional: buscar BridgeConstructionGrid si no está asignado
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
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
