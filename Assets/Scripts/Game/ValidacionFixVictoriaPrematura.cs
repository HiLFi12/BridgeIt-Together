using UnityEngine;
using System.Collections;

/// <summary>
/// Script de validación final para verificar que el bug de victoria prematura está corregido
/// </summary>
public class ValidacionFixVictoriaPrematura : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameConditionManager gameConditionManager;
    [SerializeField] private AutoGenerator autoGenerator;
    
    private void Start()
    {
        // Auto-encontrar referencias
        if (gameConditionManager == null)
            gameConditionManager = FindFirstObjectByType<GameConditionManager>();
        
        if (autoGenerator == null)
            autoGenerator = FindFirstObjectByType<AutoGenerator>();
    }
    
    [ContextMenu("🧪 Test Fix Victoria Prematura")]
    public void TestFixVictoriaPrematura()
    {
        Debug.Log("=== INICIANDO TEST DE VALIDACIÓN ===");
        
        if (gameConditionManager == null || autoGenerator == null)
        {
            Debug.LogError("❌ No se encontraron las referencias necesarias");
            return;
        }
        
        // Configurar rondas de test
        ConfigurarRondasDeTest();
        
        // Iniciar monitoreo automático
        StartCoroutine(MonitorearTestAutomatico());
        
        Debug.Log("✅ Test iniciado. Monitoreando sistema...");
    }
    
    private void ConfigurarRondasDeTest()
    {
        Debug.Log("🔧 Configurando rondas de test...");
        
        // Configurar rondas simples y predecibles
        RondaConfig[] rondasTest = new RondaConfig[]
        {
            new RondaConfig 
            { 
                nombreRonda = "Test Ronda A", 
                cantidadAutos = 3,  // 3 vehículos
                tiempoEntreAutos = 2f // Spawn cada 2 segundos
            },
            new RondaConfig 
            { 
                nombreRonda = "Test Ronda B", 
                cantidadAutos = 2,  // 2 vehículos
                tiempoEntreAutos = 3f // Spawn cada 3 segundos
            }
        };
        
        // Configurar AutoGenerator
        autoGenerator.ConfigurarRondas(rondasTest);
        autoGenerator.SetSistemaRondasActivo(true);
        
        // Configurar GameConditionManager
        gameConditionManager.ConfigurarVictoriaPorRondas(true, autoGenerator);
        gameConditionManager.ReiniciarJuego();
          Debug.Log("✅ Configuración completada:");
        Debug.Log($"   - Ronda A: {rondasTest[0].cantidadAutos} vehículos");
        Debug.Log($"   - Ronda B: {rondasTest[1].cantidadAutos} vehículos");
        Debug.Log($"   - Total esperado: {rondasTest[0].cantidadAutos + rondasTest[1].cantidadAutos} vehículos antes de victoria");
    }
    
    private IEnumerator MonitorearTestAutomatico()
    {
        float tiempoInicio = Time.time;
        int ultimaRonda = -1;
        int ultimoContadorVictoria = -1;
        bool testCompletado = false;
        bool victoriaActivada = false;
        
        Debug.Log("🔍 Iniciando monitoreo automático...");
        
        while (!testCompletado && (Time.time - tiempoInicio) < 60f) // Timeout de 60 segundos
        {
            yield return new WaitForSeconds(1f);
              // Obtener estado actual
            int rondaActual = autoGenerator.GetRondaActual();
            int contadorVictoria = gameConditionManager.GetProgresoVictoria();
            bool juegoTerminado = gameConditionManager.IsJuegoTerminado();
            
            // Verificar si cambió la ronda
            if (rondaActual != ultimaRonda)
            {
                ultimaRonda = rondaActual;
                Debug.Log($"📋 Ronda cambiada a: {rondaActual}/{autoGenerator.GetTotalRondas()}");
            }
            
            // Verificar si cambió el contador de victoria
            if (contadorVictoria != ultimoContadorVictoria)
            {
                ultimoContadorVictoria = contadorVictoria;
                Debug.Log($"📊 Contador de victoria: {contadorVictoria}");
                
                // VERIFICACIÓN CRÍTICA: La victoria NO debe activarse hasta que todas las rondas terminen
                if (juegoTerminado && rondaActual < autoGenerator.GetTotalRondas())
                {
                    Debug.LogError("❌ BUG DETECTADO: Victoria activada prematuramente!");
                    Debug.LogError($"   - Ronda actual: {rondaActual}/{autoGenerator.GetTotalRondas()}");
                    Debug.LogError($"   - Contador victoria: {contadorVictoria}");
                    Debug.LogError($"   - Juego terminado: {juegoTerminado}");
                    testCompletado = true;
                    yield break;
                }
            }
            
            // Verificar si la victoria se activó correctamente
            if (juegoTerminado && !victoriaActivada)
            {
                victoriaActivada = true;
                
                if (rondaActual >= autoGenerator.GetTotalRondas())
                {
                    Debug.Log("✅ VICTORIA CORRECTA: Todas las rondas completadas!");
                    Debug.Log($"   - Rondas completadas: {autoGenerator.GetTotalRondas()}");
                    Debug.Log($"   - Vehículos que pasaron: {contadorVictoria}");
                    Debug.Log($"   - Tiempo total: {Time.time - tiempoInicio:F1} segundos");
                    testCompletado = true;
                }
                else
                {
                    Debug.LogError("❌ VICTORIA PREMATURA DETECTADA!");
                    Debug.LogError($"   - Ronda actual: {rondaActual}/{autoGenerator.GetTotalRondas()}");
                    testCompletado = true;
                }
            }
        }
        
        // Verificar timeout
        if (!testCompletado)
        {
            Debug.LogWarning("⏰ Test terminado por timeout. Posible problema de configuración.");
        }
        
        Debug.Log("=== TEST FINALIZADO ===");
    }
    
    [ContextMenu("🧹 Limpiar y Resetear")]
    public void LimpiarYResetear()
    {
        StopAllCoroutines();
        autoGenerator.ClearActiveAutos();
        gameConditionManager.ReiniciarJuego();
        Debug.Log("🧹 Sistema limpiado y reseteado");
    }
    
    [ContextMenu("📊 Mostrar Estado Actual")]
    public void MostrarEstadoActual()
    {
        Debug.Log("=== ESTADO ACTUAL DEL SISTEMA ===");
        Debug.Log($"AutoGenerator:");
        Debug.Log($"   - Ronda: {autoGenerator.GetRondaActual()}/{autoGenerator.GetTotalRondas()}");
        Debug.Log($"   - Info: {autoGenerator.GetInfoRondaActual()}");
        Debug.Log($"   - Sistema activo: {autoGenerator.IsUsandoSistemaRondas()}");
          Debug.Log($"GameConditionManager:");
        Debug.Log($"   - Victoria por rondas: {gameConditionManager.IsUsandoVictoriaPorRondas()}");
        Debug.Log($"   - Contador victoria: {gameConditionManager.GetProgresoVictoria()}");
        Debug.Log($"   - Juego terminado: {gameConditionManager.IsJuegoTerminado()}");
        
        VehiclePool pool = autoGenerator.GetComponent<VehiclePool>();
        if (pool != null)
        {
            Debug.Log($"VehiclePool:");
            Debug.Log($"   - Vehículos activos: {pool.GetActiveVehicleCount()}");
            Debug.Log($"   - Vehículos disponibles: {pool.GetAvailableVehicleCount()}");
        }
        
        Debug.Log("=== FIN ESTADO ===");
    }
}
