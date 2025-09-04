using UnityEngine;

namespace BridgeItTogether.Gameplay.Abstractions
{
    // Marcador: objetos con este componente+interfaz no deben colisionar con el auto
    public interface ICollidableNT { }

    // Componente vacío que implementa la interfaz; úsalo en prefabs/objetos a ignorar por los vehículos
    [DisallowMultipleComponent]
    [AddComponentMenu("Bridge It Together/Collisions/Non-Collidable (Vehicles)")]
    [Tooltip("Marca este objeto como NO colisionable con los vehículos controlados por AutoController.")]
    public class CollidableNT : MonoBehaviour, ICollidableNT { }
}
