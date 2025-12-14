using UnityEngine;
using BridgeItTogether.Gameplay.AutoControllers;

/// <summary>
/// Trigger dedicado para contar únicamente vehículos que "pasaron".
/// No destruye ni devuelve al pool: solo incrementa el progreso de victoria.
/// 
/// Importante: expone RemoverVehiculoContado para que VehiclePool/VehicleReturnTriggerManager
/// puedan limpiar el cache cuando un vehículo vuelve al pool.
/// </summary>
[RequireComponent(typeof(Collider))]
public class VictoryCountOnlyTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string tagVehiculoRequerido = "Vehicle";
    [SerializeField] private bool mostrarDebugInfo = true;

    [Header("Estado")]
    [SerializeField] private bool triggerActivo = true;

    private GameConditionManager gameManager;
    private Collider triggerCollider;
    private readonly System.Collections.Generic.HashSet<GameObject> vehiculosYaContados = new System.Collections.Generic.HashSet<GameObject>();

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        gameManager = GameConditionManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameConditionManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError($"No se encontró GameConditionManager para el trigger {gameObject.name}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerActivo || gameManager == null) return;
        if (!gameManager.IsJuegoActivo()) return;
        if (other == null) return;

        GameObject vehiculoFinal = BuscarVehiculo(other.gameObject);
        if (vehiculoFinal == null) return;

        if (vehiculosYaContados.Contains(vehiculoFinal)) return;
        vehiculosYaContados.Add(vehiculoFinal);

        gameManager.OnVehiculoPasaPuente(vehiculoFinal);

        if (mostrarDebugInfo)
        {
            Debug.Log($"✅ VictoryCountOnlyTrigger: Vehículo contado: {vehiculoFinal.name} (Trigger: {gameObject.name})");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject vehiculoFinal = BuscarVehiculo(other != null ? other.gameObject : null);
        if (vehiculoFinal != null)
        {
            vehiculosYaContados.Remove(vehiculoFinal);
        }
    }

    private GameObject BuscarVehiculo(GameObject obj)
    {
        if (obj == null) return null;

        if (EsVehiculo(obj)) return obj;

        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            if (EsVehiculo(parent.gameObject)) return parent.gameObject;
            parent = parent.parent;
        }

        AutoMovement movement = obj.GetComponentInChildren<AutoMovement>();
        if (movement != null && EsVehiculo(movement.gameObject)) return movement.gameObject;

        VehicleBridgeCollision bridgeCollision = obj.GetComponentInChildren<VehicleBridgeCollision>();
        if (bridgeCollision != null && EsVehiculo(bridgeCollision.gameObject)) return bridgeCollision.gameObject;

        AutoController autoControllerChild = obj.GetComponentInChildren<AutoController>();
        if (autoControllerChild != null && EsVehiculo(autoControllerChild.gameObject)) return autoControllerChild.gameObject;

        return null;
    }

    private bool EsVehiculo(GameObject obj)
    {
        if (obj == null) return false;
        if (!obj.CompareTag(tagVehiculoRequerido)) return false;

        bool tieneAutoMovement = obj.GetComponent<AutoMovement>() != null;
        bool tieneVehicleBridgeCollision = obj.GetComponent<VehicleBridgeCollision>() != null;
        bool tieneAutoController = obj.GetComponent<AutoController>() != null;
        return tieneAutoMovement || tieneVehicleBridgeCollision || tieneAutoController;
    }

    public void SetTriggerActivo(bool activo) => triggerActivo = activo;
    public bool IsTriggerActivo() => triggerActivo;

    /// <summary>
    /// Limpieza puntual para permitir re-conteo cuando el vehículo vuelva del pool.
    /// </summary>
    public void RemoverVehiculoContado(GameObject vehiculo)
    {
        if (vehiculo == null) return;
        vehiculosYaContados.Remove(vehiculo);
    }
}
