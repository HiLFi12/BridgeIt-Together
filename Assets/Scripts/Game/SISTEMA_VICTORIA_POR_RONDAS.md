# Sistema de Victoria por Rondas - GameConditionManager

## 📋 Resumen del Sistema

El `GameConditionManager` ahora soporta **dos tipos de victoria**:

### 1. 🎯 Victoria por Conteo (Original)
- **Condición**: X vehículos pasan por el trigger de victoria
- **Uso**: Juegos con flujo continuo de vehículos
- **Configuración**: `usarVictoriaPorRondas = false`

### 2. 🏆 Victoria por Rondas Completadas (Nuevo)
- **Condición**: Todas las rondas del AutoGenerator terminan
- **Uso**: Juegos con niveles/rondas definidas
- **Configuración**: `usarVictoriaPorRondas = true`

---

## 🎛️ Configuración en el Inspector

### Configuración de Victoria por Rondas
```
[Header("Configuración de Victoria por Rondas")]
[SerializeField] private bool usarVictoriaPorRondas = false;
[SerializeField] private AutoGenerator autoGenerator;
```

### Parámetros
- **Usar Victoria por Rondas**: Habilita el sistema de victoria por rondas
- **Auto Generator**: Referencia al AutoGenerator (se busca automáticamente si no se asigna)

---

## 🔧 Funcionamiento Interno

### Sistema de Victoria por Rondas
1. El `AutoGenerator` ejecuta todas sus rondas configuradas
2. Cuando termina la última ronda (sin loop), notifica al `GameConditionManager`
3. El `GameConditionManager` activa automáticamente la victoria
4. Se ejecuta la secuencia de victoria normal

### Integración Automática
- El `GameConditionManager` se conecta automáticamente al `AutoGenerator`
- No requiere configuración manual de eventos
- Valida automáticamente que el sistema de rondas esté habilitado

---

## 💻 API Pública

### Métodos de Configuración
```csharp
// Configurar victoria por rondas
void ConfigurarVictoriaPorRondas(bool habilitar, AutoGenerator generador = null)

// Obtener estado
bool IsUsandoVictoriaPorRondas()
string GetInfoProgresoRondas()

// Notificación (usado por AutoGenerator)
void NotificarTodasLasRondasCompletadas()
```

### Métodos de Información
```csharp
// Obtener información del progreso de rondas
string GetInfoProgresoRondas()
// Retorna: "Ronda X/Y" si está usando victoria por rondas
```

---

## 🎮 Comportamiento por Tipo de Victoria

### Victoria por Conteo
```csharp
// Cada vehículo que pasa cuenta hacia la victoria
// Cuando se alcanza el número objetivo -> Victoria inmediata
public void OnVehiculoPasaPuente(GameObject vehiculo)
{
    contadorVictoria++;
    if (contadorVictoria >= vehiculosParaVictoria)
    {
        Victoria(); // ¡Victoria inmediata!
    }
}
```

### Victoria por Rondas
```csharp
// Los vehículos que pasan NO activan victoria
// Solo cuenta estadísticas, la victoria viene del AutoGenerator
public void OnVehiculoPasaPuente(GameObject vehiculo)
{
    contadorVictoria++; // Solo estadísticas
    // NO verifica victoria - espera notificación de rondas
}
```

---

## 🚀 Casos de Uso

### Juego de Niveles Progresivos
```csharp
// Configurar para que la victoria sea completar todas las rondas
gameConditionManager.ConfigurarVictoriaPorRondas(true, autoGenerator);

// Las rondas del AutoGenerator definen el progreso
autoGenerator.ConfigurarRondas(new RondaConfig[]
{
    new RondaConfig { nombreRonda = "Fácil", cantidadAutos = 3, tiempoEntreAutos = 8f },
    new RondaConfig { nombreRonda = "Normal", cantidadAutos = 5, tiempoEntreAutos = 6f },
    new RondaConfig { nombreRonda = "Difícil", cantidadAutos = 8, tiempoEntreAutos = 4f }
});
```

### Juego de Supervivencia
```csharp
// Configurar para que la victoria sea por conteo
gameConditionManager.ConfigurarVictoriaPorRondas(false);
gameConditionManager.ConfigurarCondiciones(50, 3); // 50 pasan = Victoria, 3 caen = Derrota
```

---

## 🔄 Compatibilidad

### Retrocompatibilidad
- **100% compatible** con el sistema anterior
- El comportamiento por defecto es `usarVictoriaPorRondas = false`
- No requiere cambios en código existente

### Coexistencia
- **Derrota**: Siempre funciona igual (X vehículos caen)
- **Victoria**: Depende del modo configurado
- **Estadísticas**: Se mantienen en ambos modos

---

## 🐛 Debugging y Diagnóstico

### Context Menu
```csharp
[ContextMenu("Simular Fin de Rondas")]
public void SimularFinDeRondas()

[ContextMenu("Mostrar Estado Actual")]
public void MostrarEstadoActual()
```

### Logs del Sistema
```
[GameConditionManager] Sistema de victoria por rondas habilitado. Total de rondas: 3
[AutoGenerator] Todas las rondas completadas. Deteniendo generación.
[AutoGenerator] Notificando al GameConditionManager que todas las rondas han terminado.
[GameConditionManager] 🎉 Todas las rondas completadas! Activando victoria por rondas.
🎉 ¡VICTORIA POR RONDAS! ¡Has completado todas las rondas configuradas!
```

### Estado del Sistema
```csharp
// Información disponible
Debug.Log($"Victoria por Rondas: {gameConditionManager.IsUsandoVictoriaPorRondas()}");
Debug.Log($"Progreso: {gameConditionManager.GetInfoProgresoRondas()}");
Debug.Log($"Rondas AutoGenerator: {autoGenerator.GetRondaActual()}/{autoGenerator.GetTotalRondas()}");
```

---

## ⚠️ Consideraciones Importantes

### Configuración Requerida
1. **AutoGenerator**: Debe tener `usarSistemaRondas = true`
2. **GameConditionManager**: Debe tener `usarVictoriaPorRondas = true`
3. **Rondas**: Debe haber al menos una ronda configurada
4. **Loop**: Debe estar `loopearRondas = false` para que termine

### Validaciones Automáticas
- El sistema verifica que el AutoGenerator tenga rondas habilitadas
- Si no se encuentra AutoGenerator, se desactiva automáticamente
- Se muestran warnings en consola si hay problemas de configuración

### Orden de Ejecución
1. `AutoGenerator` completa todas sus rondas
2. `AutoGenerator` llama a `NotificarTodasLasRondasCompletadas()`
3. `GameConditionManager` ejecuta `VictoriaPorRondas()`
4. Se activa el evento `OnVictoria`

---

## 📊 Ejemplo de Integración Completa

```csharp
// Configurar AutoGenerator con 3 rondas
autoGenerator.ConfigurarRondas(new RondaConfig[]
{
    new RondaConfig { nombreRonda = "Introducción", cantidadAutos = 2, tiempoEntreAutos = 10f },
    new RondaConfig { nombreRonda = "Prueba", cantidadAutos = 4, tiempoEntreAutos = 8f },
    new RondaConfig { nombreRonda = "Desafío", cantidadAutos = 6, tiempoEntreAutos = 5f }
});

// Configurar GameConditionManager para victoria por rondas
gameConditionManager.ConfigurarVictoriaPorRondas(true, autoGenerator);

// Configurar derrota normal
gameConditionManager.ConfigurarCondiciones(0, 3); // 0 para victoria (no usado), 3 para derrota

// ¡El juego ahora termina en victoria cuando se completan las 3 rondas!
// La derrota sigue siendo si 3 vehículos caen
```

---

## 🎯 Ventajas del Sistema

### Para Diseñadores
- **Control preciso** sobre la duración del nivel
- **Progresión clara** con rondas definidas
- **Flexibilidad** para combinar ambos sistemas

### Para Jugadores
- **Progreso visible** (Ronda X/Y)
- **Objetivos claros** (completar rondas vs. supervivencia)
- **Variedad de gameplay** entre niveles

### Para Desarrolladores
- **Separación clara** de responsabilidades
- **Fácil testing** con Context Menu
- **Logs detallados** para debugging
