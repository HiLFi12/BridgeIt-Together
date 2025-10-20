using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Versión simplificada rehacida desde cero: reutiliza un único objeto y lista de spawn points.
/// Llama a <see cref="SpawnRandom"/> para activarlo en un punto aleatorio.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    [Header("Objeto a activar")]
    [FormerlySerializedAs("prefab")]
    [Tooltip("Referencia a un GameObject existente (desactivado) que será reposicionado y activado una única vez.")]
    public GameObject pooledObject;

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
    /// Activa el objeto reutilizable en un punto aleatorio de la lista.
    /// </summary>
    public void SpawnRandom()
    {
        if (hasSpawned)
            return; // Ya se generó el único spawn

        if (pooledObject == null || spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("[PowerUpSpawner] Objeto a activar o spawnPoints no asignados.");
            return;
        }

        if (pooledObject.activeInHierarchy)
        {
            Debug.LogWarning("[PowerUpSpawner] El objeto configurado ya está activo, no se puede reutilizar para el spawn único.");
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
        // Combinar la rotación del spawn point con la del objeto para respetar la orientación propia
        Quaternion finalRot = point.rotation * pooledObject.transform.rotation;
        Vector3 targetPosition = new Vector3(point.position.x, point.position.y, point.position.z);

        pooledObject.transform.SetPositionAndRotation(targetPosition, finalRot);
        pooledObject.SetActive(true);

        // Instanciar efecto de aparición si existe
        if (spawnEffectPrefab != null)
        {
            Vector3 fxPos = point.TransformPoint(effectOffset);
            GameObject fx = Instantiate(spawnEffectPrefab, fxPos, finalRot);
            if (attachEffectAsChild && pooledObject != null)
            {
                fx.transform.SetParent(pooledObject.transform, true);
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