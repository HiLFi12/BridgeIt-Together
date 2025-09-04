using UnityEngine;

/// <summary>
/// Marca a un objeto como miembro de un grupo de llegada.
/// Usado por RoyalCarriageSplitAction para agrupar componentes que cruzan una meta.
/// </summary>
[DisallowMultipleComponent]
public class ArrivalGateMember : MonoBehaviour
{
    [Tooltip("Si es true, este miembro cuenta como clave para completar la llegada del grupo.")]
    public bool isKey = false;
}
