# Guía de Configuración de la Escena para Sistema de Reparación

## 1. Configurar Tags y Layers (En Editor de Unity)

### Tags Necesarios:
1. Ve a `Edit → Project Settings → Tags and Layers`
2. En la sección **Tags**, añade:
   - `BridgeQuadrant`
   - `BridgeLayer3` (para la 4ta capa - adoquín)

### Layers Necesarios:
1. En la sección **Layers**, asigna:
   - Layer 6: `Bridge`
   - Layer 8: `BridgeLayer4` (opcional, para capa específica)

## 2. Configurar el Sistema de Grilla de Puentes

### Crear BridgeGrid GameObject:
1. Crea un objeto vacío en la escena
2. Nómbralo `BridgeGrid`
3. Añádele el componente `BridgeConstructionGrid`
4. Configura los siguientes valores:
   - **Grid Width**: 3
   - **Grid Length**: 5
   - **Quadrant Size**: 1
   - **Default Quadrant SO**: Arrastra `PrehistoricBridgeQuadrant.asset`
   - **Quadrant Prefab**: Arrastra `BridgeQuadrantPrefab.prefab`
   - **Show Debug Grid**: ✓ (marcado)

### Crear QuadrantParent:
1. Crea un objeto vacío como hijo de `BridgeGrid`
2. Nómbralo `QuadrantContainer`
3. Asígnalo al campo **Quadrant Parent** en `BridgeConstructionGrid`

## 3. Configurar los Jugadores

### Para cada jugador (Player1 y Player2):
1. Añade el componente `PlayerBridgeInteraction`
2. Configura:
   - **Bridge Grid**: Arrastra el objeto `BridgeGrid`
   - **Interaction Range**: 2.0
   - **Bridge Layer**: Selecciona `Bridge` (Layer 6)

### Configurar Build Point:
1. Crea un objeto vacío como hijo del jugador
2. Nómbralo `BuildPoint`
3. Posiciónalo frente al jugador (por ejemplo: Position = (0, 0, 1))
4. Asígnalo al campo **Build Point** en `PlayerBridgeInteraction`

## 4. Colocar GenericObject3 en la Escena

### Configurar el Generador de Adoquín:
1. Arrastra `GenericObject3.prefab` a la escena
2. Posiciónalo cerca del área del puente
3. Verifica que tenga el componente `GenericObject3` configurado
4. Debe estar configurado para producir **Material Tipo 4** (adoquín)

## 5. Verificar MaterialPrefabSO

### Asegurar Configuración Correcta:
1. Abre `MaterialesPrefabs.asset` en el inspector
2. Verifica que contenga una entrada para:
   - **Material Tipo**: 4
   - **Era**: Prehistoric (0)
   - **Prefab**: `PrefabMaterial4.prefab`

## 6. Probar el Sistema

### Flujo de Prueba:
1. **Ejecuta la escena**
2. **Construye un puente**: Usa materials 1-3 para construir las primeras capas
3. **Daña la última capa**: Haz que un vehículo pase varias veces hasta que se dañe
4. **Genera adoquín**: Mantén presionado `GenericObject3` durante 1 segundo
5. **Repara**: Acércate al cuadrante dañado con el adoquín y interactúa

## 7. Depuración

### Si hay problemas:
1. Revisa la Consola de Unity para mensajes de debug
2. Verifica que todos los prefabs tengan los componentes necesarios
3. Asegúrate de que las referencias en el inspector estén asignadas
4. Usa el `BridgeDebugger` component para información adicional

## 8. Comandos de Prueba Automática

### En el inspector de BridgeRepairTest:
1. Arrastra `PrehistoricBridgeQuadrant.asset` al campo **Test Quadrant**
2. Arrastra `MaterialesPrefabs.asset` al campo **Material Prefabs SO**
3. La prueba se ejecutará automáticamente al iniciar la escena
