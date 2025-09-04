using System.Collections;
using UnityEngine;
using BridgeItTogether.Gameplay.Carriles;
using BridgeItTogether.Gameplay.Spawning;

namespace BridgeItTogether.Gameplay.Rondas
{
    /// <summary>
    /// Controla la lógica de rondas: decide qué vehículo, cada cuánto, y cuándo cambia de ronda.
    /// Depende de un VehicleSpawner para realizar el spawn real.
    /// </summary>
    [DisallowMultipleComponent]
    public class RoundController : MonoBehaviour
    {
        [Header("Rondas")]
        [SerializeField] private bool usarSistemaRondas = true;
        [SerializeField] private float tiempoEsperaEntreRondas = 3f;
        [SerializeField] private BridgeItTogether.Gameplay.Rondas.RondaConfig[] configuracionRondas = new BridgeItTogether.Gameplay.Rondas.RondaConfig[0];
        [SerializeField] private bool loopearRondas = true;
        [SerializeField] private bool mostrarDebugInfo = true;

        [Header("Dependencias")]
        [SerializeField] private VehicleSpawner spawner;

        private int rondaActual = 0;
        private int autosSpawneadosEnRonda = 0;
        private int autosQueDebenVolverAlPool = 0;
        private bool esperandoFinDeRonda = false;
        private bool esperandoInicioDeRonda = false;
        private Coroutine corrutina;
        private float timeoutRonda = 60f;
        private PosicionCarril ultimoCarrilUsado = PosicionCarril.Inferior;

        private void Reset()
        {
            spawner = GetComponent<VehicleSpawner>();
        }

        private void Start()
        {
            if (!usarSistemaRondas) return;
            if (spawner == null) spawner = GetComponent<VehicleSpawner>();
            if (spawner == null)
            {
                Debug.LogError("RoundController: No se encontró VehicleSpawner en el mismo GameObject");
                enabled = false; return;
            }

            // Asegurar que el spawner no esté en modo continuo para evitar doble spawn
            spawner.SetModoContinuo(false);

            for (int i = 0; i < configuracionRondas.Length; i++)
                configuracionRondas[i]?.ValidarConfiguracionCarriles();

            rondaActual = 0;
            autosSpawneadosEnRonda = 0;
            autosQueDebenVolverAlPool = 0;
            esperandoFinDeRonda = false;
            esperandoInicioDeRonda = true;

            corrutina = StartCoroutine(EjecutarRondas());
        }

        private IEnumerator EjecutarRondas()
        {
            float inicioEspera = 0f;
            bool desdeIzquierda = true;

            while (true)
            {
                if (esperandoInicioDeRonda)
                {
                    if (tiempoEsperaEntreRondas > 0)
                        yield return new WaitForSeconds(tiempoEsperaEntreRondas);
                    esperandoInicioDeRonda = false;
                }

                if (!esperandoFinDeRonda && rondaActual < configuracionRondas.Length && autosSpawneadosEnRonda < configuracionRondas[rondaActual].cantidadAutos)
                {
                    BridgeItTogether.Gameplay.Rondas.RondaConfig ronda = configuracionRondas[rondaActual];

                    // Tipo de carril a usar esta ronda
                    var tipoCarrilUso = ronda.sobrescribirTipoCarril ? ronda.tipoCarrilRonda : spawner.ObtenerTipoCarrilActual();
                    BridgeItTogether.Gameplay.Spawning.TipoVehiculo tipoVehiculo = (BridgeItTogether.Gameplay.Spawning.TipoVehiculo)ronda.ObtenerTipoVehiculoParaAuto(autosSpawneadosEnRonda);
                    var posCarril = ronda.ObtenerPosicionCarrilParaAuto(autosSpawneadosEnRonda, ultimoCarrilUsado);
                    ultimoCarrilUsado = posCarril;

                    // Determinar lado de spawn según configuración de carril
                    bool ladoParaSpawn = desdeIzquierda;
                    if (tipoCarrilUso == TipoCarril.SoloIzquierda) ladoParaSpawn = true;
                    else if (tipoCarrilUso == TipoCarril.SoloDerecha) ladoParaSpawn = false;

                    // Spawn
                    spawner.SetTipoCarril(tipoCarrilUso);
                    var go = spawner.SpawnVehicle(tipoVehiculo, tipoCarrilUso, ladoParaSpawn, posCarril);
                    if (go != null)
                    {
                        autosSpawneadosEnRonda++;
                        autosQueDebenVolverAlPool++;
                    }

                    if (autosSpawneadosEnRonda >= ronda.cantidadAutos)
                    {
                        esperandoFinDeRonda = true;
                        inicioEspera = Time.time;
                        if (mostrarDebugInfo)
                            Debug.Log($"[RoundController] ⏳ Todos los autos spawneados para {ronda.nombreRonda}. Esperando retorno de: {autosQueDebenVolverAlPool}");
                    }

                    yield return new WaitForSeconds(ronda.tiempoEntreAutos);
                }
                else if (esperandoFinDeRonda)
                {
                    // Timeout de seguridad
                    if (Time.time - inicioEspera > timeoutRonda)
                    {
                        autosQueDebenVolverAlPool = 0;
                        AvanzarRonda();
                    }
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    yield return new WaitForSeconds(0.5f);
                }

                // Alternar origen solo en doble carril
                if (spawner.ObtenerTipoCarrilActual() == TipoCarril.DobleCarril)
                    desdeIzquierda = !desdeIzquierda;
            }
        }

        public void NotificarAutoDevueltoAlPool(GameObject vehiculo)
        {
            if (usarSistemaRondas && esperandoFinDeRonda)
            {
                autosQueDebenVolverAlPool--;
                if (mostrarDebugInfo)
                    Debug.Log($"[RoundController] Vehículo {vehiculo.name} devuelto. Quedan: {autosQueDebenVolverAlPool}");
                if (autosQueDebenVolverAlPool <= 0)
                    AvanzarRonda();
            }
        }

        private void AvanzarRonda()
        {
            if (configuracionRondas == null || configuracionRondas.Length == 0) return;

            if (mostrarDebugInfo)
                Debug.Log($"[RoundController] Completando ronda {rondaActual}: {configuracionRondas[rondaActual].nombreRonda}");

            rondaActual++;
            if (rondaActual >= configuracionRondas.Length)
            {
                if (loopearRondas) rondaActual = 0; else { StopCoroutine(corrutina); return; }
            }

            autosSpawneadosEnRonda = 0;
            autosQueDebenVolverAlPool = 0;
            esperandoFinDeRonda = false;
            esperandoInicioDeRonda = true;
            ultimoCarrilUsado = PosicionCarril.Inferior;
        }

        // API pública útil
        public bool IsUsandoSistemaRondas() => usarSistemaRondas;
        public int GetRondaActual() => rondaActual;
        public int GetTotalRondas() => configuracionRondas?.Length ?? 0;
        public int GetTotalVehiclesForLevel()
        {
            if (configuracionRondas == null) return 0;
            int total = 0; foreach (var r in configuracionRondas) total += (r?.cantidadAutos ?? 0); return total;
        }

        public void SetRondas(RondaConfig[] rondas, bool reiniciar = true)
        {
            configuracionRondas = rondas;
            if (reiniciar && gameObject.activeInHierarchy)
            {
                if (corrutina != null) StopCoroutine(corrutina);
                Start();
            }
        }
    }
}
