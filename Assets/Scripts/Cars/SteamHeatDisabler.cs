using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Componente para el gran vapor direccional:
/// - Debe ir en un GameObject con un collider marcado como Trigger.
/// - Al entrar un horno (Furnace) o HeatSphere, desactiva los hijos que tengan HeatSphere (SetActive(false)).
///   (Esto simula "apagar" su calor visual / funcional). 
/// - Al salir, actualmente NO los reactiva (diseño basado en especificación). Si se requiere reactivación, se puede extender.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SteamHeatDisabler : MonoBehaviour
{
    [Tooltip("Capas filtradas. Si se deja en 0, no filtra por capa.")] public LayerMask layerMask;
    [Tooltip("Mostrar logs de depuración.")] [SerializeField] private bool debugLogs = false;

    private readonly HashSet<HeatSphere> _disabled = new HashSet<HeatSphere>();

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col && !col.isTrigger) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!LayerAllowed(other.gameObject.layer)) return;

        HeatSphere hs = other.GetComponentInParent<HeatSphere>();
        if (hs != null)
        {
            if (!_disabled.Contains(hs))
            {
                _disabled.Add(hs);
                hs.gameObject.SetActive(false);
                if (debugLogs)
                {
                    Debug.Log($"[SteamHeatDisabler] HeatSphere '{hs.name}' desactivada.", this);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    private bool LayerAllowed(int layer)
    {
        if (layerMask.value == 0) return true;
        return (layerMask.value & (1 << layer)) != 0;
    }
}
