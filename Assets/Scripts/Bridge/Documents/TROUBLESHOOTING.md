# Solución de Problemas - Sistema de Puentes

## Lista de Verificación Inicial

- [ ] **Tags y Layers**
  - [ ] Tag "BridgeQuadrant" creado
  - [ ] Layer "Bridge" creada (en cualquier slot disponible, ej: User Layer 8)

- [ ] **Prefab del Cuadrante**
  - [ ] Objeto con BoxCollider
  - [ ] Script BridgeQuadrant.cs añadido
  - [ ] Tag "BridgeQuadrant" asignado
  - [ ] Layer "Bridge" asignada

- [ ] **Materiales**
  - [ ] Materiales para cada capa (base, soporte, estructura, superficie)
  - [ ] Materiales para estado dañado
  - [ ] Materiales para estado destruido

- [ ] **Prefabs de Visualización**
  - [ ] Un prefab para cada una de las 4 capas con modelos 3D
  - [ ] Cada prefab tiene un Renderer (MeshRenderer o SkinnedMeshRenderer)

- [ ] **ScriptableObject de Cuadrante**
  - [ ] Crear vía Project > Create > Bridge > Quadrant
  - [ ] Configuración de las 4 capas con sus respectivos prefabs y materiales

- [ ] **Grilla de Construcción**
  - [ ] GameObject con BridgeConstructionGrid.cs
  - [ ] Asignado el ScriptableObject del cuadrante
  - [ ] Asignado el prefab del cuadrante
  - [ ] Creado un objeto vacío como QuadrantParent

- [ ] **Jugadores**
  - [ ] Componente PlayerBridgeInteraction en ambos jugadores
  - [ ] Transform vacío como buildPoint en la posición correcta
  - [ ] Layer "Bridge" seleccionada en el campo bridgeLayer

- [ ] **Materiales Recogibles**
  - [ ] Al menos un objeto con BridgeMaterialPickup para cada capa (0-3)
  - [ ] Prefab de material asignado a cada uno

## Problemas Comunes y Soluciones

### No aparece el menú para crear el ScriptableObject
- **Causa**: Unity no ha compilado los scripts o hay errores.
- **Solución**: Verificar si hay errores de compilación. Crear manualmente un script que herede de ScriptableObject como alternativa.

### Los Cuadrantes No Aparecen en la Escena
- **Causa**: El prefab del cuadrante no está asignado o la grilla no se inicializa.
- **Solución**: Verificar las referencias en BridgeConstructionGrid y asegurarse de que el quadrantParent esté asignado.

### No Puedo Construir en los Cuadrantes
1. **Causa**: El jugador no detecta los cuadrantes.
   - **Solución**: Verifica que la capa "Bridge" esté seleccionada en bridgeLayer y que interactionRange sea suficientemente grande.

2. **Causa**: Errores en PlayerObjectHolder.
   - **Solución**: Asegúrate de que handTransform esté asignado y que GetHeldObject() devuelva correctamente el objeto.

3. **Causa**: LayerMask incorrecta.
   - **Solución**: En PlayerBridgeInteraction, haz clic en el selector de bridgeLayer y selecciona solo "Bridge".

### Visualización de Capas No Aparece
- **Causa**: Prefabs de visualización sin Renderer o mal configurados.
- **Solución**: Asegurar que cada prefab de capa tiene un Renderer y está correctamente asignado en el ScriptableObject.

### Los Jugadores No Pueden Recoger Materiales
- **Causa**: Problemas con el sistema de interacción o con BridgeMaterialPickup.
- **Solución**: Verifica que el objeto tenga un Collider para ser detectado por el sistema de interacción.

### La Última Capa No Cambia de Estado
- **Causa**: Materiales dañados no asignados o lógica de daño incorrecta.
- **Solución**: Verifica que damagedMaterial y destroyedMaterial estén asignados en el ScriptableObject.

## Uso del Depurador

1. Añade el componente `BridgeDebugger.cs` a cualquier objeto en la escena.
2. Asigna la referencia a tu BridgeConstructionGrid.
3. Asigna un prefab de material de prueba.
4. Usa las teclas T e Y durante el juego para probar construcción e impactos.

## Debugging con Console
Hemos añadido muchos mensajes de Debug.Log para ayudar a identificar problemas. Abre la consola (Ctrl+Shift+C o Window > General > Console) para ver estos mensajes durante la ejecución.

## Contacto y Soporte
Si continúas teniendo problemas, proporciona los siguientes detalles:
1. Capturas de pantalla de la configuración de tu grilla
2. Log de errores de la consola
3. Descripción detallada del problema

¡Buena suerte con la implementación del sistema de puentes! 