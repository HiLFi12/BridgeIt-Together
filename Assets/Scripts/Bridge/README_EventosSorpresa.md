# Sistema de Eventos Sorpresa - Bridge it Together!

## Descripción

Este sistema permite implementar eventos sorpresa que destruyen el puente hasta una capa específica después de completar ciertas rondas en el AutoGenerator.

## Archivos Involucrados

1. **BridgeSurpriseEvent.cs** - Maneja la lógica de destrucción del puente
2. **AutoGenerator.cs** - Modificado para integrar los eventos sorpresa

## Cómo Usar

### Configuración Básica

1. **Agregar el componente BridgeSurpriseEvent:**
   - En la escena, crea un GameObject vacío llamado "BridgeSurpriseEventManager"
   - Añade el componente `BridgeSurpriseEvent`
   - Asigna las referencias al `BridgeConstructionGrid` y `BridgeInitialConstructor` en el inspector

2. **El sistema se activa automáticamente:**
   - El AutoGenerator detectará automáticamente el componente BridgeSurpriseEvent
   - Los eventos están configurados para ejecutarse después de las rondas 2 y 4 por defecto

### Configuración de Eventos

En el inspector del componente `BridgeSurpriseEvent`:

- **Usar Eventos Predefinidos:** Habilita/deshabilita los eventos configurados
- **Eventos Predefinidos:** Array de eventos que se pueden configurar desde el inspector

#### Configuración de Cada Evento

- **Nombre Evento:** Nombre descriptivo del evento
- **Después De Ronda:** Número de la ronda después de la cual se ejecuta el evento (1-based)
  - Ejemplo: 2 = Se ejecuta después de completar la ronda 2
  - Ejemplo: 5 = Se ejecuta después de completar la ronda 5
- **Capa Objetivo:** Hasta qué capa permanecerá el puente (0-4)
  - 0 = Destrucción completa
  - 1 = Solo capa base
  - 2 = Base + soporte
  - 3 = Base + soporte + estructura
  - 4 = Puente completo (no destruye nada)
- **Afectar Todos Los Cuadrantes:** Si se afectan todos los cuadrantes o solo específicos
- **Duración Efecto Visual:** Tiempo del efecto visual (en segundos)
- **Mostrar Mensajes Debug:** Si mostrar información en la consola

### Cómo Configurar las Rondas para los Eventos

#### Opción 1: Desde el Inspector (Recomendado)

1. **Selecciona el GameObject** con el componente `BridgeSurpriseEvent`
2. **Expande "Eventos Predefinidos"** en el inspector
3. **Configura cada evento:**
   - Cambia el **"Después De Ronda"** al número que quieras
   - Ajusta la **"Capa Objetivo"** según cuánto quieres destruir
   - Personaliza el **"Nombre Evento"** para identificarlo fácilmente

#### Opción 2: Añadir Nuevos Eventos

1. **Aumenta el "Size"** del array "Eventos Predefinidos"
2. **Configura el nuevo evento:**
   ```
   Nombre Evento: "Mi Evento Personalizado"
   Después De Ronda: 3 (se ejecuta después de la ronda 3)
   Capa Objetivo: 2 (deja base + soporte)
   Afectar Todos Los Cuadrantes: ✓
   ```

#### Opción 3: Eliminar Eventos

1. **Reduce el "Size"** del array para eliminar eventos
2. O **desmarca "Usar Eventos Predefinidos"** para desactivar todos

### Ejemplo de Configuración

```
Evento 1:
  Nombre: "Colapso Parcial"
  Después De Ronda: 2
  Capa Objetivo: 1

Evento 2:
  Nombre: "Destrucción Mayor"  
  Después De Ronda: 4
  Capa Objetivo: 0

Resultado:
Ronda 1 → Juega normalmente
Ronda 2 → Al terminar: EVENTO "Colapso Parcial" (destruye hasta capa 1)
Ronda 3 → Juega normalmente
Ronda 4 → Al terminar: EVENTO "Destrucción Mayor" (destruye hasta capa 0)
Ronda 5 → Juega normalmente
```

### Ejemplos de Configuración Avanzada

#### Eventos Múltiples en la Misma Ronda
```
Evento 1: Después De Ronda = 3, Capa Objetivo = 2
Evento 2: Después De Ronda = 3, Capa Objetivo = 1

Resultado: Después de la ronda 3, se ejecutan ambos eventos en secuencia
```

#### Eventos Espaciados
```
Evento 1: Después De Ronda = 1, Capa Objetivo = 2  
Evento 2: Después De Ronda = 5, Capa Objetivo = 0
Evento 3: Después De Ronda = 8, Capa Objetivo = 1

Resultado: Eventos en las rondas 1, 5 y 8
```

### Personalización

Para cambiar cuándo ocurren los eventos sorpresa, modifica la función `VerificarEventoSorpresa` en AutoGenerator.cs:

```csharp
// Cambiar de ronda 2 a ronda 3
if (rondaCompletada == 3) // En lugar de == 2
{
    // ... código del evento
}
```

### Métodos de Debug

El componente BridgeSurpriseEvent incluye métodos de prueba en el Context Menu:

- **Probar Evento Sorpresa:** Ejecuta un evento de prueba
- **Destruir Puente Completamente:** Destruye todo el puente
- **Destruir Hasta Capa Base:** Destruye hasta dejar solo la base

### Características del Sistema

- ✅ **Integración automática** con el sistema de rondas existente
- ✅ **Configuración visual** desde el inspector de Unity
- ✅ **Eventos personalizables** para diferentes rondas
- ✅ **Destrucción selectiva** de capas específicas
- ✅ **Efectos visuales** configurables
- ✅ **Sistema de debug** completo
- ✅ **Sin scripts de testing** - listo para producción

### Notas Importantes

1. El sistema requiere que el `usarSistemaRondas` esté habilitado en el AutoGenerator
2. El sistema detecta automáticamente el BridgeSurpriseEvent en la escena
3. Si no se encuentra el componente, el sistema se desactiva silenciosamente
4. Los eventos están sincronizados con el final de cada ronda

### Flujo de Ejecución

1. El jugador completa una ronda
2. El AutoGenerator llama a `VerificarEventoSorpresa()`
3. Se verifica si hay un evento configurado para esa ronda
4. Si existe, se ejecuta el evento sorpresa
5. El puente se modifica según la configuración
6. Continúa con la siguiente ronda

Este sistema es completamente funcional y está listo para usar en producción.
