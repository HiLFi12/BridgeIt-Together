# Sistema de Construcción Inicial del Puente

## Descripción General

Este sistema permite configurar el puente para que aparezca construido hasta diferentes capas al iniciar el nivel. Es útil para crear niveles con diferentes estados iniciales del puente.

## Componentes del Sistema

### 1. BridgeInitialConstructor

**Archivo:** `BridgeInitialConstructor.cs`

Script principal que controla la construcción inicial del puente.

#### Configuración Básica:
- **Bridge Grid**: Referencia al BridgeConstructionGrid de la escena
- **Initial Constructed Layers**: Número de capas a construir (0-4)
  - 0: Sin construir
  - 1: Solo capa base
  - 2: Base + soporte
  - 3: Base + soporte + estructura
  - 4: Puente completo

#### Configuración de Cuadrantes:
- **Construct All Quadrants**: Si construir todos los cuadrantes o solo algunos específicos
- **Specific Quadrants**: Array de booleanos para seleccionar cuadrantes específicos

#### Estado de la Última Capa:
- **Last Layer State**: Estado de la última capa cuando se construyen 4 capas
  - Complete: Completamente funcional
  - Damaged: Dañada (necesita reparación)
  - Destroyed: Destruida

#### Opciones de Debug:
- **Show Debug Messages**: Mostrar mensajes de debug en la consola
- **Apply On Start**: Aplicar la construcción automáticamente al iniciar

### 2. BridgeInitialConstructorEditor

**Archivo:** `Editor/BridgeInitialConstructorEditor.cs`

Editor personalizado que proporciona una interfaz más amigable en el Inspector:

- Selector visual de capas con descripciones
- Grilla visual para seleccionar cuadrantes específicos
- Botones de acción rápida
- Validación de configuración

### 3. BridgeConstructionPreset

**Archivo:** `BridgeConstructionPreset.cs`

ScriptableObject para guardar configuraciones preestablecidas:

- Permite crear presets reutilizables
- Fácil aplicación de configuraciones comunes
- Descripción y metadatos para cada preset

### 4. BridgePresetManager

**Archivo:** `BridgeConstructionPreset.cs`

Componente para gestionar múltiples presets:

- Aplicación de presets por índice o nombre
- Lista de presets disponibles
- Gestión de preset actual

## Guía de Uso

### Configuración Básica

1. **Añadir el Componente:**
   ```
   - Selecciona un GameObject en la escena
   - Add Component → Bridge Initial Constructor
   ```

2. **Configurar Referencias:**
   ```
   - Arrastra el BridgeConstructionGrid al campo "Bridge Grid"
   ```

3. **Seleccionar Capas:**
   ```
   - Usa el dropdown "Capas a Construir" para seleccionar 0-4 capas
   - Lee la descripción que aparece debajo
   ```

4. **Configurar Cuadrantes:**
   ```
   - Marca "Construct All Quadrants" para construir todos
   - O desmarca y usa "Seleccionar Cuadrantes Específicos"
   ```

### Uso de la Grilla Visual

Cuando "Construct All Quadrants" está desmarcado:

1. **Configurar Grilla:**
   ```
   - Presiona "Configurar Cuadrantes Específicos"
   - Se creará una grilla basada en las dimensiones del BridgeGrid
   ```

2. **Seleccionar Cuadrantes:**
   ```
   - Expande "Seleccionar Cuadrantes Específicos"
   - Usa los botones [x,z] para seleccionar/deseleccionar
   - Verde = Seleccionado, Rojo = No seleccionado
   ```

3. **Selección Rápida:**
   ```
   - "Seleccionar Todos": Selecciona todos los cuadrantes
   - "Deseleccionar Todos": Deselecciona todos los cuadrantes
   ```

### Acciones Disponibles

- **Construir Puente Inicial**: Ejecuta la construcción con la configuración actual
- **Reinicializar Puente**: Destruye todas las capas construidas
- **Validar Configuración**: Verifica que todo esté configurado correctamente

### Uso de Presets

1. **Crear un Preset:**
   ```
   Project → Create → Bridge → Initial Construction Preset
   ```

2. **Configurar el Preset:**
   ```
   - Establece nombre y descripción
   - Configura todas las opciones deseadas
   ```

3. **Usar Preset Manager:**
   ```
   - Añade BridgePresetManager a un GameObject
   - Asigna presets al array "Available Presets"
   - Selecciona "Current Preset"
   - Presiona "Aplicar Preset Actual"
   ```

## Casos de Uso Comunes

### Nivel Tutorial - Puente Vacío
```
- Initial Constructed Layers: 0
- Construct All Quadrants: true
- Apply On Start: true
```

### Nivel Intermedio - Base Construida
```
- Initial Constructed Layers: 1
- Construct All Quadrants: true
- Apply On Start: true
```

### Nivel de Reparación - Puente Dañado
```
- Initial Constructed Layers: 4
- Last Layer State: Damaged
- Construct All Quadrants: true
- Apply On Start: true
```

### Nivel Avanzado - Puente Parcial
```
- Initial Constructed Layers: 2
- Construct All Quadrants: false
- Specific Quadrants: Seleccionar manualmente
- Apply On Start: true
```

## Métodos de Código

### BridgeInitialConstructor

```csharp
// Construir puente programáticamente
public void ConstructInitialBridge()

// Reinicializar puente
public void ResetBridge()

// Validar configuración
public void ValidateConfiguration()

// Configurar cuadrantes específicos
public void ConfigureSpecificQuadrants()
```

### BridgePresetManager

```csharp
// Aplicar preset por índice
public void ApplyPreset(int index)

// Aplicar preset por nombre
public void ApplyPreset(string presetName)

// Aplicar preset actual
public void ApplyCurrentPreset()
```

## Solución de Problemas

### El puente no se construye
- Verifica que BridgeGrid esté asignado
- Asegúrate de que Initial Constructed Layers > 0
- Revisa que Apply On Start esté marcado

### Algunos cuadrantes no se construyen
- Verifica la configuración de Specific Quadrants
- Asegúrate de que los cuadrantes estén dentro de los límites de la grilla
- Revisa los mensajes de debug en la consola

### La grilla visual no aparece
- Presiona "Configurar Cuadrantes Específicos" primero
- Asegúrate de que BridgeGrid esté asignado
- Verifica que "Construct All Quadrants" esté desmarcado

## Integración con Otros Sistemas

Este sistema es compatible con:
- Sistema de reparación de puentes existente
- Power-ups de construcción
- Sistema de materiales
- Debug tools del BridgeConstructionGrid

## Notas Técnicas

- Usa reflexión para acceder a métodos privados del BridgeConstructionGrid
- Crea objetos temporales para pasar validaciones de construcción
- Compatible con el sistema de capas y estados existente
- No interfiere con la mecánica de juego normal
