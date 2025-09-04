using UnityEngine;


[RequireComponent(typeof(MaterialTipo2Base))]
public class MaterialTipo2CombinarPalo : MonoBehaviour
{
    private MaterialTipo2Base baseMaterial;

    private void Awake()
    {
        baseMaterial = GetComponent<MaterialTipo2Base>();
        baseMaterial.OnInteracted += HandleInteract;
    }

    private void OnDestroy()
    {
        if (baseMaterial != null)
        {
            baseMaterial.OnInteracted -= HandleInteract;
        }
    }

    private void HandleInteract(GameObject interactor)
    {
        Debug.Log("[MaterialTipo2CombinarPalo] Resina lista para combinar con palo.");
    }
}
