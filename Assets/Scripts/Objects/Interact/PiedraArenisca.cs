using System.Collections;
using UnityEngine;

/// <summary>
/// Piedra Arenisca (Era Medieval):
/// Objeto interactuable que entrega el Material Tipo 2 al presionar E.
/// No requiere condiciones especiales: solo acercarse e interactuar.
/// Genera el prefab correspondiente al material tipo 2 de la era medieval.
/// </summary>
public class PiedraArenisca : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private MaterialPrefabSO materialPrefabsSO; // SO con prefabs de materiales
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private float tiempoRecarga = 2.0f; // Tiempo de cooldown entre entregas
    [SerializeField] private Transform puntoSpawn; // Dónde aparece el material (fallback)

    private bool enRecarga = false;

    public InteractPriority InteractPriority => interactPriority;

    private void Start()
    {
        if (materialPrefabsSO == null)
        {
            Debug.LogError("No se ha asignado MaterialPrefabSO en " + gameObject.name);
        }
        if (puntoSpawn == null)
        {
            puntoSpawn = transform;
        }
    }

    // Entrega Material Tipo 2 (Medieval) al interactuar
    public void Interact(GameObject interactor)
    {
        if (enRecarga)
        {
            Debug.Log("Piedra Arenisca en recarga, espera un momento.");
            return;
        }

        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder == null)
        {
            Debug.LogWarning("El interactor no tiene PlayerObjectHolder. Spawneando en el suelo como fallback.");
            GenerarAreniscaEnSuelo();
            StartCoroutine(Recargar());
            return;
        }

        EntregarAreniscaEnMano(holder);
        StartCoroutine(Recargar());
    }

    private void EntregarAreniscaEnMano(PlayerObjectHolder playerObjectHolder)
    {
        if (materialPrefabsSO == null) return;
        GameObject prefabTipo2Medieval = materialPrefabsSO.GetMaterialPrefab(2, BridgeQuadrantSO.EraType.Medieval);
        if (prefabTipo2Medieval == null)
        {
            Debug.LogError("No se encontró prefab para Material Tipo 2 (Medieval).");
            return;
        }

        // Instanciar temporalmente cerca de la piedra y pasarlo a la mano del jugador
        Vector3 pos = transform.position + Vector3.up * 0.3f;
        GameObject instancia = Instantiate(prefabTipo2Medieval, pos, Quaternion.identity);

        if (playerObjectHolder.HasObjectInHand())
        {
            Debug.Log("Jugador ya sostiene un objeto. Intercambiando objetos.");
        }

        playerObjectHolder.PickUpExistingInstance(instancia);
        ProducirEfectos();
        Debug.Log("Se entregó Material Tipo 2 (Piedra Arenisca) a la mano del jugador.");
    }

    private void GenerarAreniscaEnSuelo()
    {
        if (materialPrefabsSO == null) return;
        GameObject prefabTipo2Medieval = materialPrefabsSO.GetMaterialPrefab(2, BridgeQuadrantSO.EraType.Medieval);
        if (prefabTipo2Medieval == null)
        {
            Debug.LogError("No se encontró prefab para Material Tipo 2 (Medieval).");
            return;
        }
        Vector3 pos = puntoSpawn.position + Vector3.down * 0.25f;
        Instantiate(prefabTipo2Medieval, pos, Quaternion.identity);
        ProducirEfectos();
        Debug.Log("Se entregó Material Tipo 2 (Piedra Arenisca) en el suelo (fallback).");
    }

    private IEnumerator Recargar()
    {
        enRecarga = true;
        yield return new WaitForSeconds(tiempoRecarga);
        enRecarga = false;
    }

    private void ProducirEfectos()
    {
        // TODO: efectos visuales/sonoros de desprendimiento de arenisca
    }
}
