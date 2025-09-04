using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

namespace BridgeItTogether.Gameplay.AutoControllers
{
    public class DefaultPlayerCollisionHandler : IPlayerCollisionHandler
    {
        private GameObject owner;
        private Transform ownerTransform;
        private Collider[] vehiculoCols;
        private MonoBehaviour coroutineRunner;
        private PlayerCollisionSettings cfg;

        public void Initialize(GameObject owner, MonoBehaviour coroutineRunner, PlayerCollisionSettings settings)
        {
            this.owner = owner;
            this.ownerTransform = owner != null ? owner.transform : null;
            this.coroutineRunner = coroutineRunner;
            this.cfg = settings ?? new PlayerCollisionSettings();
            vehiculoCols = owner != null ? owner.GetComponentsInChildren<Collider>() : new Collider[0];
            if (this.cfg.curvaVuelo == null) this.cfg.curvaVuelo = AnimationCurve.EaseInOut(0,0,1,1);
        }

        public void Tick()
        {
            if (!cfg.usarDeteccionProximidadJugador) return;
            var jugadores = Physics.OverlapSphere(ownerTransform.position, cfg.radioDeteccionJugador, cfg.playerLayer);
            foreach (var col in jugadores)
            {
                if (col != null && col.CompareTag("Player"))
                {
                    float dist = Vector3.Distance(ownerTransform.position, col.transform.position);
                    if (dist < cfg.distanciaActivacionImpacto && !EstaIgnorandoColisionesConVehiculo(col.gameObject))
                    {
                        HandleHit(col.gameObject, col.transform.position);
                        break;
                    }
                }
            }
        }

        public void HandleHit(GameObject jugador, Vector3 puntoImpacto)
        {
            var launch = jugador.GetComponent<PlayerLaunchController>();
            if (launch != null && launch.EstaSiendoLanzado()) return;
            if (launch == null) launch = jugador.AddComponent<PlayerLaunchController>();

            if (cfg.desactivarColisionesEnVuelo)
            {
                DesactivarColisionesConJugador(jugador);
            }

            var holder = jugador.GetComponent<PlayerObjectHolder>();
            if (holder != null && holder.HasObjectInHand()) holder.DropObject();

            Vector3 destino = CalcularPosicionAterrizaje(jugador.transform.position);
            float h = cfg.alturaVuelo * cfg.factorAlturaParabola;
            float t = cfg.tiempoEnAire * cfg.factorTiempoParabola;

            launch.LanzarJugador(destino, h, t, cfg.curvaVuelo);

            if (cfg.desactivarColisionesEnVuelo)
            {
                coroutineRunner.StartCoroutine(ReactivarColisionesConJugador(jugador, t + cfg.tiempoExtraIgnorarColisiones));
            }
        }

        private Vector3 CalcularPosicionAterrizaje(Vector3 posicionJugador)
        {
            if (cfg.usarPuntosPersonalizados && cfg.puntosDeCaidaPersonalizados != null && cfg.puntosDeCaidaPersonalizados.Length > 0)
            {
                return SeleccionarMejorPuntoPersonalizado(cfg.puntosDeCaidaPersonalizados, posicionJugador);
            }
            return posicionJugador;
        }

        private Vector3 SeleccionarMejorPuntoPersonalizado(Transform[] puntos, Vector3 posicionOriginal)
        {
            float mejorDist = float.MaxValue; Vector3 mejor = posicionOriginal; Transform best = null;
            Vector3 posicionChoque = ownerTransform.position;
            foreach (var p in puntos)
            {
                if (!EsPuntoCaidaValido(p)) continue;
                float d = Vector3.Distance(posicionChoque, p.position);
                if (d < mejorDist) { mejorDist = d; mejor = p.position; best = p; }
            }
            if (best != null) { mejor.y += 0.5f; }
            return mejor;
        }

        private bool EsPuntoCaidaValido(Transform p)
        {
            if (p == null) return false;
            if (!cfg.requerirPuntosValidos) return true;
            foreach (var tag in cfg.tagsPermitidosPuntoCaida)
            {
                if (!string.IsNullOrEmpty(tag) && p.CompareTag(tag)) return true;
            }
            var col = p.GetComponent<Collider>();
            if (col != null && !col.isTrigger) return true;
            return false;
        }

        private void DesactivarColisionesConJugador(GameObject jugador)
        {
            Collider[] jugadorCols = jugador.GetComponentsInChildren<Collider>();
            foreach (var cj in jugadorCols)
            {
                if (cj == null || cj.isTrigger) continue;
                foreach (var cv in vehiculoCols)
                {
                    if (cv == null || cv.isTrigger) continue;
                    Physics.IgnoreCollision(cj, cv, true);
                }
            }
        }

        private System.Collections.IEnumerator ReactivarColisionesConJugador(GameObject jugador, float esperar)
        {
            yield return new WaitForSeconds(esperar);
            if (jugador == null || owner == null) yield break;
            Collider[] jugadorCols = jugador.GetComponentsInChildren<Collider>();
            foreach (var cj in jugadorCols)
            {
                if (cj == null || cj.isTrigger) continue;
                foreach (var cv in vehiculoCols)
                {
                    if (cv == null || cv.isTrigger) continue;
                    Physics.IgnoreCollision(cj, cv, false);
                }
            }
        }

        private bool EstaIgnorandoColisionesConVehiculo(GameObject jugador)
        {
            var cJ = jugador.GetComponent<Collider>();
            var cV = owner.GetComponent<Collider>();
            if (cJ != null && cV != null)
            {
                return Physics.GetIgnoreCollision(cJ, cV);
            }
            return false;
        }
    }
}
