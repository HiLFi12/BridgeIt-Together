# Cómo Probar el Sistema de Reparación

## Flujo de Prueba Paso a Paso:

### 1. Preparación Inicial
- Ejecuta la escena
- Verifica que aparezcan cuadrantes de puente en la grilla
- La consola debe mostrar mensajes de inicialización

### 2. Construir un Puente
**Materiales necesarios (en orden):**
1. **Material Tipo 1** (Cimientos) - Usa GenericObject1
2. **Material Tipo 2** (Soporte) - Usa GenericObject2  
3. **Material Tipo 3** (Estructura) - Usa otro generador
4. **Material Tipo 4** (Adoquín) - Usa GenericObject3 ⭐

**Proceso:**
1. Acércate a un generador de material
2. Mantén presionada la tecla de interacción (por defecto F o botón de acción)
3. Espera a que se genere el material
4. Acércate a un cuadrante del puente
5. Presiona la tecla de interacción para colocar el material

### 3. Dañar la Última Capa
**Métodos para crear daño:**
- **Automático**: Haz que un vehículo pase por el puente varias veces
- **Manual**: Usa el BridgeDebugger para simular impacto de vehículo
- **Código**: Llama a `quadrantSO.OnVehicleImpact()` múltiples veces

**Señales de daño:**
- La consola mostrará "Última capa dañada"
- El estado visual del cuadrante puede cambiar
- `lastLayerState` será `Damaged`

### 4. Reparar con Adoquín
1. **Generar Adoquín:**
   - Acércate a GenericObject3
   - Mantén presionada la tecla de interacción durante **1 segundo completo**
   - Deberías obtener un Material Tipo 4 (adoquín)

2. **Aplicar Reparación:**
   - Acércate al cuadrante dañado (debe tener `lastLayerState: Damaged`)
   - Presiona la tecla de interacción
   - El sistema detectará que es una reparación y restaurará el cuadrante

### 5. Verificar Reparación Exitosa
**Señales de éxito:**
- Consola muestra: "Reparando última capa dañada (capa 3)"
- `lastLayerState` cambia a `Complete`
- El cuadrante vuelve a ser funcional para vehículos

## Comandos de Debug Útiles:

### En BridgeRepairTest:
- `Verificar Configuracion Escena` - Verifica que todo esté configurado
- `Ejecutar Prueba Completa` - Ejecuta todas las verificaciones automáticas
- `Test GenericObject3 Production` - Verifica el generador de adoquín

### En BridgeDebugger (si está presente):
- Permite simular impactos de vehículos
- Muestra estadísticas del estado del puente
- Útil para depuración avanzada

## Solución de Problemas Comunes:

### Problema: No se genera material adoquín
**Solución:**
- Verifica que GenericObject3 esté en la escena
- Confirma que MaterialPrefabSO contenga entrada para tipo 4
- Mantén presionada la tecla el tiempo completo (1 segundo)

### Problema: No se puede reparar
**Solución:**
- Verifica que el cuadrante esté realmente dañado (`lastLayerState: Damaged`)
- Asegúrate de tener el material tipo 4 en la mano
- Confirma que estés dentro del rango de interacción

### Problema: La reparación no funciona
**Solución:**
- Revisa la consola para mensajes de error
- Verifica que BridgeQuadrantSO.TryAddLayer() esté siendo llamado
- Confirma que la lógica de reparación (líneas 113-122) esté ejecutándose

## Mensajes de Consola Esperados:

### Durante construcción normal:
```
[TryAddLayer] Estado actual del cuadrante: Capa 0: Completada, Capa 1: Incompleta, ...
ÉXITO: Capa 1 marcada como completada
```

### Durante reparación:
```
[TryAddLayer] Estado actual del cuadrante: Capa 0: Completada, Capa 1: Completada, ...
Reparando última capa dañada (capa 3)
```

### Si hay errores:
```
ERROR: Capa 3 ya está completada y no necesita reparación
ERROR DE SECUENCIA: Debes construir primero la capa X, no la capa Y
```
