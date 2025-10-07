// File: Assets/Scripts/Gameplay/AutoControllers/AutoController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions; // Para IHitable
using BridgeItTogether.Gameplay.SafeZones;   // Para SafeZoneArea

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
        [SerializeField] private string bridgeQuadrantTag = "BridgeQuadrant";

        // ==== Interacción con Puente (OverlapSphere + One Interact Per Collider) ====
        [Header("Interacción con Puente (OverlapSphere)")]
        [SerializeField] private Transform interactionPoint;
        [SerializeField] private float interactionRadius = 1.5f;
        [SerializeField] private LayerMask bridgeLayer; // capa de colliders del puente
        [SerializeField] private bool interactAuto = true;
        [SerializeField, Tooltip("Si está activo, cada collider sólo producirá una interacción de por vida (hasta reinicio)." )] private bool oneInteractPerCollider = true;
        [SerializeField, Tooltip("Si está activo, sólo se interactúa la primera vez que el collider entra (no mientras permanece dentro)." )] private bool onlyOnEnter = true;
        [SerializeField] private bool debugInteract = false;

        // ==== Lanzamiento de IHitable (Parábola Física) ====
        [Header("Lanzamiento IHitable (Parábola Física)")]
        [SerializeField] private string safeZoneTag = "SafeZone";
        [SerializeField, Range(10f, 80f)] private float launchAngleDeg = 45f;
        [SerializeField, Min(0.1f)] private float launchSpeed = 12f;
        [SerializeField, Min(0.1f)] private float launchGravity = 9.81f;
        [SerializeField] private bool kinematicDuringLaunch = true;
        [SerializeField] private float minLaunchDistance = 0.05f;

        // Estado
        private Rigidbody rb;
        private bool isInitialized;
        private bool isPaused;
        private Vector3 direccionMovimiento;
        // Tracking de interacción (lógica estilo BaseProbabilitySkill)
        private readonly HashSet<Collider> previousInsideBridge = new();
        private readonly HashSet<Collider> insideNowBridge = new();
        private readonly HashSet<Collider> interactedLifetime = new();

        // Buffers
        private readonly Collider[] overlap = new Collider[8];
        private readonly Dictionary<Transform, Coroutine> activeLaunches = new();

        private void Awake()
        {
            direccionMovimiento = direccionInicial.sqrMagnitude > 0.0001f ? direccionInicial.normalized : Vector3.right;
            if (interactionPoint == null) interactionPoint = transform;
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
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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

            // Movimiento hacia la dirección
            if (rb != null && velocidadBase > 0f)
            {
                Vector3 dir = direccionMovimiento.sqrMagnitude > 0.0001f ? direccionMovimiento.normalized : Vector3.right;
                rb.MovePosition(rb.position + dir * velocidadBase * Time.fixedDeltaTime);
            }

            // Interacción automática con el puente (detección frame a frame con sets)
            if (interactAuto)
                RunInteractionDetection();
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

        // ===================== Interact (daño al puente) =====================
        private void RunInteractionDetection()
        {
            if (!isInitialized) return;

            insideNowBridge.Clear();

            int count = Physics.OverlapSphereNonAlloc(
                interactionPoint.position,
                interactionRadius,
                overlap,
                bridgeLayer,
                QueryTriggerInteraction.Collide
            );

            if (count <= 0)
            {
                previousInsideBridge.Clear();
                return;
            }

            var legacy = GetComponent<VehicleBridgeCollision>();
            if (legacy == null)
            {
                if (debugInteract)
                    Debug.LogWarning("[AutoController] VehicleBridgeCollision no encontrado en el vehículo.", this);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var col = overlap[i];
                if (col == null) continue;

                insideNowBridge.Add(col);

                bool isNewEntry = !previousInsideBridge.Contains(col);
                if (onlyOnEnter && !isNewEntry) continue; // Solo primera vez al entrar
                if (oneInteractPerCollider && interactedLifetime.Contains(col)) continue; // ya interactuado de por vida
                if (!string.IsNullOrEmpty(bridgeQuadrantTag) && !col.CompareTag(bridgeQuadrantTag)) continue;

                VehicleBridgeCollision.HandleTriggerFromChild(gameObject, col);
                if (oneInteractPerCollider)
                    interactedLifetime.Add(col);

                if (debugInteract)
                    Debug.Log($"[AutoController] Bridge interact -> {col.name} (newEntry={isNewEntry}, lifetime={(oneInteractPerCollider ? "tracked" : "multi")})", col);
            }

            previousInsideBridge.Clear();
            foreach (var c in insideNowBridge)
                previousInsideBridge.Add(c);
        }

        /// <summary>
        /// Limpia el estado de interacción (para reutilización / pooling).
        /// </summary>
        public void ResetInteractionState(bool clearLifetime = false)
        {
            previousInsideBridge.Clear();
            insideNowBridge.Clear();
            if (clearLifetime)
                interactedLifetime.Clear();
        }

        // ===================== IHitable: colisión y lanzamiento =====================
        private void OnCollisionEnter(Collision collision)
        {
            if (!isInitialized) return;
            TryLaunchHitable(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized) return;
            TryLaunchHitable(other);
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

            if (activeLaunches.ContainsKey(targetT)) return;

            if (!TryGetRandomPointInNearestSafeZone(targetT.position, out var destino)) return;

            hitable.OnLaunched(destino);

            var routine = StartCoroutine(LaunchRoutine(targetT, destino));
            activeLaunches[targetT] = routine;
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

        private IEnumerator LaunchRoutine(Transform target, Vector3 destino)
        {
            if (target == null) yield break;
            if (target.IsChildOf(transform))
                target.SetParent(null, true);

            Vector3 start = target.position;
            Vector3 startXZ = new Vector3(start.x, 0f, start.z);
            Vector3 endXZ = new Vector3(destino.x, 0f, destino.z);
            Vector3 flat = endXZ - startXZ;
            float distance = flat.magnitude;

            if (distance < minLaunchDistance)
            {
                target.position = destino;
                activeLaunches.Remove(target);
                yield break;
            }

            Vector3 dir = flat / Mathf.Max(distance, 0.0001f);

            float angleRad = launchAngleDeg * Mathf.Deg2Rad;
            float v = Mathf.Max(0.01f, launchSpeed);
            float cos = Mathf.Cos(angleRad);
            float tan = Mathf.Tan(angleRad);
            float g = Mathf.Max(0.01f, launchGravity);

            // Tiempo total horizontal: x(t) = v * cos(a) * t
            float totalTime = distance / (v * cos);

            Rigidbody trb = target.GetComponent<Rigidbody>();
            bool hadRB = trb != null;
            bool prevKinematic = false;
            if (hadRB && kinematicDuringLaunch)
            {
                prevKinematic = trb.isKinematic;
                trb.isKinematic = true;
            }

            float t = 0f;
            while (t < totalTime && target != null)
            {
                t += Time.deltaTime;
                float ct = Mathf.Min(t, totalTime);
                float x = v * cos * ct;
                float yOffset = x * tan - (g * x * x) / (2f * v * v * cos * cos);

                Vector3 pos = start + dir * x;
                pos.y = start.y + yOffset;
                target.position = pos;

                yield return null;
            }

            if (target != null)
                target.position = destino; // snap final

            if (hadRB && kinematicDuringLaunch && trb != null)
                trb.isKinematic = prevKinematic;

            if (target != null)
                activeLaunches.Remove(target);
        }

        private bool TryGetRandomPointInNearestSafeZone(Vector3 from, out Vector3 point)
        {
            point = default;
            GameObject[] zones = GameObject.FindGameObjectsWithTag(safeZoneTag);
            if (zones == null || zones.Length == 0) return false;

            SafeZoneArea bestArea = null;
            Transform fallback = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < zones.Length; i++)
            {
                var go = zones[i];
                if (go == null) continue;

                var area = go.GetComponent<SafeZoneArea>();
                var t = go.transform;
                float d = (t.position - from).sqrMagnitude;

                if (area != null)
                {
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestArea = area;
                        fallback = t;
                    }
                }
                else
                {
                    if (d < bestDist)
                    {
                        bestDist = d;
                        fallback = t;
                    }
                }
            }

            if (bestArea != null)
            {
                point = bestArea.GetRandomPointInside();
                return true;
            }

            if (fallback != null)
            {
                point = fallback.position;
                return true;
            }

            return false;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (interactionPoint == null) interactionPoint = transform;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
        }
    }
}