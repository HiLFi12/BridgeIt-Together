using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool de objetos para los autos. Permite reutilizar instancias en lugar de crear y destruir constantemente.
/// </summary>
public class AutoPool : MonoBehaviour
{
    [SerializeField] private GameObject autoPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private bool expandible = true;
    
    private List<GameObject> pool;
    private static AutoPool instance;
    
    /// <summary>
    /// Singleton para acceder fácilmente al pool desde cualquier script
    /// </summary>
    public static AutoPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AutoPool>();
                
                if (instance == null)
                {
                    GameObject obj = new GameObject("AutoPool");
                    instance = obj.AddComponent<AutoPool>();
                }
            }
            
            return instance;
        }
    }
    
    private void Awake()
    {
        // Singleton setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Inicializar el pool
        InitializePool();
    }
    
    /// <summary>
    /// Inicializa el pool con el tamaño especificado
    /// </summary>
    private void InitializePool()
    {
        pool = new List<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewAutoInstance();
        }
    }
    
    /// <summary>
    /// Crea una nueva instancia del auto y la agrega al pool
    /// </summary>
    private GameObject CreateNewAutoInstance()
    {
        GameObject newAuto = Instantiate(autoPrefab, transform);
        newAuto.SetActive(false);
        
        // Asegurarse de que tenga todos los componentes necesarios
        if (newAuto.GetComponent<AutoMovement>() == null)
        {
            newAuto.AddComponent<AutoMovement>();
        }
        
        if (newAuto.GetComponent<VehicleBridgeCollision>() == null)
        {
            newAuto.AddComponent<VehicleBridgeCollision>();
        }
        
        if (newAuto.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = newAuto.AddComponent<Rigidbody>();
            rb.mass = 1000;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        pool.Add(newAuto);
        return newAuto;
    }
    
    /// <summary>
    /// Obtiene un auto del pool o crea uno nuevo si es necesario
    /// </summary>
    public GameObject GetAuto()
    {
        // Buscar un auto inactivo en el pool
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }
        
        // Si no hay autos disponibles y el pool es expandible, crear uno nuevo
        if (expandible)
        {
            Debug.Log("Pool de autos expandido automáticamente");
            return CreateNewAutoInstance();
        }
        
        // Si no hay autos disponibles y el pool no es expandible, retornar null
        Debug.LogWarning("No hay autos disponibles en el pool y no está configurado como expandible");
        return null;
    }
    
    /// <summary>
    /// Devuelve un auto al pool (lo desactiva)
    /// </summary>
    public void ReturnAuto(GameObject auto)
    {
        auto.SetActive(false);
        
        // Resetear propiedades del auto si es necesario
        Rigidbody rb = auto.GetComponent<Rigidbody>();
        if (rb != null)
        {
        rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    /// <summary>
    /// Configura el prefab del auto si no fue asignado en el Inspector
    /// </summary>
    public void SetAutoPrefab(GameObject prefab)
    {
        if (autoPrefab == null)
        {
            autoPrefab = prefab;
            
            // Si el pool ya está inicializado, no hacer nada más
            if (pool == null || pool.Count == 0)
            {
                InitializePool();
            }
        }
        else
        {
            Debug.LogWarning("El prefab del auto ya está configurado. No se ha cambiado.");
        }
    }
}
