using UnityEngine;

/// <summary>
/// Componente que hace de "proyectil" que cae desde el cielo y, al impactar con el puente,
/// notifica al BridgeSurpriseEvent para ejecutar el efecto sorpresa.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class BridgeSurpriseProjectile : MonoBehaviour
{
    private BridgeSurpriseEvent owner;
    private EventoSorpresa evento;

    private bool triggered;

    public void Setup(BridgeSurpriseEvent owner, EventoSorpresa evento)
    {
        this.owner = owner;
        this.evento = evento;
        // Asegurar collider adecuado para colisión
        var col = GetComponent<Collider>();
        col.isTrigger = false;
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (triggered) return;
        triggered = true;
        owner?.OnProjectileHitBridge(evento, collision.collider);
    }
}
