using System.Collections;
using UnityEngine;
using BridgeItTogether.Gameplay.Carriles;
using BridgeItTogether.Gameplay.Rondas;
using BridgeItTogether.Gameplay.Abstractions;
using BridgeItTogether.Gameplay.AutoControllers;

namespace BridgeItTogether.Gameplay.Spawning
{
    /// <summary>
    /// Componente responsable de spawnear vehículos, delegando pool y colisiones a otros componentes.
    /// Puede operar en modo continuo o controlado por rondas a través de RoundController.
    /// </summary>
    [DisallowMultipleComponent]
    public class VehicleSpawner : MonoBehaviour
    {
    [Header("Prefabs (Auto1..Auto5)")]
    [SerializeField] private GameObject[] autoPrefabs = new GameObject[5];

        [Header("Puntos de Spawn")]
        [SerializeField] private Transform puntoSpawnIzquierdo;
        [SerializeField] private Transform puntoSpawnDerecho;
        [SerializeField] private bool iniciarDesdeIzquierda = true;

        [Header("Carriles")]
        [SerializeField] private float separacionCarriles = 12f;
        [SerializeField] private TipoCarril tipoCarril = TipoCarril.DobleCarril;

        [Header("Pool")]
        [SerializeField] private int poolSize = 10;
        [SerializeField] private bool poolExpandible = true;

        [Header("Modo continuo")]
        [SerializeField] private bool modoContinuo = true;
        [SerializeField] private float tiempoEntreAutos = 10f;

        [Header("Debug")]
        [SerializeField] private bool mostrarDebugInfo = true;

        [Header("Referencias")]
        [SerializeField] private BridgeConstructionGrid bridgeGrid;
        [SerializeField] private RoundController roundController; // opcional
    [Header("Retorno a Pool")]
    [SerializeField] private Collider[] returnTriggers; // triggers de retorno
    [SerializeField] private bool activarTriggersAlIniciar = true;


    private BridgeItTogether.Gameplay.Abstractions.IVehiclePoolService vehiclePool;
        private bool spawnDesdeIzquierda;

        private void Awake()
        {
            vehiclePool = GetComponent<BridgeItTogether.Gameplay.Abstractions.IVehiclePoolService>();
            if (vehiclePool == null)
            {
                // agregar adaptador si no existe
                var adapter = GetComponent<VehiclePoolAdapter>();
                if (adapter == null) adapter = gameObject.AddComponent<VehiclePoolAdapter>();
                vehiclePool = adapter;
            }
        }

        private void Start()
        {
            spawnDesdeIzquierda = iniciarDesdeIzquierda;
            SetupSpawnPoints();
            SetupBridgeGrid();
            InitializeVehiclePool();

            if (modoContinuo)
                StartCoroutine(GenerarAutosContinuo());

            if (roundController == null) roundController = GetComponent<RoundController>();

            // Configurar sistema de retorno a pool si hay triggers
            if (returnTriggers != null && returnTriggers.Length > 0)
            {
                var triggerManager = GetComponent<VehicleReturnTriggerManager>();
                if (triggerManager == null) triggerManager = gameObject.AddComponent<VehicleReturnTriggerManager>();
                triggerManager.Initialize(vehiclePool, returnTriggers, activarTriggersAlIniciar);
            }
        }

        private void SetupSpawnPoints()
        {
            if (puntoSpawnIzquierdo == null) puntoSpawnIzquierdo = transform;
            if (puntoSpawnDerecho == null) puntoSpawnDerecho = transform;
        }

        private void SetupBridgeGrid()
        {
            if (bridgeGrid == null)
            {
                bridgeGrid = FindFirstObjectByType<BridgeConstructionGrid>();
            }
        }

        private void InitializeVehiclePool()
        {
            var defaultPrefab = ObtenerPrimerPrefabDisponible();
            if (defaultPrefab == null)
            {
                Debug.LogError("VehicleSpawner: No hay prefabs asignados en 'autoPrefabs'. Asigna al menos Auto1.");
                return;
            }
            vehiclePool.Initialize(defaultPrefab, poolSize, poolExpandible, bridgeGrid);
        }

        private IEnumerator GenerarAutosContinuo()
        {
            while (true)
            {
                yield return new WaitForSeconds(tiempoEntreAutos);
                SpawnVehicle(TipoVehiculo.Auto1, ObtenerTipoCarrilActual(), spawnDesdeIzquierda);
                spawnDesdeIzquierda = !spawnDesdeIzquierda;
            }
        }

        public GameObject SpawnVehicle(TipoVehiculo tipoVehiculo, TipoCarril tipoCarrilUso, bool desdeIzquierda, PosicionCarril? carrilIndividual = null)
        {
            var prefab = ObtenerPrefabSegunTipo(tipoVehiculo);
            var auto = vehiclePool.GetVehicleFromPool(prefab);
            if (auto == null) return null;

            // Renombrar la instancia según el prefab elegido (soporta Random)
            int idx = PrefabIndex(prefab);
            if (idx >= 0) auto.name = $"Auto{idx + 1}";
            else auto.name = ObtenerNombreParaTipo(tipoVehiculo);

            ConfigurarPosicionAuto(auto, tipoCarrilUso, desdeIzquierda, carrilIndividual);
            auto.SetActive(true);
            // Asegurar notificador
            var notifier = auto.GetComponent<VehicleReturnNotifier>();
            if (notifier == null) notifier = auto.AddComponent<VehicleReturnNotifier>();
            notifier.roundController = roundController;

            ConfigurarMovimientoAuto(auto, tipoVehiculo, desdeIzquierda);
            ConfigurarColisionPuente(auto);
            return auto;
        }

        private void ConfigurarPosicionAuto(GameObject auto, TipoCarril tipoActual, bool desdeIzquierda, PosicionCarril? carrilIndividual)
        {
            var puntoSpawn = desdeIzquierda ? puntoSpawnIzquierdo : puntoSpawnDerecho;
            var posicionSpawn = puntoSpawn.position;

            if (tipoActual == TipoCarril.DobleCarril)
            {
                float offset = desdeIzquierda ? separacionCarriles / 2f : -separacionCarriles / 2f;
                posicionSpawn += new Vector3(0, 0, offset);
            }
            else if (tipoActual == TipoCarril.SoloIzquierda || tipoActual == TipoCarril.SoloDerecha)
            {
                var pos = carrilIndividual ?? PosicionCarril.Inferior;
                float offset = (pos == PosicionCarril.Superior) ? separacionCarriles / 2f : -separacionCarriles / 2f;
                posicionSpawn += new Vector3(0, 0, offset);
            }
            auto.transform.position = posicionSpawn;
            auto.transform.rotation = Quaternion.LookRotation(Vector3.right);

            var rb = auto.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private void ConfigurarMovimientoAuto(GameObject auto, TipoVehiculo tipo, bool desdeIzquierda)
        {
            var direccion = desdeIzquierda ? Vector3.left : Vector3.right;

            // Preferir AutoController si está presente
            var controller = auto.GetComponent<AutoController>();
            if (controller != null)
            {
                controller.Initialize(direccion);
                return;
            }

            // Fallback al comportamiento anterior si no hay controller
            var mov = auto.GetComponent<AutoMovement>();
            if (mov == null) { Debug.LogWarning("VehicleSpawner: AutoMovement no encontrado"); return; }
            mov.SetDireccionMovimiento(direccion);
            mov.AplicarCorreccionAutomatica();
            mov.enabled = true;
        }

        private void ConfigurarColisionPuente(GameObject auto)
        {
            // Preferir el controller para configurar grid
            var controller = auto.GetComponent<AutoController>();
            if (controller != null)
            {
                //if (bridgeGrid != null)
                //    controller.SetBridgeGrid(bridgeGrid);
                // Aviso de tag Vehicle si no coincide (AutoController puede forzarlo)
                if (!auto.CompareTag("Vehicle"))
                {
                    Debug.LogWarning($"VehicleSpawner: El vehículo {auto.name} no tiene tag 'Vehicle'. VehicleBridgeCollision requiere ese tag para validar colisiones.");
                }
            }
            else
            {
                var bridgeCol = auto.GetComponent<VehicleBridgeCollision>();
                if (bridgeCol != null && bridgeGrid != null)
                    bridgeCol.bridgeGrid = bridgeGrid;
                // Si no hay AutoController ni VehicleBridgeCollision, no habrá daño al puente
                if (bridgeCol == null)
                    Debug.LogWarning($"VehicleSpawner: {auto.name} no tiene AutoController ni VehicleBridgeCollision. No dañará el puente.");
            }
        }

        private GameObject ObtenerPrefabSegunTipo(TipoVehiculo tipo)
        {
            if (tipo == TipoVehiculo.Random)
            {
                int count = CountPrefabsDisponibles();
                int idx = UnityEngine.Random.Range(0, Mathf.Max(count, 1));
                return ObtenerPrefabPorIndice(idx) ?? ObtenerPrimerPrefabDisponible();
            }

            int index = TipoToIndex(tipo);
            var p = ObtenerPrefabPorIndice(index);
            if (p != null) return p;

            // Fallback: primer prefab disponible
            return ObtenerPrimerPrefabDisponible();
        }

        public TipoCarril ObtenerTipoCarrilActual() => tipoCarril;

        // Expuestos para configuración desde RoundController
        public void SetTipoCarril(TipoCarril t) => tipoCarril = t;
        public void SetTiempoEntreAutos(float t) => tiempoEntreAutos = t;
        public void SetModoContinuo(bool enabled) => modoContinuo = enabled;

        private int TipoToIndex(TipoVehiculo tipo)
        {
            switch (tipo)
            {
                case TipoVehiculo.Auto1: return 0;
                case TipoVehiculo.Auto2: return 1;
                case TipoVehiculo.Auto3: return 2;
                case TipoVehiculo.Auto4: return 3;
                case TipoVehiculo.Auto5: return 4;
                default: return 0;
            }
        }

        private GameObject ObtenerPrefabPorIndice(int index)
        {
            if (autoPrefabs != null && index >= 0 && index < autoPrefabs.Length)
                return autoPrefabs[index];
            return null;
        }

        private int CountPrefabsDisponibles()
        {
            if (autoPrefabs == null) return 0;
            int c = 0;
            for (int i = 0; i < autoPrefabs.Length; i++) if (autoPrefabs[i] != null) c++;
            return Mathf.Max(1, c);
        }

        private GameObject ObtenerPrimerPrefabDisponible()
        {
            if (autoPrefabs != null)
            {
                for (int i = 0; i < autoPrefabs.Length; i++) if (autoPrefabs[i] != null) return autoPrefabs[i];
            }
            return null;
        }

        private string ObtenerNombreParaTipo(TipoVehiculo tipo)
        {
            if (tipo == TipoVehiculo.Random)
            {
                // Si llega Random, nombramos según el índice elegido al obtener prefab
                // Pero como no lo guardamos, asumimos Auto1
                return "Auto1";
            }
            int index = TipoToIndex(tipo) + 1;
            return $"Auto{index}";
        }

        private int PrefabIndex(GameObject prefab)
        {
            if (prefab == null) return -1;
            if (autoPrefabs != null)
            {
                for (int i = 0; i < autoPrefabs.Length; i++)
                    if (autoPrefabs[i] == prefab) return i;
            }
            return -1;
        }
    }
}
