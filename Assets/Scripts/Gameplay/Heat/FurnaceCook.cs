using UnityEngine;
using System.Reflection;

/// <summary>
/// Mezcladora que hereda de GenericObject2 pero solo permite procesar (mezclar) cuando el HeatSphere está activo.
/// No modifica GenericObject2: intercepta Interact para suprimir la cocción automática sin calor
/// y permitirla cuando hay calor. Mantiene el flujo de colocación de materiales del padre.
/// </summary>
[DisallowMultipleComponent]
public class FurnaceCook : GenericObject2
{
    [Header("Heat Gating")]
    [SerializeField] private HeatSphere heatSphere; // Referencia al HeatSphere que habilita la mezcla
    [SerializeField] private bool logMessages = false;

    private FieldInfo eraField; // acceso por reflexión al campo privado 'era' del padre

    private void Reset()
    {
        // Intentar autovincular HeatSphere cercano (en este GameObject o en padres)
        if (heatSphere == null)
        {
            heatSphere = GetComponentInParent<HeatSphere>();
            if (heatSphere == null)
                heatSphere = FindAnyObjectByType<HeatSphere>();
        }
    }

    private void Awake()
    {
        // Preparar reflection para cambiar temporalmente la 'era' y evitar autococción cuando no hay calor
        eraField = typeof(GenericObject2).GetField("era", BindingFlags.Instance | BindingFlags.NonPublic);
        if (eraField == null && logMessages)
        {
            Debug.LogWarning("[FurnaceCook] No se pudo acceder al campo 'era' por reflexión. El gating Prehistoric->Medieval podría no funcionar.", this);
        }
    }

    private bool IsHeated()
    {
        return heatSphere != null && heatSphere.gameObject.activeInHierarchy;
    }

    public new void Interact(GameObject interactor)
    {
        bool heated = IsHeated();

        // Determinar si el interactor tiene un objeto en mano (para decidir si llamamos a la colocación del padre)
        var holder = interactor != null ? interactor.GetComponent<PlayerObjectHolder>() : null;
        bool hasItem = holder != null && holder.HasObjectInHand();

        // Si NO hay calor y el jugador intenta cocinar sin objeto, bloquear
        if (!heated && !hasItem)
        {
            if (logMessages) Debug.Log("[FurnaceCook] Se requiere calor para iniciar la mezcla.", this);
            // No llamar a base.Interact, para evitar que inicie cocción o hold
            return;
        }

        // Si hay un objeto en mano y NO hay calor, queremos permitir la colocación pero evitar la cocción automática
        if (hasItem && !heated && eraField != null)
        {
            // Guardar era actual y setear temporalmente a Medieval para suprimir autococción Prehistoric
            var eraActual = GetEra();
            try
            {
                eraField.SetValue(this, BridgeQuadrantSO.EraType.Medieval);
                base.Interact(interactor); // Coloca el material y NO autococina
            }
            finally
            {
                // Restaurar era original
                eraField.SetValue(this, eraActual);
            }
            return;
        }

        // En los demás casos, delegar al comportamiento base (si hay calor, funciona normal)
        base.Interact(interactor);
    }
}
