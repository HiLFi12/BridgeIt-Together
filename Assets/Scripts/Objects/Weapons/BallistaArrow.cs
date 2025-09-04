using UnityEngine;

/// Flecha de ballesta:
/// - Estados: recargando (CanShoot=false), lista (CanShoot=true), disparada.
/// - Durante recarga permanece como hijo de la ballesta; al disparar se desacopla.
/// - Colisión: siempre se destruye. Si colisiona con Player, lo empuja hacia atrás.
/// - Si colisiona con Material Tipo 2 (MaterialTipo2Base o MaterialBaseInteractable con LayerIndex 1),
///   instancia humo y habilita PuedeConstruirse.
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BallistaArrow : MonoBehaviour
{
  
}
