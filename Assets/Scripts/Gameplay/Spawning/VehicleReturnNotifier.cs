using UnityEngine;
using BridgeItTogether.Gameplay.Rondas;

namespace BridgeItTogether.Gameplay.Spawning
{
    /// <summary>
    /// Notifica al RoundController cuando el vehículo es devuelto al pool (normalmente vía SetActive(false)).
    /// Asume que OnDisable ocurre al regresar al pool.
    /// </summary>
    public class VehicleReturnNotifier : MonoBehaviour
    {
        public RoundController roundController;

        private void OnDisable()
        {
            if (roundController != null)
            {
                roundController.NotificarAutoDevueltoAlPool(gameObject);
            }
        }
    }
}
