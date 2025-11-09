using System.Collections;
using UnityEngine;

/// <summary>
/// Acero Industrial (Era Industrial):
/// Objeto interactuable que entrega/instancia el Material Tipo 2 de la era Industrial al presionar E.
/// - Si el jugador tiene PlayerObjectHolder, lo coloca en la mano.
/// - Si no, lo instancia en el punto de spawn (o en la posición del objeto como fallback).
/// Incluye un pequeño tiempo de recarga para evitar spam.
/// </summary>
public class AceroIndustrial : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private MaterialPrefabSO materialPrefabsSO; // SO con prefabs de materiales
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
    [SerializeField] private float tiempoRecarga = 2.0f; // Cooldown entre entregas
    [SerializeField] private Transform puntoSpawn; // Dónde aparece el material si no hay holder

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
    
    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }

    public void Interact(GameObject interactor)
    {
        if (enRecarga)
        {
            Debug.Log("Acero Industrial en recarga, espera un momento.");
            return;
        }

        var holder = interactor != null ? interactor.GetComponent<PlayerObjectHolder>() : null;
        if (holder == null)
        {
            // Fallback: instanciar en el suelo
            GenerarMaterialEnSuelo();
            StartCoroutine(Recargar());
            return;
        }

        EntregarMaterialEnMano(holder);
        StartCoroutine(Recargar());
    }

    private void EntregarMaterialEnMano(PlayerObjectHolder playerObjectHolder)
    {
        if (materialPrefabsSO == null) return;
        GameObject prefab = materialPrefabsSO.GetMaterialPrefab(2, BridgeQuadrantSO.EraType.Industrial);
        if (prefab == null)
        {
            Debug.LogError("No se encontró prefab para Material Tipo 2 (Industrial).");
            return;
        }

        Vector3 pos = transform.position + Vector3.up * 0.3f;
        GameObject instancia = Instantiate(prefab, pos, Quaternion.identity);

        if (playerObjectHolder.HasObjectInHand())
        {
            Debug.Log("Jugador ya sostiene un objeto. Intercambiando objetos.");
        }

        playerObjectHolder.PickUpExistingInstance(instancia);
        ProducirEfectos();
        Debug.Log("Se entregó Material Tipo 2 (Acero Industrial) a la mano del jugador.");
    }

    private void GenerarMaterialEnSuelo()
    {
        if (materialPrefabsSO == null) return;
        GameObject prefab = materialPrefabsSO.GetMaterialPrefab(2, BridgeQuadrantSO.EraType.Industrial);
        if (prefab == null)
        {
            Debug.LogError("No se encontró prefab para Material Tipo 2 (Industrial).");
            return;
        }

        Vector3 pos = puntoSpawn.position + Vector3.down * 0.25f;
        Instantiate(prefab, pos, Quaternion.identity);
        ProducirEfectos();
        Debug.Log("Se entregó Material Tipo 2 (Acero Industrial) en el suelo (fallback).");
    }

    private IEnumerator Recargar()
    {
        enRecarga = true;
        yield return new WaitForSeconds(tiempoRecarga);
        enRecarga = false;
    }

    private void ProducirEfectos()
    {
        // TODO: efectos visuales/sonoros de extracción de acero (chisporroteo, etc.)
    }
}

