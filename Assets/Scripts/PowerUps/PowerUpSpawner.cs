using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Versión simplificada rehacida desde cero: un solo prefab y lista de spawn points.
/// Llama a <see cref="SpawnRandom"/> para instanciarlo en un punto aleatorio.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    [Header("Prefab a instanciar")]
    [Tooltip("PowerUp (o cualquier GameObject) que será instanciado una única vez.")]
    public GameObject prefab;

    [Header("Puntos de spawn (Empty Transforms en escena)")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawn Automático Único")] 
    [Tooltip("Si está activo, hará un único spawn automático tras un retardo aleatorio.")] public bool autoSpawn = true;
    [Tooltip("Intervalo mínimo antes de spawnear (segundos)")] public float minSpawnDelay = 5f;
    [Tooltip("Intervalo máximo antes de spawnear (segundos)")] public float maxSpawnDelay = 15f;

    [Header("Efecto de Aparición")]
    [Tooltip("Prefab de efecto (partícula / VFX / sonido) a instanciar cuando aparece el power-up.")]
    public GameObject spawnEffectPrefab;
    [Tooltip("Si true, el efecto se parenta al objeto spawneado.")] public bool attachEffectAsChild = true;
    [Tooltip("Offset local aplicado a la posición de spawn para el efecto.")] public Vector3 effectOffset = Vector3.zero;

    private bool hasSpawned = false;

    /// <summary>
    /// Instancia el prefab en un punto aleatorio de la lista.
    /// </summary>
    public void SpawnRandom()
    {
        if (hasSpawned)
            return; // Ya se generó el único spawn

        if (prefab == null || spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("[PowerUpSpawner] Prefab o spawnPoints no asignados.");
            return;
        }

    Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
    // Combinar la rotación del spawn point con la del prefab para respetar la orientación propia
    Quaternion finalRot = point.rotation * prefab.transform.rotation;
    GameObject spawned = Instantiate(prefab, point.position, finalRot);

        // Instanciar efecto de aparición si existe
        if (spawnEffectPrefab != null)
        {
            Vector3 fxPos = point.TransformPoint(effectOffset);
            GameObject fx = Instantiate(spawnEffectPrefab, fxPos, finalRot);
            if (attachEffectAsChild && spawned != null)
            {
                fx.transform.SetParent(spawned.transform, true);
            }

            // Auto-destroy si tiene ParticleSystem principal
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float ttl = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(fx, ttl);
            }
        }
        hasSpawned = true;
    }

    private void Start()
    {
        if (autoSpawn && !hasSpawned)
            StartCoroutine(SpawnOnceDelayed());
    }

    private IEnumerator SpawnOnceDelayed()
    {
        if (maxSpawnDelay < minSpawnDelay)
            maxSpawnDelay = minSpawnDelay;
        float wait = Random.Range(minSpawnDelay, maxSpawnDelay);
        yield return new WaitForSeconds(wait);
        SpawnRandom();
    }
}