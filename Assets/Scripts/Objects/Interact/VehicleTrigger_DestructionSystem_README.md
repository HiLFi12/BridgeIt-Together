# Sistema de Triggers con Destrucción de Objetos No-Vehículos

## Descripción General

El sistema de triggers ha sido modificado para manejar dos tipos de objetos:
1. **Vehículos** (con componentes `AutoMovement` o `VehicleBridgeCollision`) → Se envían al pool
2. **Objetos no-vehículos** → Se destruyen automáticamente

## Archivos Modificados

### 1. VehicleReturnTrigger.cs (Modificado)
**Funcionalidad Nueva:**
- Detección automática de objetos no-vehículos
- Destrucción configurable de objetos no-vehículos
- Sistema de protección para objetos importantes
- Configuración desde Inspector y código

**Campos Configurables:**
- `destroyNonVehicles`: Activa/desactiva la destrucción
- `showDebugMessages`: Muestra mensajes de debug
- `additionalProtectedTags`: Tags adicionales que no se destruirán

### 2. VehicleTriggerConfigurator.cs (Nuevo)
**Funcionalidad:**
- Configuración masiva de múltiples triggers
- Búsqueda automática de triggers en la escena
- Gestión global de tags protegidos
- Herramientas de testing y debug

## Cómo Usar

### Configuración Individual (Por Trigger)

1. **En el Inspector:**
   ```
   - Selecciona el GameObject con VehicleReturnTrigger
   - Configura "Destroy Non Vehicles" (true/false)
   - Configura "Show Debug Messages" (true/false)
   - Añade tags protegidos en "Additional Protected Tags"
   ```

2. **Por Código:**
   ```csharp
   VehicleReturnTrigger trigger = GetComponent<VehicleReturnTrigger>();
   
   // Activar/desactivar destrucción
   trigger.SetDestroyNonVehicles(true);
   
   // Configurar debug
   trigger.SetDebugMessages(true);
   
   // Añadir tags protegidos
   trigger.AddProtectedTags(new string[] { "Collectible", "PowerUp" });
   ```

### Configuración Masiva (Múltiples Triggers)

1. **Añadir VehicleTriggerConfigurator:**
   ```
   - Crea un GameObject vacío
   - Añade el componente VehicleTriggerConfigurator
   - Configurar opciones globales en el Inspector
   ```

2. **Configuración Global:**
   ```csharp
   VehicleTriggerConfigurator configurator = GetComponent<VehicleTriggerConfigurator>();
   
   // Buscar todos los triggers
   configurator.FindAllTriggers();
   
   // Aplicar configuración a todos
   configurator.ApplyConfigurationToAllTriggers();
   
   // Activar/desactivar destrucción en todos
   configurator.EnableDestructionOnAll();
   configurator.DisableDestructionOnAll();
   
   // Añadir tag protegido global
   configurator.AddGlobalProtectedTag("Collectible");
   ```

## Objetos Protegidos (No se Destruyen)

### Tags Protegidos por Defecto:
- `Player` - Jugadores
- `MainCamera` - Cámaras principales
- `GameController` - Controladores de juego
- `UI` - Elementos de interfaz
- `BridgeQuadrant` - Cuadrantes del puente
- `Ground` - Suelo/terreno
- `Platform` - Plataformas
- `Respawn` - Puntos de respawn
- `Finish` - Líneas de meta
- `EditorOnly` - Objetos solo del editor

### Componentes Protegidos:
- `Camera` - Cámaras
- `Light` - Luces
- `AudioListener` - Listeners de audio
- `Canvas` - Canvas de UI (incluye hijos)
- `VehicleReturnTrigger` - Otros triggers
- `VehicleReturnTriggerManager` - Managers de triggers
- `BridgeConstructionGrid` - Sistema de puentes

## Ejemplos de Uso

### Escenario 1: Nivel Normal
```csharp
// Destruir solo objetos específicos, proteger coleccionables
VehicleReturnTrigger trigger = GetComponent<VehicleReturnTrigger>();
trigger.SetDestroyNonVehicles(true);
trigger.AddProtectedTags(new string[] { "Collectible", "PowerUp", "Coin" });
```

### Escenario 2: Modo Sandbox (Sin Destrucción)
```csharp
// Desactivar destrucción completamente
VehicleTriggerConfigurator configurator = GetComponent<VehicleTriggerConfigurator>();
configurator.DisableDestructionOnAll();
```

### Escenario 3: Nivel de Limpieza
```csharp
// Solo proteger elementos esenciales
VehicleReturnTrigger trigger = GetComponent<VehicleReturnTrigger>();
trigger.SetDestroyNonVehicles(true);
// No añadir tags adicionales = solo protecciones básicas
```

## Mensajes de Debug

Con `showDebugMessages = true`, verás:

```
🗑️ Destruyendo objeto no-vehículo: CubeTest (Tag: Untagged)
⚠️ Objeto Player1 (Tag: Player) protegido - no se destruye
ℹ️ Objeto Rock (Tag: Untagged) ignorado (destrucción deshabilitada)
```

## Métodos Útiles del Configurador

### Métodos de Context Menu (Inspector):
- `Buscar Todos los Triggers` - Encuentra automáticamente triggers
- `Aplicar Configuración a Todos` - Aplica configuración global
- `Activar Destrucción en Todos` - Activa destrucción masivamente
- `Desactivar Destrucción en Todos` - Desactiva destrucción masivamente
- `Mostrar Estadísticas` - Muestra información de triggers
- `Test: Crear Objeto de Prueba` - Crea cubo de prueba para testing

### Métodos Programáticos:
```csharp
configurator.FindAllTriggers();
configurator.ApplyConfigurationToAllTriggers();
configurator.EnableDestructionOnAll();
configurator.DisableDestructionOnAll();
configurator.SetDebugMessagesForAll(true);
configurator.AddGlobalProtectedTag("NewProtectedTag");
configurator.ShowStatistics();
```

## Consideraciones de Rendimiento

- La destrucción es instantánea con `Destroy()`
- Las verificaciones de componentes son eficientes
- El sistema solo procesa objetos que entran al trigger
- Los tags protegidos se verifican por orden de importancia

## Solución de Problemas

### Objetos importantes se están destruyendo:
1. Verificar que tienen el tag correcto
2. Añadir el tag a `additionalProtectedTags`
3. Verificar que tienen componentes protegidos

### La destrucción no funciona:
1. Verificar que `destroyNonVehicles = true`
2. Verificar que el trigger está activo
3. Verificar que el objeto no está protegido

### Muchos mensajes de debug:
1. Establecer `showDebugMessages = false`
2. Usar `SetDebugMessagesForAll(false)` para todos los triggers

## Integración con Sistemas Existentes

Este sistema es compatible con:
- Sistema de pooling de vehículos existente
- AutoGenerator y VehicleReturnTriggerManager
- Sistema de puentes y colisiones
- Otros sistemas que usen triggers

No interfiere con:
- Mecánicas de juego existentes
- Sistema de respawn de jugadores
- UI y cámaras
- Sistema de niveles
