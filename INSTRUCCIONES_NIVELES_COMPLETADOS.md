# Sistema de Niveles Completados - SIMPLIFICADO

## 🎯 Cómo Funciona

El `LevelProgressManager` tiene una lista de botones de nivel. Cuando ganas un nivel, guarda el progreso en PlayerPrefs y actualiza todos los botones mostrando u ocultando las estrellas.

## 🚀 Configuración Rápida

### 1. En la Escena del Menú

1. **Asegúrate de tener el LevelProgressManager**
   - Ya lo agregaste, perfecto ✅

2. **Configurar la Lista de Niveles**
   - Selecciona el GameObject `LevelProgressManager`
   - En el Inspector, verás `Level Buttons` (lista)
   - Aumenta el tamaño de la lista según cuántos botones de nivel tienes

3. **Para cada nivel en la lista:**
   - **Level Scene Name**: Escribe el nombre EXACTO de la escena (ej: "Level1", "Level0_M")
   - **Button**: Arrastra el botón del nivel desde la jerarquía
   - **Completion Star**: Arrastra la imagen de estrella que quieres mostrar cuando esté completado

### Ejemplo de configuración:
```
Element 0:
  - Level Scene Name: "Level1"
  - Button: [Botón Level1 de la jerarquía]
  - Completion Star: [Imagen hijo "Star" del botón]

Element 1:
  - Level Scene Name: "Level2"
  - Button: [Botón Level2 de la jerarquía]
  - Completion Star: [Imagen hijo "Star" del botón]
```

### 2. Preparar los Botones de Nivel

Para cada botón de nivel:

1. **Agregar imagen de estrella como hijo:**
   - Click derecho en el botón → UI → Image
   - Nombra la imagen: "Star" o "CompletedStar"
   - Asigna un sprite de estrella
   - Posiciónala donde quieras (ej: esquina superior derecha)
   - **NO desactives la imagen**, el script lo hará automáticamente

2. **Configurar el MenuButton** (si no lo tienes ya):
   - Add Component → MenuButton
   - Navigation Type: Custom
   - Custom Scene: Selecciona la escena del nivel

### 3. En Todas las Escenas de Nivel

El `LevelProgressManager` ya está configurado con `DontDestroyOnLoad`, así que estará presente en todas las escenas automáticamente. **No necesitas hacer nada más.**

## 🎮 Flujo Automático

1. **Jugador completa nivel** → GameConditionManager detecta victoria
2. **Si tiene ≥1 estrella** → Busca LevelProgressManager con `FindFirstObjectByType`
3. **Guarda en PlayerPrefs** → `PlayerPrefs.SetInt("Level_NombreNivel_Completed", 1)`
4. **Actualiza TODOS los botones** → Muestra estrellas en niveles completados
5. **Regresa al menú** → Las estrellas se mantienen visibles

## 🧪 Testing en Unity

Con el `LevelProgressManager` seleccionado en el Inspector, puedes usar:

### Comandos de Context Menu (Click derecho):
- **"Mostrar Progreso de Niveles"** → Ver en consola qué niveles están completados
- **"Completar Todos los Niveles (Debug)"** → Marcar todos como completados (para testing)
- **"Limpiar Todo el Progreso"** → Resetear todo el progreso
- **"Actualizar Todos los Botones"** → Forzar actualización visual

### Testing Manual:
1. Juega un nivel y gánalo con al menos 1 estrella
2. Regresa al menú
3. La estrella del nivel completado debería aparecer automáticamente

## ⚠️ Puntos Importantes

✅ **El nombre de la escena debe ser EXACTO**
   - Revisa en Build Settings → Scenes In Build
   - Debe coincidir con el campo "Level Scene Name"

✅ **El LevelProgressManager NO se destruye entre escenas**
   - Solo debe estar en la escena del menú
   - Persistirá automáticamente en los niveles

✅ **Las estrellas se ocultan automáticamente al inicio**
   - No necesitas desactivarlas manualmente
   - El script las activará solo si el nivel está completado

✅ **Solo guarda si el jugador tiene ≥1 estrella**
   - Si pierdes todas las estrellas pero llegas al final, NO se guarda

## 🐛 Si No Funciona

1. **Verifica en la consola:**
   - Busca el mensaje: "✅ Nivel '[nombre]' marcado como completado"
   - Busca el mensaje: "🔄 Actualizando X botones de nivel..."

2. **Usa los comandos de debug:**
   - Click derecho en LevelProgressManager → "Mostrar Progreso de Niveles"

3. **Verifica las referencias:**
   - Todas las referencias (Button, Completion Star) deben estar asignadas
   - Los nombres de escena deben ser exactos

## 📝 Estructura del LevelProgressManager

```
LevelProgressManager (GameObject)
├── LevelProgressManager (Script)
    ├── Level Buttons (Lista)
    │   ├── Element 0 (Level1)
    │   ├── Element 1 (Level2)
    │   └── Element 2 (Level0_M)
    ├── Eras (Lista) - Para después
    └── Show Debug Logs: ✅
```

¡Eso es todo! El sistema es simple y automático. 🎉

