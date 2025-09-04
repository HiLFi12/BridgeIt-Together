# Sistema de Canvas de Fin de Juego - GameConditionManager

## 📋 Resumen de Funcionalidades

Se ha implementado un sistema completo para manejar el fin de juego que incluye:

1. **Desactivación automática del AutoGenerator**
2. **Desactivación automática del PlayerController**
3. **Activación automática de Canvas de Victoria/Derrota**
4. **Sistema de reinicio que reactiva todos los componentes**
5. **Animaciones de Victoria y Derrota para personajes** ← NUEVO

---

## 🎯 Características Principales

### ✅ Control de Sistemas de Juego
- **AutoGenerator**: Se desactiva automáticamente cuando el juego termina
- **PlayerController**: Se desactivan todos los PlayerController en la escena
- **Reactivación**: Al reiniciar el juego, todos los sistemas se reactivan automáticamente

### 🎭 Sistema de Animaciones de Fin de Juego ← NUEVO
- **Animaciones de Victoria**: Se activan automáticamente en todos los PlayerAnimator al ganar
- **Animaciones de Derrota**: Se activan automáticamente en todos los PlayerAnimator al perder
- **Bloqueo de movimiento**: Las animaciones normales se bloquean durante el fin de juego
- **Reinicio automático**: Las animaciones vuelven al estado normal al reiniciar

### 🖼️ Canvas de Fin de Juego
- **Canvas de Victoria**: Se instancia automáticamente cuando se gana
- **Canvas de Derrota**: Se instancia automáticamente cuando se pierde
- **Gestión automática**: Los canvas anteriores se destruyen al reiniciar

### 🔧 Configuración Flexible
- **Desactivación opcional**: Puedes elegir qué sistemas desactivar
- **Prefabs configurables**: Asigna tus propios canvas prefabs
- **Auto-configuración**: Sistema automático para encontrar prefabs

---

## 🚀 Cómo Usar

### Método 1: Configuración Automática (Recomendado)

1. **Agregar el script helper**:
   ```csharp
   // Agregar GameConditionCanvasSetup a cualquier GameObject en la escena
   GameObject setupObject = new GameObject("GameCondition Setup");
   setupObject.AddComponent<GameConditionCanvasSetup>();
   ```

2. **Configurar en el Inspector**:
   - Asignar `Victory Canvas Prefab` → `Assets/Prefabs/GameConditions/Victory.prefab`
   - Asignar `Defeat Canvas Prefab` → `Assets/Prefabs/GameConditions/Defeat.prefab`
   - Marcar `Configurar Automaticamente Al Iniciar` ✅
   - Marcar `Buscar Prefabs Automaticamente` ✅

### Método 2: Configuración Manual

```csharp
// Obtener referencia al GameConditionManager
GameConditionManager gameManager = GameConditionManager.Instance;

// Cargar los prefabs
GameObject victoryPrefab = Resources.Load<GameObject>("Victory");
GameObject defeatPrefab = Resources.Load<GameObject>("Defeat");

// Configurar canvas prefabs
gameManager.ConfigurarCanvasPrefabs(victoryPrefab, defeatPrefab);

// Configurar opciones de desactivación
gameManager.ConfigurarDesactivacionSistemas(true, true); // Player, AutoGenerator
```

---

## ⚙️ Nuevos Campos en GameConditionManager

### Inspector - Sección "Canvas de Fin de Juego"
- **Victory Canvas Prefab**: Prefab del canvas que se muestra al ganar
- **Defeat Canvas Prefab**: Prefab del canvas que se muestra al perder

### Inspector - Sección "Control de Jugador"
- **Desactivar Player Controller En Fin De Juego**: ✅ Por defecto
- **Desactivar Auto Generator En Fin De Juego**: ✅ Por defecto
- **Activar Animaciones Fin De Juego**: ✅ Por defecto ← NUEVO

---

## 🎭 Parámetros Requeridos en Animator Controller

Para que las animaciones funcionen correctamente, tu Animator Controller debe tener estos parámetros:

### Parámetros Obligatorios para Animaciones de Fin de Juego:
- **TriggerVictory** (Trigger): Activa la animación de victoria
- **TriggerDefeat** (Trigger): Activa la animación de derrota
- **IsGameEnded** (Bool): Indica si el juego ha terminado

### Parámetros Normales de Gameplay:
- **Speed** (Float): Velocidad de movimiento del personaje
- **IsHolding** (Bool): Si está sosteniendo un objeto
- **IsBuilding** (Bool): Si está construyendo
- **TriggerBuild** (Trigger): Animación de construcción
- **TriggerDrop** (Trigger): Animación de soltar objeto

### Estados Recomendados en el Animator:
1. **Idle** → Estado base
2. **Running** → Cuando Speed > 0.1
3. **Victory** → Activado por TriggerVictory
4. **Defeat** → Activado por TriggerDefeat
5. **Building** → Activado por IsBuilding
6. **Holding** → Modificador cuando IsHolding = true

---

## 🔍 Métodos Nuevos Disponibles

### Configuración
```csharp
// Configurar canvas prefabs
gameManager.ConfigurarCanvasPrefabs(victoryPrefab, defeatPrefab);

// Configurar desactivación de sistemas
gameManager.ConfigurarDesactivacionSistemas(bool player, bool autoGen);
```

### Control Manual de Sistemas
```csharp
// Desactivar sistemas manualmente
gameManager.DesactivarSistemasDeJuego();

// Reactivar sistemas manualmente
gameManager.ReactivarSistemasDeJuego();

// Activar canvas específicos
gameManager.ActivarCanvasVictoria();
gameManager.ActivarCanvasDerrota();

// Controlar animaciones específicas ← NUEVO
gameManager.ActivarAnimacionesVictoria();
gameManager.ActivarAnimacionesDerrota();
```

### Control de Animaciones PlayerAnimator ← NUEVO
```csharp
// Obtener referencia al PlayerAnimator
PlayerAnimator playerAnimator = GetComponent<PlayerAnimator>();

// Activar animación de victoria
playerAnimator.TriggerVictoryAnimation();

// Activar animación de derrota
playerAnimator.TriggerDefeatAnimation();

// Reiniciar a estado de gameplay
playerAnimator.ResetToGameplayState();

// Verificar estados
bool gameEnded = playerAnimator.IsGameEnded();
bool inVictory = playerAnimator.IsInVictoryState();
bool inDefeat = playerAnimator.IsInDefeatState();
```

### Testing (Context Menu)
- **Test - Activar Canvas Victoria**
- **Test - Activar Canvas Derrota**
- **Test - Desactivar Sistemas**
- **Test - Reactivar Sistemas**
- **Test - Activar Animación Victoria** ← NUEVO
- **Test - Activar Animación Derrota** ← NUEVO
- **Test - Reiniciar Animaciones** ← NUEVO

---

## 📁 Estructura de Archivos Recomendada

```
Assets/
├── Prefabs/
│   └── GameConditions/
│       ├── Victory.prefab    ← Canvas de victoria
│       └── Defeat.prefab     ← Canvas de derrota
├── Scripts/
│   ├── Game/
│   │   ├── GameConditionManager.cs         ← Script principal (modificado)
│   │   └── GameConditionCanvasSetup.cs     ← Script helper (nuevo)
│   └── Player/
│       └── PlayerAnimator.cs               ← Script de animaciones (modificado)
└── Animations/
    └── Player/
        ├── PlayerAnimatorController.controller ← Animator Controller con parámetros
        ├── VictoryAnimation.anim              ← Animación de victoria
        └── DefeatAnimation.anim               ← Animación de derrota
```

---

## 🧪 Testing y Debugging

### Context Menu en GameConditionManager
```
- Simular Vehículo Pasa
- Simular Vehículo Cae
- Reiniciar Juego
- Mostrar Estado Actual
- Simular Fin de Rondas
- Test - Activar Canvas Victoria    ← NUEVO
- Test - Activar Canvas Derrota     ← NUEVO
- Test - Desactivar Sistemas        ← NUEVO
- Test - Reactivar Sistemas         ← NUEVO
```

### Context Menu en GameConditionCanvasSetup
```
- Configurar GameConditionManager
- Buscar Prefabs Automáticamente
- Test - Simular Victoria
- Test - Simular Derrota
- Test - Mostrar Estado
```

---

## 🐛 Solución de Problemas

### ❓ Los canvas no aparecen
**Solución**: Verificar que los prefabs estén asignados en el Inspector
```csharp
// Verificar en el log
Debug.Log("Victory Prefab: " + (victoryCanvasPrefab != null ? victoryCanvasPrefab.name : "NULL"));
```

### ❓ Los sistemas no se desactivan
**Solución**: Verificar que las opciones de desactivación estén habilitadas
```csharp
// Verificar configuración
Debug.Log($"Desactivar Player: {desactivarPlayerControllerEnFinDeJuego}");
Debug.Log($"Desactivar AutoGen: {desactivarAutoGeneratorEnFinDeJuego}");
```

### ❓ El PlayerController no se encuentra
**Solución**: El sistema busca automáticamente todos los PlayerController al iniciar
```csharp
// Verificar en el log al iniciar
Debug.Log($"PlayerControllers encontrados: {playerControllers?.Length ?? 0}");
```

### ❓ Las animaciones no cambian
**Solución**: Verificar que el Animator Controller tenga los parámetros correctos
```csharp
// Verificar en PlayerAnimator que los parámetros existen
// Los warnings aparecerán en la consola si faltan parámetros
```

### ❓ El personaje sigue corriendo después de ganar/perder
**Solución**: Verificar que `activarAnimacionesFinDeJuego` esté habilitado
```csharp
// Verificar configuración en GameConditionManager
Debug.Log($"Activar Animaciones: {activarAnimacionesFinDeJuego}");
```

### ❓ Las animaciones no se reinician al reiniciar el juego
**Solución**: El sistema automáticamente llama ResetToGameplayState()
```csharp
// Verificar en el log que se ejecuta el reinicio
Debug.Log("🔄 PlayerAnimator(s) reiniciado(s) a estado de gameplay");
```

---

## 🎮 Flujo de Funcionamiento

1. **Inicio del Juego**:
   - Se buscan automáticamente todos los PlayerController
   - Se configura el sistema según los parámetros del Inspector

2. **Durante el Juego**:
   - Los sistemas funcionan normalmente
   - Los triggers de victoria/derrota están activos

3. **Fin del Juego** (Victoria o Derrota):
   - Se desactiva el AutoGenerator (si está configurado)
   - Se desactivan todos los PlayerController (si está configurado)
   - Se activan las animaciones de victoria/derrota en todos los PlayerAnimator ← NUEVO
   - Se instancia y activa el canvas correspondiente
   - Se disparan los eventos OnVictoria/OnDerrota

4. **Reinicio del Juego**:
   - Se reactivan todos los sistemas desactivados
   - Se reinician las animaciones a estado de gameplay ← NUEVO
   - Se destruye el canvas de fin de juego
   - Se resetean todos los contadores
   - El juego vuelve al estado original

---

## ✨ Beneficios del Nuevo Sistema

- 🎯 **Automático**: No requiere código adicional en otros scripts
- 🔧 **Configurable**: Puedes elegir qué desactivar y qué canvas usar
- 🧪 **Testeable**: Métodos de testing integrados
- 🚀 **Fácil de usar**: Setup automático con script helper
- 🔄 **Reversible**: El reinicio restaura todo al estado original
- 📱 **Escalable**: Funciona con múltiples PlayerController automáticamente
- 🎭 **Animaciones integradas**: Control automático de animaciones de victoria/derrota ← NUEVO
- 🎯 **Bloqueo inteligente**: Las animaciones normales se bloquean durante el fin de juego ← NUEVO

¡El sistema está listo para usar! Solo necesitas:
1. Asignar los prefabs de canvas en el Inspector
2. Configurar los parámetros en tu Animator Controller
3. ¡Todo funcionará automáticamente! 🎉

## 🎮 Guía Rápida de Configuración del Animator

### Pasos para configurar las animaciones:

1. **Abrir tu Animator Controller**
2. **Agregar los parámetros requeridos**:
   - `TriggerVictory` (Trigger)
   - `TriggerDefeat` (Trigger) 
   - `IsGameEnded` (Bool)
3. **Crear los estados de animación**:
   - Estado `Victory` con tu animación de victoria
   - Estado `Defeat` con tu animación de derrota
4. **Configurar las transiciones**:
   - Desde `Any State` → `Victory` cuando `TriggerVictory`
   - Desde `Any State` → `Defeat` cuando `TriggerDefeat`
   - Condiciones adicionales: `IsGameEnded == true`
5. **¡Listo!** El sistema manejará todo automáticamente
