using UnityEngine;

public class MaterialTipo2Ready : MaterialTipo2Base
{
    [Header("Referencias de mallas")]
    [SerializeField] private GameObject notReadyMesh;
    [SerializeField] private GameObject readyMesh;

    [Header("Estado (heredado)")]
    [SerializeField, Tooltip("Inicializa el material como listo. Si se desactiva, requiere flecha.")] private bool startReady = false;
    public override bool PuedeConstruirse => base.PuedeConstruirse; // la base ya combina isReady

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

    private void AutoVincularMeshesSiFaltan()
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

    private void AplicarEstadoVisual()
    {
    if (notReadyMesh) notReadyMesh.SetActive(!isReady);
    if (readyMesh) readyMesh.SetActive(isReady);
        // Mantiene coherencia interna aunque el flujo de construcción usa la propiedad override:
    puedeConstruirse = isReady; // la propiedad combina gating
    }

    private void Activar()
    {
    if (isReady) return;
    isReady = true; // heredado
    AplicarEstadoVisual();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider && collision.collider.GetComponent<Arrow>() != null)
            Activar();
    }
}