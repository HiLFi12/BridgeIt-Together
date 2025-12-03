using UnityEngine;

/// <summary>
/// Trigger que destruye cualquier objeto que lo toque,
/// mostrando efectos visuales y respetando una lista de tags protegidos.
/// </summary>
[DisallowMultipleComponent]
public class VehicleReturnTrigger : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    [Header("Efecto al destruir objetos")]
    [SerializeField] private GameObject efectoDestruccionPrefab = null;
    [SerializeField] private bool spawnearEfectoAlDestruir = true;

    [Header("Efecto al destruir vehículos (tag 'Vehicle')")]
    [SerializeField] private GameObject efectoDestruccionVehiculoPrefab = null;
    [SerializeField] private bool spawnearEfectoVehiculo = true;

    [Header("Tags protegidos (no se destruyen)")]
    [SerializeField] private string[] protectedTags = new string[]
    {
        "Player",
        "MainCamera",
        "GameController",
        "UI",
        "BridgeQuadrant",
        "Ground",
        "Platform",
        "Respawn",
        "Finish",
        "EditorOnly"
    };

    private Collider triggerCollider;
    private bool isActive = true;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// Inicializa el trigger desde un manager externo (compatibilidad legacy).
    /// Actualmente solo asegura el collider como trigger.
    /// </summary>
    public void Initialize(VehicleReturnTriggerManager triggerManager)
    {
        // El manager ya no es necesario para la destrucción,
        // pero mantenemos este método para no romper referencias existentes.
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// Activa o desactiva este trigger.
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        if (triggerCollider != null)
            triggerCollider.enabled = active;
    }

    public bool IsActive()
    {
        return isActive && triggerCollider != null && triggerCollider.enabled;
    }

    /// <summary>
    /// Compatibilidad legacy: en la nueva lógica todos los objetos no protegidos se destruyen,
    /// así que este flag ya no tiene efecto directo, pero lo conservamos para no romper llamadas.
    /// </summary>
    public void SetDestroyNonVehicles(bool enabled)
    {
        // La lógica actual ya destruye todo lo que no está protegido;
        // podríamos usar este flag para futuros refinamientos si hace falta.
    }

    /// <summary>
    /// Activa o desactiva los mensajes de debug (compatibilidad con configurador).
    /// </summary>
    public void SetDebugMessages(bool enabled)
    {
        showDebugMessages = enabled;
    }

    /// <summary>
    /// Añade tags protegidos adicionales (compatibilidad con configurador).
    /// </summary>
    public void AddProtectedTags(string[] newProtectedTags)
    {
        if (newProtectedTags == null || newProtectedTags.Length == 0) return;

        var all = new System.Collections.Generic.List<string>();
        if (protectedTags != null) all.AddRange(protectedTags);

        foreach (var tag in newProtectedTags)
        {
            if (!string.IsNullOrEmpty(tag) && !all.Contains(tag))
            {
                all.Add(tag);
            }
        }

        protectedTags = all.ToArray();

        if (showDebugMessages)
        {
            Debug.Log($"Tags protegidos actualizados en trigger {gameObject.name}: {string.Join(", ", protectedTags)}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (other == null) return;

        GameObject obj = other.gameObject;

        // No destruir este mismo trigger
        if (obj == gameObject) return;

        if (!DebeDestruir(obj))
        {
            if (showDebugMessages)
                Debug.Log($"⚠️ Objeto protegido, no se destruye: {obj.name} (Tag: {obj.tag})", obj);
            return;
        }

        // Elegir efecto según si es vehículo o no
        if (obj.CompareTag("Vehicle") && spawnearEfectoVehiculo && efectoDestruccionVehiculoPrefab != null)
        {
            SpawnearEfectoDestruccionEspecifico(obj.transform.position, efectoDestruccionVehiculoPrefab, true);
        }
        else
        {
            SpawnearEfectoDestruccion(obj.transform.position);
        }

        if (showDebugMessages)
            Debug.Log($"🗑️ Destruyendo objeto: {obj.name} (Tag: {obj.tag})", obj);

        Destroy(obj);
    }

    private bool DebeDestruir(GameObject obj)
    {
        // Tags protegidos
        if (!string.IsNullOrEmpty(obj.tag) && obj.tag != "Untagged")
        {
            for (int i = 0; i < protectedTags.Length; i++)
            {
                var t = protectedTags[i];
                if (!string.IsNullOrEmpty(t) && obj.CompareTag(t))
                    return false;
            }
        }

        // No destruir sistemas importantes
        if (obj.GetComponent<Camera>() != null ||
            obj.GetComponent<Light>() != null ||
            obj.GetComponent<AudioListener>() != null ||
            obj.GetComponentInParent<Canvas>() != null ||
            obj.GetComponent<VehicleReturnTrigger>() != null)
        {
            return false;
        }

        // No destruir objetos que sean parte del sistema de puentes
        if (obj.GetComponent<BridgeConstructionGrid>() != null ||
            obj.GetComponentInChildren<BridgeConstructionGrid>() != null ||
            obj.GetComponentInParent<BridgeConstructionGrid>() != null)
        {
            return false;
        }

        return true;
    }

    private void SpawnearEfectoDestruccion(Vector3 posicion)
    {
        if (!spawnearEfectoAlDestruir || efectoDestruccionPrefab == null) return;

        GameObject efecto = Instantiate(efectoDestruccionPrefab, posicion, Quaternion.identity);

        if (showDebugMessages)
            Debug.Log($"💥 Efecto de destrucción spawneado en posición: {posicion}", efecto);

        var ps = efecto.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float duracion = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(efecto, duracion);
        }
        else
        {
            Destroy(efecto, 5f);
        }
    }

    private void SpawnearEfectoDestruccionEspecifico(Vector3 posicion, GameObject prefab, bool esVehiculo)
    {
        if (prefab == null) return;

        GameObject efecto = Instantiate(prefab, posicion, Quaternion.identity);

        if (showDebugMessages)
        {
            string tipo = esVehiculo ? "vehículo" : "objeto";
            Debug.Log($"💥 Efecto de destrucción ({tipo}) spawneado en posición: {posicion}", efecto);
        }

        var ps = efecto.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float duracion = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(efecto, duracion);
        }
        else
        {
            Destroy(efecto, 5f);
        }
    }

    private void OnDrawGizmos()
    {
        if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null) return;

        Gizmos.color = isActive ? new Color(1f, 0f, 0f, 0.4f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (triggerCollider is BoxCollider box)
            Gizmos.DrawWireCube(box.center, box.size);
        else if (triggerCollider is SphereCollider sphere)
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
    }

    private void OnDrawGizmosSelected()
    {
        if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null) return;

        Gizmos.color = isActive ? Color.green : Color.gray;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (triggerCollider is BoxCollider box)
            Gizmos.DrawCube(box.center, box.size);
        else if (triggerCollider is SphereCollider sphere)
            Gizmos.DrawSphere(sphere.center, sphere.radius);
    }
}
