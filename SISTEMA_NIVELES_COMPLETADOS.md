# Sistema de Niveles Completados - Instrucciones de Uso

## 📖 Descripción General

Este sistema permite marcar niveles como completados cuando el jugador gana, y mostrar una imagen visual en el selector de niveles para indicar qué niveles ya se completaron. Utiliza `PlayerPrefs` para guardar el progreso de forma persistente.

## 🎯 Componentes del Sistema

### 1. **LevelProgressManager.cs**
- **Ubicación**: `Assets/Scripts/Game/LevelProgressManager.cs`
- **Función**: Gestiona el guardado y lectura del progreso de niveles usando PlayerPrefs
- **Singleton**: Se mantiene entre escenas con `DontDestroyOnLoad`
- **Métodos principales**:
  - `MarkLevelAsCompleted(string levelSceneName)` - Marca un nivel como completado
  - `IsLevelCompleted(string levelSceneName)` - Verifica si un nivel está completado
  - `MarkCurrentLevelAsCompleted()` - Marca el nivel actual como completado
  - `ClearAllLevelProgress()` - Limpia todo el progreso (útil para testing)

### 2. **LevelCompletionMarker.cs**
- **Ubicación**: `Assets/Scripts/Game/LevelCompletionMarker.cs`
- **Función**: Muestra/oculta una imagen de "completado" en los botones de nivel
- **Se agrega a**: Cada botón de nivel en el selector de niveles

### 3. **GameConditionManager.cs** (Modificado)
- **Función añadida**: Guarda automáticamente el progreso cuando el jugador gana
- **Condición**: Solo guarda si el jugador tiene al menos 1 estrella (vida) restante

### 4. **LifeStarsUI.cs** (Modificado)
- **Método añadido**: `GetCurrentLives()` - Retorna las vidas actuales del jugador

## 🚀 Configuración Paso a Paso

### Paso 1: Agregar LevelProgressManager a la Escena del Menú

1. Abre la escena `Menu.unity`
2. Crea un GameObject vacío llamado "LevelProgressManager"
3. Agrégale el componente `LevelProgressManager`
4. Asegúrate de que esté marcado como `DontDestroyOnLoad` (el script lo hace automáticamente)

### Paso 2: Configurar Botones de Nivel

Para cada botón de nivel en el selector de niveles:

1. Selecciona el botón del nivel (ej: "Level1Button", "Level0_M Button")
2. Agrega el componente `LevelCompletionMarker`
3. Configura los campos:
   - **Level Scene Name**: Nombre exacto de la escena (ej: "Level1", "Level0_M", "Level2")
   - **Completion Image**: Imagen que se mostrará cuando el nivel esté completado
   - **Auto Detect Level From Button**: Mantener activado para detectar automáticamente el nombre del nivel

#### Crear la Imagen de Completado

1. En cada botón de nivel, crea una imagen hija:
   - Click derecho en el botón → UI → Image
   - Nombra la imagen como "CompletedImage" o "Star" o "CheckMark"
2. Configura la imagen:
   - Asigna un sprite (estrella, check mark, medalla, etc.)
   - Ajusta el tamaño y posición (generalmente en una esquina del botón)
   - **Importante**: La imagen se mostrará/ocultará automáticamente según el progreso

3. Arrastra la imagen al campo "Completion Image" del componente `LevelCompletionMarker`

### Paso 3: Verificar GameConditionManager

El `GameConditionManager` ya está modificado para guardar el progreso automáticamente. Solo asegúrate de que:

1. Tiene asignado el `LifeStarsUI` en el inspector
2. La configuración de victoria está correcta

## 📝 Nombres de Niveles Soportados

El sistema detecta automáticamente los siguientes patrones de nombres de escenas:

### Prehistóricos
- `Level0`, `Level1`, `Level2`, ... `Level9`
- `Level01`, `Level02`

### Medievales
- `Level0_M`, `Level1_M`, `Level2_M`, ... `Level5_M`

### Industriales
- `Level0_I`, `Level1_I`, etc.

### Contemporáneos
- `Level0_C`, `Level1_C`, etc.

### Futuristas
- `Level0_F`, `Level1_F`, etc.

## 🎮 Flujo de Uso

1. **Durante el Juego**:
   - El jugador completa un nivel
   - `GameConditionManager` detecta la victoria
   - Si tiene al menos 1 estrella, guarda el nivel como completado usando `PlayerPrefs`

2. **En el Selector de Niveles**:
   - Cada botón con `LevelCompletionMarker` verifica si su nivel está completado
   - Muestra la imagen de completado si el nivel fue terminado con éxito

3. **Persistencia**:
   - El progreso se guarda en `PlayerPrefs` (persiste entre sesiones)
   - Key format: `Level_{NombreNivel}_Completed` (ej: `Level_Level1_Completed`)

## 🛠️ Testing y Debug

### Comandos de Debug en LevelProgressManager

En el Inspector, con el `LevelProgressManager` seleccionado, puedes usar estos comandos:

1. **Show Level Progress**: Muestra en la consola qué niveles están completados
2. **Complete All Levels (Debug)**: Marca todos los niveles como completados (para testing)
3. **Clear All Level Progress**: Limpia todo el progreso guardado

### Testing Manual

```csharp
// En cualquier script, puedes llamar:

// Marcar nivel como completado
LevelProgressManager.Instance.MarkLevelAsCompleted("Level1");

// Verificar si está completado
bool completed = LevelProgressManager.Instance.IsLevelCompleted("Level1");

// Limpiar progreso de un nivel
LevelProgressManager.Instance.ClearLevelProgress("Level1");
```

### Verificar PlayerPrefs

Para ver las keys guardadas en Windows:
- Registro: `HKEY_CURRENT_USER\Software\[CompanyName]\[ProductName]`
- O usa: `PlayerPrefs.GetInt("Level_Level1_Completed", 0)`

## ⚠️ Consideraciones Importantes

1. **Nombres de Escenas**: Deben coincidir exactamente entre:
   - Build Settings → Scenes In Build
   - Campo "Level Scene Name" en `LevelCompletionMarker`
   - Nombre real de la escena en Unity

2. **Estrellas Requeridas**: El nivel solo se marca como completado si el jugador tiene al menos 1 estrella al ganar. Si pierde todas las estrellas pero llega al final, NO se guarda como completado.

3. **Singleton Pattern**: `LevelProgressManager` usa singleton, solo debe haber uno en la escena.

4. **Auto-detección**: Si el nombre del GameObject del botón contiene el nombre del nivel (ej: "Level1Button"), el sistema puede detectarlo automáticamente.

## 🎨 Ejemplo de Configuración Visual

```
LevelSelector Canvas
└── PrehistoricLevels Panel
    ├── Level0 Button
    │   ├── [MenuButton] → navigationtype: Custom, scene: "Level0"
    │   ├── [LevelCompletionMarker] → levelSceneName: "Level0"
    │   ├── Text (nombre del nivel)
    │   └── CompletedImage (Image)
    │       └── Sprite: StarIcon (inicialmente invisible)
    │
    ├── Level1 Button
    │   ├── [MenuButton] → navigationtype: Custom, scene: "Level1"
    │   ├── [LevelCompletionMarker] → levelSceneName: "Level1"
    │   ├── Text (nombre del nivel)
    │   └── CompletedImage (Image)
    │       └── Sprite: StarIcon
    │
    └── ... (más botones de nivel)
```

## 🔄 Flujo Completo

1. Jugador selecciona un nivel → Carga la escena del nivel
2. Jugador juega y completa el nivel con al menos 1 estrella
3. `GameConditionManager.Victoria()` → llama a `GuardarProgresoNivel()`
4. `LevelProgressManager.MarkCurrentLevelAsCompleted()` → guarda en PlayerPrefs
5. Jugador regresa al selector de niveles
6. `LevelCompletionMarker.Start()` → verifica progreso y muestra imagen si está completado

## 📦 Archivos Creados/Modificados

### Nuevos Archivos:
- `Assets/Scripts/Game/LevelProgressManager.cs`
- `Assets/Scripts/Game/LevelCompletionMarker.cs`

### Archivos Modificados:
- `Assets/Scripts/Game/GameConditionManager.cs` (agregado `GuardarProgresoNivel()`)
- `Assets/Scripts/Game/LifeStarsUI.cs` (agregado `GetCurrentLives()`)

---

## 💡 Tips Adicionales

- **Iconos de Completado**: Usa sprites como estrellas doradas, checkmarks verdes, o medallas
- **Animaciones**: Puedes agregar animaciones al mostrar la imagen de completado
- **Progreso por Estrellas**: Si quieres guardar cuántas estrellas obtuvo el jugador (1, 2, o 3), puedes extender el sistema para guardar el número de estrellas con `PlayerPrefs.SetInt($"Level_{levelName}_Stars", stars)`
- **Desbloqueo de Niveles**: Puedes combinar este sistema con un sistema de desbloqueo secuencial

¡El sistema está listo para usar! Solo necesitas configurar los botones en el selector de niveles según las instrucciones anteriores.

