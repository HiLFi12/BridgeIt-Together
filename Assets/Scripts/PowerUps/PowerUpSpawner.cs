using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    public List<GameObject> powerUpPrefabs;
    public List<Transform> spawnPoints;
    
    [Header("Intervalos de Spawn Aleatorio")]
    [Tooltip("Tiempo mínimo entre spawns (en segundos)")]
    public float minSpawnInterval = 15f;
    [Tooltip("Tiempo máximo entre spawns (en segundos)")]
    public float maxSpawnInterval = 45f;
    [Tooltip("Tiempo de espera después de que un power-up es activado")]
    public float cooldownAfterDespawn = 8f;
    
    [Header("Configuración Avanzada")]
    [Tooltip("Si está activado, solo spawneará UNA vez por partida")]
    public bool spawnOnlyOnce = true;
    [Tooltip("Permite múltiples power-ups activos simultáneamente (solo si spawnOnlyOnce está desactivado)")]
    public bool allowMultiplePowerUps = false;
    [Tooltip("Máximo número de power-ups activos (solo si allowMultiplePowerUps está activo)")]
    public int maxActivePowerUps = 2;
    
    [Header("Referencias")]
    public BridgeConstructionGrid bridgeGrid; // Referencia directa a la grilla

    private bool canSpawn = true;
    private List<GameObject> activePowerUps = new List<GameObject>();
    private float nextSpawnTime;
    private bool hasSpawnedOnce = false; // Control para spawn único

    private void Start()
    {
        // Si no se ha asignado la referencia, intentar encontrarla automáticamente
        if (bridgeGrid == null)
        {
            bridgeGrid = FindObjectOfType<BridgeConstructionGrid>();
            if (bridgeGrid == null)
            {
                Debug.LogError("¡No se encontró referencia a BridgeConstructionGrid! Los PowerUps no funcionarán correctamente.");
            }
        }
        
        // Configurar el primer tiempo de spawn aleatorio
        SetNextRandomSpawnTime();
        StartCoroutine(SpawnRoutine());
    }

    private void SetNextRandomSpawnTime()
    {
        float randomInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        nextSpawnTime = Time.time + randomInterval;
        Debug.Log($"Próximo PowerUp spawneará en {randomInterval:F1} segundos");
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Si solo puede spawnear una vez y ya lo hizo, no hacer nada más
            if (spawnOnlyOnce && hasSpawnedOnce)
            {
                yield return new WaitForSeconds(1f); // Verificar cada segundo
                continue;
            }

            if (canSpawn && Time.time >= nextSpawnTime)
            {
                bool canSpawnNew = false;
                
                if (spawnOnlyOnce)
                {
                    // Si es spawn único, solo puede spawnear si no ha spawneado antes
                    canSpawnNew = !hasSpawnedOnce;
                }
                else
                {
                    // Lógica original para múltiples spawns
                    canSpawnNew = allowMultiplePowerUps ? 
                        activePowerUps.Count < maxActivePowerUps : 
                        activePowerUps.Count == 0;
                }

                if (canSpawnNew)
                {
                    SpawnPowerUp();
                    
                    if (spawnOnlyOnce)
                    {
                        hasSpawnedOnce = true;
                        Debug.Log("PowerUp único spawneado. No habrá más spawns en esta partida.");
                    }
                    else
                    {
                        SetNextRandomSpawnTime(); // Solo configurar siguiente tiempo si no es spawn único
                    }
                }
            }
            
            // Limpiar power-ups que han sido destruidos
            activePowerUps.RemoveAll(powerUp => powerUp == null);
            
            yield return new WaitForSeconds(0.5f); // Verificar cada medio segundo
        }
    }

    private void SpawnPowerUp()
    {
        if (powerUpPrefabs.Count == 0 || spawnPoints.Count == 0) return;
        
        int prefabIndex = Random.Range(0, powerUpPrefabs.Count);
        int pointIndex = Random.Range(0, spawnPoints.Count);
        
        Debug.Log($"Spawneando PowerUp: {powerUpPrefabs[prefabIndex].name} en {spawnPoints[pointIndex].name}");
        GameObject newPowerUp = Instantiate(powerUpPrefabs[prefabIndex], spawnPoints[pointIndex].position, Quaternion.identity);
        
        // Añadir a la lista de power-ups activos
        activePowerUps.Add(newPowerUp);

        // --- INICIO: Asignación de referencias para PowerUps ---
        // PowerUpRitualGranFuego
        var ritual = newPowerUp.GetComponent<PowerUpRitualGranFuego>();
        if (ritual != null)
        {
            // Asignar la referencia a BridgeConstructionGrid
            ritual.bridgeGrid = bridgeGrid;
            
            // NOTA: Las antorchas del tótem ahora se configuran directamente en el prefab
            // del PowerUp asignando los colliders desde el inspector. Ya no necesitamos
            // buscar antorchas por tags en la escena.
        }
        
        // PowerUpConstructorHolografico
        var constructor = newPowerUp.GetComponent<PowerUpConstructorHolografico>();
        if (constructor != null)
        {
            // Asignar la referencia a TechUpgradeManager si existe
            constructor.techUpgradeManager = FindObjectOfType<TechUpgradeManager>();
        }
        
        // PowerUpCalorHumano
        var calor = newPowerUp.GetComponent<PowerUpCalorHumano>();
        if (calor != null)
        {
            // Si tiene alguna referencia específica, asignarla aquí
        }
        // --- FIN: Asignación de referencias para PowerUps ---

        PowerUpBase powerUp = newPowerUp.GetComponent<PowerUpBase>();
        if (powerUp != null)
        {
            PowerUpBase.OnPowerUpActivated += OnPowerUpActivated;
        }
    }

    private void OnPowerUpActivated(PowerUpBase powerUp)
    {
        PowerUpBase.OnPowerUpActivated -= OnPowerUpActivated;
        
        // Remover el power-up de la lista de activos
        if (powerUp != null && powerUp.gameObject != null)
        {
            activePowerUps.Remove(powerUp.gameObject);
        }
        
        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        canSpawn = false;
        yield return new WaitForSeconds(cooldownAfterDespawn);
        canSpawn = true;
    }

    /// <summary>
    /// Resetea el estado del spawner para permitir un nuevo spawn único.
    /// Útil para reiniciar partidas o testing.
    /// </summary>
    public void ResetSpawner()
    {
        hasSpawnedOnce = false;
        canSpawn = true;
        
        // Destruir todos los power-ups activos
        foreach (GameObject powerUp in activePowerUps)
        {
            if (powerUp != null)
            {
                Destroy(powerUp);
            }
        }
        activePowerUps.Clear();
        
        // Configurar nuevo tiempo de spawn
        SetNextRandomSpawnTime();
        
        Debug.Log("PowerUpSpawner reseteado. Listo para nuevo spawn.");
    }
} 