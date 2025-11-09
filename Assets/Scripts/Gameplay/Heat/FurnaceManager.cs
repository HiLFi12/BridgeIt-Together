using System.Collections;
using UnityEngine;

public class FurnaceManager : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractPriority interactPriority = InteractPriority.High;
    [SerializeField] private Furnace furnace;
    [SerializeField] private Gameplay.Heat.FurnaceCook furnaceCook;
    [SerializeField] private float delayColocacion = 0.1f; // Delay antes de colocar

    public InteractPriority InteractPriority => interactPriority;

    private void Awake()
    {
        if (furnace == null) furnace = GetComponentInChildren<Furnace>();
        if (furnaceCook == null) furnaceCook = GetComponent<Gameplay.Heat.FurnaceCook>();
    }

    public void Interact(GameObject player)
    {
        var holder = player.GetComponent<PlayerObjectHolder>();
        if (holder == null || !holder.HasObjectInHand()) return;

        GameObject heldObj = holder.GetHeldObject();
        
        // 1. Detectar el tipo INMEDIATAMENTE
        int itemType = DeterminarTipoItem(heldObj);
        
        if (itemType == 0)
        {
            Debug.Log("Objeto no válido para el horno");
            return;
        }

        // 2. Iniciar corutina para colocar después del delay
        StartCoroutine(ColocarConDelay(player, heldObj, itemType));
    }

    private IEnumerator ColocarConDelay(GameObject player, GameObject heldObj, int itemType)
    {
        Debug.Log($"Tipo detectado: {(itemType == 1 ? "Carbón" : "Material")} - Esperando {delayColocacion}s...");
        
        // Esperar el delay configurado
        yield return new WaitForSeconds(delayColocacion);

        // 3. Ejecutar la colocación según el tipo
        switch (itemType)
        {
            case 1: // Carbón
                Debug.Log("Colocando carbón en el horno");
                furnace?.Interact(player);
                break;

            case 2: // Material para cocinar
                Debug.Log("Colocando material en el horno");
                furnaceCook?.Interact(player);
                break;
        }
    }
    
    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }

    private int DeterminarTipoItem(GameObject objeto)
    {
        if (objeto == null) return 0;

        // Tipo 1: Carbón
        if (objeto.GetComponent<CoalItem>() != null)
            return 1;

        // Tipo 2: Material para cocinar
        if (objeto.GetComponent<MaterialTipo1>() != null)
            return 2;

        BridgeMaterialInfo materialInfo = objeto.GetComponent<BridgeMaterialInfo>();
        if (materialInfo != null && 
            (materialInfo.layerIndex == 0 || materialInfo.layerIndex == 1))
            return 2;

        return 0;
    }
}