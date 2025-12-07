using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.SafeZones;

namespace BridgeItTogether.Gameplay.AutoControllers
{
    [DisallowMultipleComponent]
    public class AutoController : MonoBehaviour
    {
        // ==== Movimiento ====
        [Header("Movimiento")]
        [SerializeField] private float velocidadBase = 5f;
        public Vector3 direccionInicial = Vector3.right;

        // ==== Tags (informativos) ====
        [Header("Tags")]
        [SerializeField] private bool asegurarTagVehiculo = true;
        [SerializeField] private string nombreTagVehiculo = "Vehicle";

        // ==== Lanzamiento de IHitable ====
        [Header("Lanzamiento IHitable")]
        [SerializeField] private string safeZoneTag = "SafeZone";
        [SerializeField] private float ignoreCollisionDuration = 0.5f; // Tiempo para re-habilitar colisiones después del impacto con IHitable

        // Estado
        private Rigidbody rb;
        private bool isInitialized;
        private bool isPaused;
        private Vector3 direccionMovimiento;
        private readonly HashSet<Collider> currentlyIgnoredColliders = new HashSet<Collider>();

        private void Awake()
        {
            direccionMovimiento = direccionInicial.sqrMagnitude > 0.0001f ? direccionInicial.normalized : Vector3.right;
        }

        public void Initialize(Vector3 direction)
        {
            SetDirection(direction);

            if (asegurarTagVehiculo)
            {
                try
                {
                    if (!string.IsNullOrEmpty(nombreTagVehiculo) && !CompareTag(nombreTagVehiculo))
                        tag = nombreTagVehiculo;
                }
                catch (UnityException)
                {
                    Debug.LogWarning($"[AutoController] Tag '{nombreTagVehiculo}' no existe. Agrega el Tag en Project Settings.", this);
                }
            }

            rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;

            AlinearRotacionConDireccion();
            isInitialized = true;
            isPaused = false;
        }

        protected virtual void FixedUpdate()
        {
            if (!isInitialized || isPaused) return;

            if (rb != null && velocidadBase > 0f)
            {
                Vector3 dir = direccionMovimiento.sqrMagnitude > 0.0001f ? direccionMovimiento.normalized : Vector3.right;
                rb.MovePosition(rb.position + dir * velocidadBase * Time.fixedDeltaTime);
            }
        }

        public void SetSpeed(float speed) => velocidadBase = Mathf.Max(0f, speed);
        public float GetSpeed() => velocidadBase;

        public void SetDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.0001f)
                direccionMovimiento = direction.normalized;
            AlinearRotacionConDireccion();
        }

        public void Pause()
        {
            isPaused = true;
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
#else
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
#endif
                rb.angularVelocity = Vector3.zero;
            }
        }

        public void Resume()
        {
            isPaused = false;
            AlinearRotacionConDireccion();
        }

        private void AlinearRotacionConDireccion()
        {
            Vector3 look = new Vector3(direccionMovimiento.x, 0f, direccionMovimiento.z);
            if (look.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(look, Vector3.up);
        }

        // ===================== IHitable: colisión y lanzamiento =====================
        private void OnCollisionEnter(Collision collision)
        {
            if (!isInitialized) return;

            // Solo procesar colisiones con objetos IHitable (players, etc.)
            var hitable = collision.collider.GetComponentInParent<IHitable>();
            if (hitable == null)
            {
                // No es IHitable: dejar que la física actúe normalmente (suelo, puente, etc.)
                // Solo procesar cuadrantes si aplica
                TryHandleQuadrantCollision(collision.collider);
                return;
            }

            // Es IHitable: ignorar colisiones temporalmente para evitar bugs de colisión repetida
            StartCoroutine(TemporarilyIgnoreCollision(collision.collider));

            // Resto de comportamiento normal para IHitable
            if (TryHandleQuadrantCollision(collision.collider)) return;
            TryLaunchHitable(collision.collider);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized) return;

            if (TryHandleQuadrantCollision(other)) return;
            TryLaunchHitable(other);
        }
        
        // csharp
        private bool TryHandleQuadrantCollision(Collider col)
        {
            if (col == null) return false;

            var quadrantInst = col.GetComponentInParent<BridgeQuadrantInstance>();
            if (quadrantInst != null && quadrantInst.quadrantSO != null)
            {
                var so = quadrantInst.quadrantSO;
                var layers = so.requiredLayers;

                bool shouldDestroy = false;

                if (layers != null && layers.Length >= 1)
                {
                    bool layer0Done = layers[0].isCompleted;
                    // consider layer2 as the "top" layer; if it doesn't exist treat as not completed
                    bool layer2Done = (layers.Length > 2) ? layers[2].isCompleted : false;

                    // Destroy when base (layer0) is present and the top layer is NOT present.
                    // This covers both: layer1 missing (layer0 built) and layer1 present but layer2 missing.
                    shouldDestroy = layer0Done && !layer2Done;
                }
                else
                {
                    // Fallback: require first layer completed and last layer not completed
                    if (layers != null && layers.Length > 0)
                    {
                        bool firstDone = layers[0].isCompleted;
                        bool lastDone = layers[layers.Length - 1].isCompleted;
                        shouldDestroy = firstDone && !lastDone;
                    }
                }

                if (shouldDestroy)
                {
                    so.ForceDestroyQuadrant();
                    Debug.Log($"[AutoController] Forced destroy quadrant on '{quadrantInst.name}' due to vehicle collision.");
                    return true;
                }

                // If conditions not met (e.g. top layer already completed), do not destroy and allow normal pass-through
                return false;
            }

            return false;
        }


        private void TryLaunchHitable(Collider col)
        {
            if (col == null) return;

            var hitable = col.GetComponent<IHitable>();
            if (hitable == null) return;

            var targetT = (hitable as Component)?.transform;
            if (targetT == null) return;

            if (IsObjectBeingHeld(targetT))
            {
                var holder = FindPlayerHoldingObject(targetT);
                if (holder != null)
                {
                    TryLaunchHitable(holder.GetComponent<Collider>());
                }
                return;
            }

            if (targetT != transform && targetT.IsChildOf(transform))
                targetT.SetParent(null, true);

            // Buscar la SafeZone más cercana
            var safeZone = FindNearestSafeZone(targetT.position);
            if (safeZone == null) return;

            // Desactivar componentes de física antes del lanzamiento
            PrepareHitableForLaunch(targetT);

            // Delegar el lanzamiento a la SafeZone
            safeZone.LaunchHitableToZone(targetT, hitable);
        }

        private void PrepareHitableForLaunch(Transform target)
        {
            if (target == null) return;

            // Deshabilitar CharacterController durante lanzamiento (Players)
            CharacterController charController = target.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = false;
            }

            // Hacer kinematic el Rigidbody durante lanzamiento (Materiales/objetos con física)
            Rigidbody trb = target.GetComponent<Rigidbody>();
            if (trb != null)
            {
#if UNITY_6000_0_OR_NEWER
                trb.linearVelocity = Vector3.zero;
#else
                trb.velocity = Vector3.zero;
#endif
                trb.angularVelocity = Vector3.zero;
                trb.isKinematic = true;
            }
        }

        private bool IsObjectBeingHeld(Transform obj)
        {
            var holders = FindObjectsOfType<PlayerObjectHolder>();
            foreach (var holder in holders)
            {
                if (holder.HasObjectInHand() && holder.GetHeldObject() == obj.gameObject)
                    return true;
            }
            return false;
        }

        private PlayerObjectHolder FindPlayerHoldingObject(Transform obj)
        {
            var holders = FindObjectsOfType<PlayerObjectHolder>();
            foreach (var holder in holders)
            {
                if (holder.HasObjectInHand() && holder.GetHeldObject() == obj.gameObject)
                    return holder;
            }
            return null;
        }

        private SafeZoneArea FindNearestSafeZone(Vector3 from)
        {
            GameObject[] zones = GameObject.FindGameObjectsWithTag(safeZoneTag);
            if (zones == null || zones.Length == 0) return null;

            SafeZoneArea nearest = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < zones.Length; i++)
            {
                var go = zones[i];
                if (go == null) continue;

                var area = go.GetComponent<SafeZoneArea>();
                if (area == null) continue;

                float d = (go.transform.position - from).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    nearest = area;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Ignora temporalmente las colisiones con el collider especificado (solo IHitable) y luego las restaura.
        /// Esto evita bugs de colisión repetida mientras permite que el auto vuelva a chocar más adelante.
        /// </summary>
        private IEnumerator TemporarilyIgnoreCollision(Collider otherCollider)
        {
            if (otherCollider == null) yield break;

            var myCollider = GetComponent<Collider>();
            if (myCollider == null) yield break;

            // Si ya estamos ignorando este collider, no duplicar la coroutine
            if (currentlyIgnoredColliders.Contains(otherCollider)) yield break;

            // Ignorar colisiones
            Physics.IgnoreCollision(otherCollider, myCollider, true);
            currentlyIgnoredColliders.Add(otherCollider);

            // Esperar el tiempo configurado
            yield return new WaitForSeconds(ignoreCollisionDuration);

            // Restaurar colisiones (si ambos colliders siguen existiendo)
            if (otherCollider != null && myCollider != null)
            {
                Physics.IgnoreCollision(otherCollider, myCollider, false);
                currentlyIgnoredColliders.Remove(otherCollider);
            }
        }
    }
}

