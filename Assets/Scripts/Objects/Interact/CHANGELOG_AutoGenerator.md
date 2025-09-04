# Cambios en el Sistema de Generación de Autos

## Última actualización: 9 de junio de 2025

### Cambios principales

#### 1. Sistema de Movimiento

- **Modificado:** Ahora los autos se mueven usando `transform.right` (eje X local) en lugar de `transform.forward` (eje Z local)
- **Razón:** Asegurar que los autos se muevan horizontalmente hacia el puente (eje X) correctamente

#### 2. Sistema de Rotación

- **Simplificado:** Se usan rotaciones fijas en el eje Y
  - Autos desde la izquierda: `Quaternion.Euler(0, 90, 0)` (mirando hacia +X)
  - Autos desde la derecha: `Quaternion.Euler(0, -90, 0)` (mirando hacia -X)
- **Eliminado:** El cálculo dinámico de rotación en `CalcularRotacionHaciaPuente()`

#### 3. Orientación del Modelo

- **Actualizado:** La corrección de rotación del modelo ahora es `(-90, 0, 0)` para mantener los autos horizontales
- **Implementado:** Método `CorregirRotacion()` en `AutoMovement.cs` que busca automáticamente el modelo 3D

#### 4. Nuevas herramientas de depuración

- **Añadido:** Método `ProbarMovimientoEjeX()` para verificar específicamente el movimiento en el eje X
- **Mejorado:** Visualización de trayectorias en el editor con `OnDrawGizmos()`

### Cómo probar los cambios

1. En el editor de Unity, selecciona el objeto con el componente `AutoGenerator`
2. Haz clic derecho en el componente y selecciona una de estas opciones:
   - "Probar Generación Auto Izquierda": Genera un auto desde la izquierda
   - "Probar Generación Auto Derecha": Genera un auto desde la derecha
   - "Probar Movimiento en Eje X": Genera autos de ambos lados usando las nuevas rotaciones fijas
   - "Probar Orientaciones del Modelo": Prueba diferentes rotaciones para el modelo 3D

### Solución de problemas

Si los modelos siguen apareciendo verticalmente en lugar de horizontalmente:

1. Prueba diferentes valores de corrección de rotación en el método `GenerarAuto()`:
   ```csharp
   // Algunas opciones a probar:
   autoMovement.SetCorreccionRotacion(new Vector3(-90f, 0f, 0f));  // Opción actual
   autoMovement.SetCorreccionRotacion(new Vector3(90f, 0f, 0f));   // Rotación opuesta en X
   autoMovement.SetCorreccionRotacion(new Vector3(0f, 0f, 90f));   // Rotación en Z
   ```

2. Usa el método "Probar Orientaciones del Modelo" para ver visualmente las diferentes opciones
