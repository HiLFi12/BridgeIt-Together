using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Carbón interactuable: cuando el jugador lo usa sobre un Furnace, intenta entregarlo como combustible.
/// Patrón similar a CortezaResistente (IInteractable) pero dirigido a Furnace.
/// </summary>
[DisallowMultipleComponent]
public class CoalItem : MonoBehaviour, IInteractable, IHitable
{
	[Header("Interacción")]
	[SerializeField] private InteractPriority interactPriority = InteractPriority.Low;
	[Tooltip("Si se deja nulo, se buscará un Furnace cercano al interactuar.")]
	[SerializeField] private Furnace targetFurnace;
	[Tooltip("Radio de búsqueda para encontrar un Furnace si no hay uno asignado.")]
	[SerializeField] private float searchRadius = 2.0f;
	[Tooltip("Capas a considerar para buscar el Furnace.")]
	[SerializeField] private LayerMask searchLayers = ~0;
	[SerializeField] private bool debugLogs = false;

	public InteractPriority InteractPriority => interactPriority;

	public void Interact(GameObject interactor)
	{
		if (interactor == null) return;

		// Verificar que el jugador realmente esté sosteniendo este carbón
		var holder = interactor.GetComponent<PlayerObjectHolder>();
		if (holder == null || !holder.HasObjectInHand())
		{
			if (debugLogs) Debug.Log("[CoalItem] El interactor no sostiene ningún objeto.", this);
			return;
		}

		var held = holder.GetHeldObject();
		if (held == null || (held != gameObject && !held.transform.IsChildOf(transform)))
		{
			if (debugLogs) Debug.Log("[CoalItem] El jugador no sostiene este carbón.", this);
			return;
		}

		Furnace furnace = targetFurnace ? targetFurnace : FindNearestFurnace();
		if (furnace == null)
		{
			if (debugLogs) Debug.Log("[CoalItem] No se encontró Furnace cercano.", this);
			return;
		}

		bool added = furnace.TryAddCoal(interactor);
		if (!added)
		{
			if (debugLogs) Debug.Log("[CoalItem] No se pudo agregar carbón al Furnace (quizá está lleno o el objeto no era válido).", this);
		}
	}

	private Furnace FindNearestFurnace()
	{
		Collider[] cols = Physics.OverlapSphere(transform.position, searchRadius, searchLayers, QueryTriggerInteraction.Collide);
		float bestDist = float.MaxValue;
		Furnace best = null;
		for (int i = 0; i < cols.Length; i++)
		{
			var c = cols[i];
			if (!c) continue;
			var f = c.GetComponentInParent<Furnace>();
			if (f == null) continue;
			float d = (f.transform.position - transform.position).sqrMagnitude;
			if (d < bestDist)
			{
				bestDist = d;
				best = f;
			}
		}
		return best;
	}

    public void OnLaunched(Vector3 targetPosition)
    {
    }

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
		Gizmos.DrawWireSphere(transform.position, searchRadius);
	}
#endif
}
