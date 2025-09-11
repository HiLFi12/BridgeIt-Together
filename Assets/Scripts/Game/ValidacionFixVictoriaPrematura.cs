using UnityEngine;
using System.Collections;
using BridgeItTogether.Gameplay.Rondas;

/// <summary>
/// Script de validación final para verificar que el bug de victoria prematura está corregido
/// </summary>
public class ValidacionFixVictoriaPrematura : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameConditionManager gameConditionManager;
    [SerializeField] private RoundController roundController;
    
    private void Start()
    {
        // Auto-encontrar referencias
        if (gameConditionManager == null)
            gameConditionManager = FindFirstObjectByType<GameConditionManager>();
        
        if (roundController == null)
            roundController = FindFirstObjectByType<RoundController>();
    }
    
    [ContextMenu("🧪 Test Fix Victoria Prematura")]
    public void TestFixVictoriaPrematura()
    {
        Debug.Log("=== INICIANDO TEST DE VALIDACIÓN ===");
        
    if (gameConditionManager == null || roundController == null)
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
        BridgeItTogether.Gameplay.Rondas.RondaConfig[] rondasTest = new BridgeItTogether.Gameplay.Rondas.RondaConfig[]
        {
            new BridgeItTogether.Gameplay.Rondas.RondaConfig 
            { 
                nombreRonda = "Test Ronda A", 
                cantidadAutos = 3,  // 3 vehículos
                tiempoEntreAutos = 2f // Spawn cada 2 segundos
            },
            new BridgeItTogether.Gameplay.Rondas.RondaConfig 
            { 
                nombreRonda = "Test Ronda B", 
                cantidadAutos = 2,  // 2 vehículos
                tiempoEntreAutos = 3f // Spawn cada 3 segundos
            }
        };
        
    // Configurar RoundController
    roundController.SetRondas(rondasTest, reiniciar: true);
        
    // Configurar GameConditionManager
    gameConditionManager.ConfigurarVictoriaPorRondas(true, roundController);
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
            int rondaActual = roundController.GetRondaActual();
            int contadorVictoria = gameConditionManager.GetProgresoVictoria();
            bool juegoTerminado = gameConditionManager.IsJuegoTerminado();
            
            // Verificar si cambió la ronda
            if (rondaActual != ultimaRonda)
            {
                ultimaRonda = rondaActual;
                Debug.Log($"📋 Ronda cambiada a: {rondaActual}/{roundController.GetTotalRondas()}");
            }
            
            // Verificar si cambió el contador de victoria
            if (contadorVictoria != ultimoContadorVictoria)
            {
                ultimoContadorVictoria = contadorVictoria;
                Debug.Log($"📊 Contador de victoria: {contadorVictoria}");
                
                // VERIFICACIÓN CRÍTICA: La victoria NO debe activarse hasta que todas las rondas terminen
                if (juegoTerminado && rondaActual < roundController.GetTotalRondas())
                {
                    Debug.LogError("❌ BUG DETECTADO: Victoria activada prematuramente!");
                    Debug.LogError($"   - Ronda actual: {rondaActual}/{roundController.GetTotalRondas()}");
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
                
                if (rondaActual >= roundController.GetTotalRondas())
                {
                    Debug.Log("✅ VICTORIA CORRECTA: Todas las rondas completadas!");
                    Debug.Log($"   - Rondas completadas: {roundController.GetTotalRondas()}");
                    Debug.Log($"   - Vehículos que pasaron: {contadorVictoria}");
                    Debug.Log($"   - Tiempo total: {Time.time - tiempoInicio:F1} segundos");
                    testCompletado = true;
                }
                else
                {
                    Debug.LogError("❌ VICTORIA PREMATURA DETECTADA!");
                    Debug.LogError($"   - Ronda actual: {rondaActual}/{roundController.GetTotalRondas()}");
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
    // No hay ClearActiveAutos en RoundController; sólo reiniciamos condiciones
        gameConditionManager.ReiniciarJuego();
        Debug.Log("🧹 Sistema limpiado y reseteado");
    }
    
    [ContextMenu("📊 Mostrar Estado Actual")]
    public void MostrarEstadoActual()
    {
        Debug.Log("=== ESTADO ACTUAL DEL SISTEMA ===");
    Debug.Log($"RoundController:");
    Debug.Log($"   - Ronda: {roundController.GetRondaActual()}/{roundController.GetTotalRondas()}");
    Debug.Log($"   - Sistema activo: {roundController.IsUsandoSistemaRondas()}");
          Debug.Log($"GameConditionManager:");
        Debug.Log($"   - Victoria por rondas: {gameConditionManager.IsUsandoVictoriaPorRondas()}");
        Debug.Log($"   - Contador victoria: {gameConditionManager.GetProgresoVictoria()}");
        Debug.Log($"   - Juego terminado: {gameConditionManager.IsJuegoTerminado()}");
        
    // Información de pool omitida: el pooling ahora se administra vía servicios del spawner
        
        Debug.Log("=== FIN ESTADO ===");
    }
}
