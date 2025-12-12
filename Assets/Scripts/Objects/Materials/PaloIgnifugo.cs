using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class PaloIgnifugo : MonoBehaviour, IHitable, IUIActivatable
{
    [Header("Configuración")]
    [SerializeField] private GameObject efectoFuego;
    [SerializeField] private Transform fuegoSpawnPoint;
    [SerializeField] private float tiempoEncendido = 9999f;

    [Header("Ignite FX")]
    [Tooltip("Prefab de efecto/shader que se instancia al encender la antorcha.")]
    [SerializeField] private GameObject igniteEffectPrefab;
    [Tooltip("Tiempo en segundos antes de destruir el efecto instanciado. 0 o negativo = no destruir automáticamente.")]
    [SerializeField] private float igniteEffectLifetime = 2f;

    [Header("UI Configuration")]
    [SerializeField] private int turnedOffIndex = 3;
    [SerializeField] private int turnedOnIndex = 4;

    [Header("Audio")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir al encender la antorcha. -1 desactiva.")]
    [SerializeField] private int turnOnSfxIndex = -1;
    [Tooltip("Si está activo, el SFX se mantiene en loop mientras la antorcha esté encendida.")]
    [SerializeField] private bool loopSfxWhileOn = true;

    private bool estaEncendido = false;
    private float tiempoRestante = 0f;

    private PlayerUIManager playerUIManager;
    private AudioManager audioManager;
    private AudioSource sfxSource;

    public int UIIndex { get; private set; }

    private void Start()
    {
        playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        audioManager = FindFirstObjectByType<AudioManager>();
        if (turnOnSfxIndex >= 0)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = loopSfxWhileOn;
        }
        // Recolocar el efecto de fuego en el spawn point si está asignado
        if (efectoFuego != null && fuegoSpawnPoint != null)
        {
            efectoFuego.transform.SetParent(fuegoSpawnPoint, false);
            efectoFuego.transform.localPosition = Vector3.zero;
            efectoFuego.transform.localRotation = Quaternion.identity;
        }
        SetEncendido(false);
    }

    private void Update()
    {
        if (!estaEncendido) return;

        if (tiempoRestante > 0f)
        {
            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante <= 0f)
            {
                // La antorcha se consume y se apaga (apaga fuego + sonido asociado al efecto)
                SetEncendido(false);
            }
        }
    }

    public void SetEncendido(bool encendido)
    {
        bool estabaEncendido = estaEncendido;
        estaEncendido = encendido;
        if (efectoFuego != null)
        {
            efectoFuego.SetActive(encendido);
        }

        // Efecto visual de encendido (solo cuando pasa de apagado a encendido)
        if (encendido && !estabaEncendido)
        {
            SpawnIgniteEffect();
        }
        if (sfxSource != null && turnOnSfxIndex >= 0)
        {
            if (encendido)
            {
                if (audioManager == null)
                    audioManager = FindFirstObjectByType<AudioManager>();

                AudioClip clip = null;
                if (audioManager != null && turnOnSfxIndex < audioManager.soundEffects.Count)
                {
                    clip = audioManager.soundEffects[turnOnSfxIndex];
                }

                if (clip != null)
                {
                    sfxSource.clip = clip;
                    sfxSource.loop = loopSfxWhileOn;
                    if (!sfxSource.isPlaying)
                        sfxSource.Play();
                }
            }
            else
            {
                if (sfxSource.isPlaying)
                    sfxSource.Stop();
            }
        }
        tiempoRestante = encendido ? tiempoEncendido : 0f;
        int index = encendido ? turnedOnIndex : turnedOffIndex;
        SetUIIndex(index);
        if (playerUIManager != null)
        {
            playerUIManager.RefreshHeldObjectUI(index);
        }
    }

    private void SpawnIgniteEffect()
    {
        if (igniteEffectPrefab == null)
            return;

        Transform spawn = fuegoSpawnPoint != null ? fuegoSpawnPoint : transform;
        GameObject instance = Instantiate(igniteEffectPrefab, spawn.position, spawn.rotation);
        instance.transform.SetParent(spawn, true);

        if (igniteEffectLifetime > 0f)
        {
            Destroy(instance, igniteEffectLifetime);
        }
    }

    public void SetUIIndex(int index)
    {
        if (playerUIManager != null)
        {
            playerUIManager.RefreshHeldObjectUI(index);
        }
        UIIndex = index;
    }

    public bool EstaEncendido()
    {
        return estaEncendido;
    }

    public void OnLaunched(Vector3 targetPosition)
    {
    }
}