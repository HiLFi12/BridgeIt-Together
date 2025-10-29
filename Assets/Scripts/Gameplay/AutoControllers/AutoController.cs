using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;
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
        private readonly Dictionary<Transform, Coroutine> activeLaunches = new();

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

                if (layers != null && layers.Length >= 2)
                {
                    bool layer0Done = layers[0].isCompleted;
                    bool layer1Done = layers[1].isCompleted;
                    bool layer2Done = (layers.Length > 2) ? layers[2].isCompleted : false;

                    // Only destroy when layer0 and layer1 are built and layer2 is NOT built
                    shouldDestroy = layer0Done && layer1Done && !layer2Done;
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

                // If conditions not met (e.g. layer2 already completed), do not destroy and allow normal pass-through
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
                target.position = destino;

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
    }
}