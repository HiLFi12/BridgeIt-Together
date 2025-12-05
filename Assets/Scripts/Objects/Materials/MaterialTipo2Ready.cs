using UnityEngine;

public class MaterialTipo2Ready : MaterialTipo2Base, IHitable
{
    [Header("Referencias de mallas")]
    [SerializeField] protected GameObject notReadyMesh;
    [SerializeField] protected GameObject readyMesh;

    [Header("Estado (heredado)")]
    [SerializeField, Tooltip("Inicializa el material como listo. Si se desactiva, requiere flecha.")] private bool startReady = false;
    public override bool PuedeConstruirse => base.PuedeConstruirse; // la base ya combina isReady

    [Header("UI Configuration")]
    [SerializeField] private int notReadyUIIndex = -1;
    [SerializeField] private int readyUIIndex = 1;

    [Header("Audio - Activación (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir cuando pasa a estado 'ready'. -1 desactiva.")]
    [SerializeField] private int readyActivateSfxIndex = -1;
    
    // Override UIIndex para devolver el índice correcto según el estado de isReady
    public override int UIIndex => isReady ? readyUIIndex : notReadyUIIndex;

    protected override void Awake()
    {
        base.Awake();
        // Activar gating en la base y setear estado inicial
        useReadyState = true;
        isReady = startReady; // usar campo heredado
        AutoVincularMeshesSiFaltan();
        AplicarEstadoVisual();
    }


    protected override void PostEnsure()
    {
        base.PostEnsure();
        if (!isReady) puedeConstruirse = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            AutoVincularMeshesSiFaltan();
            AplicarEstadoVisual();
        }
    }
#endif

    protected void AutoVincularMeshesSiFaltan()
    {
        if (notReadyMesh && readyMesh) return;

        foreach (Transform child in transform)
        {
            string n = child.name.ToLower();
            if (!notReadyMesh && n.Contains("notready"))
                notReadyMesh = child.gameObject;
            else if (!readyMesh && n.Contains("ready"))
                readyMesh = child.gameObject;
        }
    }

    protected virtual void AplicarEstadoVisual()
    {
    if (notReadyMesh) notReadyMesh.SetActive(!isReady);
    if (readyMesh) readyMesh.SetActive(isReady);
        // Mantiene coherencia interna aunque el flujo de construcción usa la propiedad override:
    puedeConstruirse = isReady; // la propiedad combina gating
    }

    protected virtual void Activar()
    {
        if (isReady) return;
        isReady = true; // heredado
        AplicarEstadoVisual();
        PlayReadySfx();
        
        // Notificar al PlayerUIManager sobre el cambio de estado
        PlayerUIManager playerUIManager = GetHoldingPlayerUIManager();
        if (playerUIManager != null)
        {
            playerUIManager.RefreshHeldObjectUI(UIIndex);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider && collision.collider.GetComponent<Arrow>() != null)
            Activar();
    }

    public bool StartedReady => startReady;
    
    public void ActivateMaterial()
    {
        if (!isReady)
        {
            SetReady(true);
            AplicarEstadoVisual();
            PlayReadySfx();
            
            // Notificar al PlayerUIManager sobre el cambio de estado
            PlayerUIManager playerUIManager = GetHoldingPlayerUIManager();
            if (playerUIManager != null)
            {
                playerUIManager.RefreshHeldObjectUI(UIIndex);
            }
        }
    }
    
    /// <summary>
    /// Obtiene el PlayerUIManager del jugador que actualmente está sosteniendo este objeto.
    /// </summary>
    private PlayerUIManager GetHoldingPlayerUIManager()
    {
        // Buscar al jugador que está sosteniendo este objeto
        PlayerObjectHolder[] holders = FindObjectsByType<PlayerObjectHolder>(FindObjectsSortMode.None);
        foreach (var holder in holders)
        {
            if (holder.GetHeldObject() == gameObject)
            {
                // Encontramos al jugador que sostiene este objeto
                return holder.GetComponent<PlayerUIManager>();
            }
        }
        return null;
    }

    private void PlayReadySfx()
    {
        if (readyActivateSfxIndex < 0) return;
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(readyActivateSfxIndex);
        }
    }
}