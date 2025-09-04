using UnityEngine;

namespace BridgeItTogether.Gameplay.Abstractions
{
    /// <summary>
    /// Filtra y aplica ignorado de colisiones entre el dueño (vehículo) y otros colliders/objetos.
    /// </summary>
    public interface ICollisionFilter
    {
        /// <summary>
        /// Inicializa el filtro con el GameObject dueño (se cachean colliders y se puede preconfigurar ignores globales).
        /// </summary>
        void Initialize(GameObject owner);

        /// <summary>
        /// Indica si el collider debería ser ignorado por política (marcadores, interfaces, etc.).
        /// No aplica cambios físicos por sí mismo.
        /// </summary>
        bool ShouldIgnore(Collider other);

        /// <summary>
        /// Aplica Physics.IgnoreCollision entre el dueño y el collider (incluye colliders hijos/padres).
        /// </summary>
        void ApplyIgnore(Collider other);

    /// <summary>
    /// Aplica IgnoreCollision con todos los objetos marcados en la escena (CollidableNT o que implementen ICollidableNT).
    /// Útil para prevenir el primer contacto físico tras el spawn.
    /// </summary>
    void ApplyIgnoreToAllMarkedInScene();
    }
}
