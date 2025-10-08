using UnityEngine;

public class FurnaceManager : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractPriority interactPriority = InteractPriority.High;
    [SerializeField] private Furnace furnace;
    [SerializeField] private Gameplay.Heat.FurnaceCook furnaceCook;

    public InteractPriority InteractPriority => interactPriority;

    private void Awake()
    {
        if (furnace == null) furnace = GetComponentInChildren<Furnace>();
        if (furnaceCook == null) furnaceCook = GetComponent<Gameplay.Heat.FurnaceCook>();
    }

    public void Interact(GameObject player)
    {
        Debug.Log("FurnaceManager.Interact llamado");

        var holder = player.GetComponent<PlayerObjectHolder>();
        if (holder == null)
        {
            Debug.LogError("PlayerObjectHolder no encontrado");
            return;
        }

        var heldObj = holder.GetHeldObject();
        if (heldObj == null)
        {
            Debug.LogWarning("No hay objeto en mano");
            return;
        }

        Debug.Log($"Objeto detectado: {heldObj.name}");

        var coalItem = heldObj.GetComponent<CoalItem>();
        var material1 = heldObj.GetComponent<MaterialTipo1>();
        var material2 = heldObj.GetComponent<MaterialBaseInteractable>();

        if (coalItem != null)
        {
            Debug.Log("Intentando agregar carbón");
            if (furnace != null)
            {
                furnace.Interact(player);
                Debug.Log("Carbón agregado exitosamente");
            }
            else
            {
                Debug.LogError("Furnace es null");
            }
        }
        else if (material1 != null || material2 != null)
        {
            Debug.Log($"Intentando cocinar material (Tipo1: {material1 != null}, Tipo2: {material2 != null})");
            if (furnaceCook != null)
            {
                furnaceCook.Interact(player);
                Debug.Log("Material enviado a cocinar");
            }
            else
            {
                Debug.LogError("FurnaceCook es null");
            }
        }
        else
        {
            Debug.LogWarning($"Objeto no válido: {heldObj.name}");
        }
    }
}