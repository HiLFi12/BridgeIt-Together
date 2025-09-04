using UnityEngine;

namespace BridgeItTogether.Gameplay.Abstractions
{
    [System.Serializable]
    public class PlayerCollisionSettings
    {
        public LayerMask playerLayer = 1;
        public float radioDeteccionJugador = 2f;
        public float distanciaActivacionImpacto = 1.5f;
        public bool usarDeteccionProximidadJugador = true;
        public bool desactivarColisionesEnVuelo = true;
        public float tiempoExtraIgnorarColisiones = 0.5f;
        public float alturaVuelo = 2f;
        public float tiempoEnAire = 1f;
        public AnimationCurve curvaVuelo = null;
        public float factorAlturaParabola = 0.6f;
        public float factorTiempoParabola = 0.8f;
        public bool usarPuntosPersonalizados = true;
        public Transform[] puntosDeCaidaPersonalizados;
        public bool requerirPuntosValidos = true;
        public string[] tagsPermitidosPuntoCaida = { "DropPoint", "Floor", "Platform", "Walkable" };
    }

    public interface IPlayerCollisionHandler
    {
        void Initialize(GameObject owner, MonoBehaviour coroutineRunner, PlayerCollisionSettings settings);
        void Tick();
        void HandleHit(GameObject jugador, Vector3 puntoImpacto);
    }
}
