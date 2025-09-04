# Instrucciones para el Generador de Autos

## Descripción
Este sistema implementa un generador de autos que utiliza un pool de objetos y spawnea autos de forma intercalada desde ambos lados del mapa.

## Configuración

### 1. Configuración del AutoGenerator

1. Arrastra el prefab `AutoGeneradorPool.prefab` a tu escena.
2. Configura las siguientes propiedades en el Inspector:
   - **Auto Prefab**: El prefab del auto que quieres generar.
   - **Tiempo Entre Autos**: Tiempo de espera entre cada generación de auto.
   - **Velocidad Auto**: La velocidad a la que se moverán los autos.
   - **Bridge Grid**: Referencia al objeto BridgeConstructionGrid de la escena.
   - **Punto Spawn Izquierdo**: Transform que marca el punto de aparición del lado izquierdo.
   - **Punto Spawn Derecho**: Transform que marca el punto de aparición del lado derecho.
   - **Iniciar Desde Izquierda**: Si está marcado, el primer auto aparecerá desde la izquierda.
   - **Pool Size**: Cantidad inicial de autos en el pool.
   - **Pool Expandible**: Si está marcado, el pool puede crear más autos si es necesario.

### 2. Configuración de los puntos de spawn

1. Crea dos objetos vacíos en la escena.
2. Nombra uno como "PuntoSpawnIzquierdo" y otro como "PuntoSpawnDerecho".
3. Posiciona cada uno en el lugar donde quieres que aparezcan los autos.
4. Asigna estas referencias al AutoGenerator.

### 3. Asegúrate de que el prefab del auto:

1. Tenga un modelo visual (Mesh Renderer, etc.).
2. Tenga un componente Rigidbody (se añade automáticamente si no existe).
3. Tenga un componente Collider (se añade automáticamente si no existe).

## Funcionamiento

- El sistema genera autos de forma alternada: primero desde un lado, luego desde el otro.
- Los autos se reciclan usando un sistema de pool de objetos, lo que mejora el rendimiento.
- Después de un tiempo (configurable), los autos se devuelven al pool para ser reutilizados.
- Si todos los autos del pool están en uso y el pool es expandible, se crearán nuevos autos.

## Métodos útiles

- `ClearActiveAutos()`: Devuelve todos los autos activos al pool (útil al reiniciar el nivel).
- `ProbarGeneracionAutoIzquierda()`: Para probar la generación de autos desde la izquierda.
- `ProbarGeneracionAutoDerecha()`: Para probar la generación de autos desde la derecha.
- `ProbarMovimientoEjeX()`: **NUEVO** - Verifica que los autos se muevan en direcciones opuestas correctamente.
- `ProbarOrientacionHorizontalModelo()`: **NUEVO** - Muestra diferentes rotaciones para encontrar la orientación horizontal correcta del modelo.
- `ProbarTodasLasRotaciones()`: Para evaluar diferentes rotaciones y determinar cuáles son las correctas.

## Configuración avanzada de rotación

El sistema ahora maneja dos tipos de rotaciones:

### 1. Rotación de Dirección

Determina hacia dónde se mueve el auto:
- Para autos desde la izquierda: se mueven hacia la izquierda usando `Vector3.left` (-X)
- Para autos desde la derecha: se mueven hacia la derecha usando `Vector3.right` (+X)
- El movimiento se configura mediante `SetDireccionMovimiento()` en `AutoMovement.cs`

### 2. Rotación del Modelo 3D

Corrige la orientación del modelo del auto para que se vea horizontalmente:
- Se aplica mediante el vector `correccionRotacion` en `AutoMovement.cs`
- El valor predeterminado es `(-90, 0, 0)` para corregir modelos que aparecen verticales

### Ajustando la Orientación del Modelo

1. Usa la opción del menú contextual "Probar Orientaciones del Modelo" en el componente `AutoGenerator`
2. Observa cuál de los autos generados tiene la orientación correcta (debería ser horizontal)
3. Modifica los valores de corrección en el método `GenerarAuto()` en `AutoGenerator.cs`

```csharp
// Cambia estos valores según cómo esté orientado tu modelo
autoMovement.SetCorreccionRotacion(new Vector3(-90f, 0f, 0f)); // Rotación en X para corregir modelos verticales
```

> **NOTA IMPORTANTE**: Si el modelo sigue apareciendo incorrectamente orientado, prueba con otros valores como `(90, 0, 0)`, `(-90, 0, 0)`, o `(0, 0, 90)` dependiendo de cómo esté orientado originalmente el modelo en el prefab.

## Resolución de problemas

- Si los autos no aparecen, verifica que los puntos de spawn estén correctamente posicionados.
- Si hay errores de "Missing Reference", asegúrate de que el Auto Prefab esté asignado.
- Si los autos no colisionan con el puente, verifica que la referencia al BridgeGrid esté asignada.

### Depuración del movimiento

Si los autos no se mueven en la dirección correcta (hacia el puente):

1. Utiliza las opciones del menú contextual en el componente AutoGenerator para probar:
   - **Probar Generación Auto Izquierda**: Genera un auto desde el lado izquierdo.
   - **Probar Generación Auto Derecha**: Genera un auto desde el lado derecho.
   - **Probar Todas las Rotaciones**: Genera autos con diferentes rotaciones para verificar cuál funciona mejor.
   - **Probar Ruta Hacia Puente**: Muestra visualmente con líneas la ruta hacia el puente y genera autos de prueba.

2. Revisa los mensajes de la consola para ver la configuración de cada auto.

3. El sistema ahora calcula automáticamente la dirección hacia el puente usando `Quaternion.LookRotation`:
   - Calcula un vector dirección desde el punto de spawn hacia el centro (puente).
   - Aplica esta dirección como rotación para que los autos miren hacia el puente.
   - Corrige la rotación para que los autos no miren hacia arriba (mantiene Y=0).

4. Observa los rayos de depuración en Scene View (Debug.DrawRay) para ver la dirección real de movimiento.

5. Si continúa habiendo problemas con la orientación de los modelos:

   - Use la opción "Probar Orientaciones del Modelo" para generar autos con diferentes configuraciones.
   - Observe qué auto tiene la orientación correcta (horizontal).
   - El método `CorregirRotacion()` en `AutoMovement.cs` busca automáticamente el modelo 3D del auto y aplica la rotación.
   - Modifique la corrección de rotación en `GenerarAuto()` basándose en los resultados de la prueba:
   - Asegúrate de que los puntos de spawn estén correctamente posicionados a ambos lados del puente.
   - Confirma que el prefab del auto tiene su parte frontal mirando hacia adelante (eje +Z).
   - Verifica que no hay obstáculos en el camino que puedan alterar el movimiento.
