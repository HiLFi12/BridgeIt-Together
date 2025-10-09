using UnityEngine;

public class PlasterBucketSpawner : MonoBehaviour, IInteractable
{
    [Header("Prefab de material tipo 2 a instanciar")]
    [SerializeField] private GameObject prefabMaterialTipo2;
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;

    public InteractPriority InteractPriority => interactPriority;

    public void Interact(GameObject interactor)
    {
        if (prefabMaterialTipo2 == null)
        {
            Debug.LogError("No se ha asignado el prefab de material tipo 2 en PlasterBucketSpawner.");
            return;
        }
        var holder = interactor.GetComponent<PlayerObjectHolder>();
        if (holder == null)
        {
            Debug.LogError("El interactor no tiene PlayerObjectHolder.");
            return;
        }
        if (holder.HasObjectInHand())
        {
            Debug.Log("El jugador ya sostiene un objeto.");
            return;
        }
        // Instanciar el material tipo 2 en la posición del spawner
        GameObject nuevoMaterial = Instantiate(prefabMaterialTipo2, transform.position + Vector3.up * 0.5f, transform.rotation);
        holder.PickUpExistingInstance(nuevoMaterial);
        Debug.Log("Material tipo 2 instanciado y entregado al jugador.");
    }
}
