# Sistema de Navegación por Canvas - Guía de Implementación

## ✅ Errores Corregidos
- Directivas de preprocesador mal formateadas
- Referencias a clases inexistentes
- Métodos obsoletos actualizados
- Nombres de métodos corregidos

## Resumen
He corregido y completado tu sistema de navegación para que funcione con Canvas en lugar de cambio de escenas. Esto permite que el AudioManager persista sin problemas entre los menús, y solo el botón "Prototype" carga una nueva escena.

## Archivos Modificados/Creados

### 1. `SceneNavigatorCanvas.cs` - **ARCHIVO PRINCIPAL**
Este es el nuevo controlador de navegación que maneja Canvas.

**Características principales:**
- ✅ Maneja activación/desactivación de Canvas
- ✅ Solo el nivel prototipo carga una escena nueva
- ✅ Integración automática con AudioManager
- ✅ Auto-asignación de Canvas si no están configurados
- ✅ Patrón Singleton para fácil acceso

### 2. `SceneNavigator.cs` - **ARCHIVO CORREGIDO**
Ahora funciona como híbrido: usa Canvas cuando está disponible, sino usa el sistema de escenas tradicional.

**Correcciones aplicadas:**
- ✅ Directivas de preprocesador corregidas
- ✅ Referencias a `SceneNavigatorCanvas` corregidas
- ✅ Método `FindFirstObjectByType` actualizado
- ✅ Nombres de métodos corregidos

### 3. `SceneNavigationEvents.cs` - **ARCHIVO CORREGIDO**
Sistema de eventos actualizado para manejar navegación por Canvas.

**Correcciones aplicadas:**
- ✅ Formato de directivas corregido
- ✅ Métodos de eventos renombrados para Canvas
- ✅ Compatibilidad con ambos sistemas (Canvas y escenas)

## Cómo Implementar

### Paso 1: Configurar la Escena Principal
1. **Unifica todas tus escenas de menú** en una sola escena (recomendado: usar la escena "Menu")
2. **Crea Canvas separados** para cada menú:
   - Canvas para Menú Principal
   - Canvas para Selector de Niveles  
   - Canvas para Créditos

### Paso 2: Configurar el GameObject Principal
1. **Toma el GameObject que tiene tu AudioManager**
2. **Agrega el script `SceneNavigatorCanvas`** al mismo GameObject
3. **En el Inspector del `SceneNavigatorCanvas`:**
   - Asigna la referencia a cada Canvas en los campos correspondientes
   - Configura el nombre de la escena del prototipo ("PrototypeLevel")
   - Ajusta el `menuBGMIndex` al índice de tu música de menú

### Paso 3: Configurar los Botones
**Tienes dos opciones para los botones:**

#### Opción A - Usar SceneNavigatorCanvas (Recomendado):
```csharp
// Menú Principal → Selector de Niveles
SceneNavigatorCanvas.NavigateToLevelSelector()

// Cualquier menú → Créditos  
SceneNavigatorCanvas.NavigateToCredits()

// Selector → Nivel Prototipo (carga escena)
SceneNavigatorCanvas.NavigateToPrototypeLevel()

// Volver al menú principal
SceneNavigatorCanvas.NavigateToMainMenu()
```

#### Opción B - Usar SceneNavigator (Híbrido):
```csharp
// Funciona igual, pero detecta automáticamente si usar Canvas o escenas
SceneNavigator.NavigateToLevelSelector()
SceneNavigator.NavigateToCredits()
SceneNavigator.NavigateToPrototypeLevel()
SceneNavigator.NavigateToMainMenu()
```

### Paso 4: Configurar Eventos (Opcional)
Si quieres que otros sistemas reaccionen a los cambios de menú:

1. **Agrega el script `SceneNavigationEvents`** a un GameObject persistente
2. **En el Inspector** puedes configurar eventos que se ejecuten:
   - Antes de cambiar de menú
   - Al entrar a cada menú específico
   - Después de cambiar de menú

## Configuración del AudioManager

### El AudioManager debe estar en el mismo GameObject que `SceneNavigatorCanvas`
- El script automáticamente encontrará el AudioManager
- Reproducirá la música de menú al iniciar
- La música continuará sonando entre todos los menús
- Solo se detendrá al cargar el nivel prototipo

### Configurar las pistas de audio:
En tu AudioManager (según la imagen que enviaste):
- **Elemento 0**: "1. Menu BIT Loop" (configurar `menuBGMIndex = 0`)
- **Elemento 1**: "2. Prehistoria BIT Loop" 
- **Elemento 2**: "3. Medieval BIT Loop"

## Ventajas de esta Solución

✅ **Sin interrupciones de audio**: La música de menú nunca se corta  
✅ **Transiciones instantáneas**: No hay tiempo de carga entre menús  
✅ **Fácil de configurar**: Auto-asignación de Canvas  
✅ **Compatible con tu código existente**: Los métodos estáticos siguen funcionando  
✅ **Eventos personalizables**: Puedes agregar lógica específica por menú  
✅ **Solo el nivel prototipo carga escena**: Mantienes la separación entre menús y gameplay  
✅ **Sin errores de compilación**: Todas las referencias corregidas  

## Notas Importantes

1. **Mantén solo un Canvas activo a la vez**: El script se encarga de esto automáticamente
2. **El AudioManager debe estar en el GameObject principal**: Para que persista correctamente
3. **Los Canvas deben estar en la misma escena**: No pueden estar en escenas separadas
4. **El nivel prototipo sigue siendo una escena separada**: Para que tenga su propia música

## Resolución de Problemas

### Si los Canvas no se asignan automáticamente:
- Asegúrate de que los nombres de los Canvas contengan palabras clave como "menu", "selector", "credits"
- O asígnalos manualmente en el Inspector

### Si la música no suena:
- Verifica que el AudioManager esté en el mismo GameObject
- Confirma que `menuBGMIndex` coincida con el índice correcto en `bgTracks`

### Si los botones no funcionan:
- Verifica que las llamadas a los métodos estáticos estén correctas
- Asegúrate de que el `SceneNavigatorCanvas` esté activo en la escena

### Si hay errores de compilación:
- Todos los errores ya fueron corregidos en esta versión
- Asegúrate de que todos los archivos estén en la carpeta correcta
- Verifica que no haya archivos duplicados
