# Sistema Anti-Caídas para Jugadores

Este sistema implementa la funcionalidad descrita en la especificación:

> "No pueden caer fuera del mapa: se utiliza OverlapSphereNonAlloc para detectar si están saliendo del área de movimiento. Si se detecta una posición fuera del límite, el personaje es suavemente reposicionado hacia el interior del área válida."

## Componentes del Sistema

1. **WalkableSurfaceDetector.cs**: Componente principal que detecta superficies caminables y previene caídas.
2. **PlayerSafetyManager.cs**: Gestor centralizado que configura automáticamente todos los jugadores en la escena.
3. **Integración con PlayerController.cs**: El controlador de movimiento ahora responde a la detección de superficies.

## Cómo Configurar

### Método 1: Configuración Automática (Recomendado)

1. Añade el prefab `PlayerSafetyManager` a tu escena.
2. Configura la capa `walkableSurfaceLayer` para que coincida con la capa de tus superficies caminables.
3. ¡Listo! El manager detectará automáticamente todos los jugadores y configurará el sistema.

### Método 2: Configuración Manual

1. Añade el componente `WalkableSurfaceDetector` a cada jugador.
2. Configura la capa `walkableSurfaceLayer` para cada detector.
3. Asegúrate de que cada jugador tenga un `CharacterController`.

## Cómo Funciona

1. El detector crea un punto de verificación debajo del jugador.
2. **Sistema de detección en tres niveles**:
   - **Nivel 1**: Utiliza `Physics.OverlapSphereNonAlloc` para detectar superficies caminables específicas.
   - **Nivel 2**: Utiliza `Physics.RaycastNonAlloc` como respaldo para detectar CUALQUIER terreno debajo del jugador.
   - **Nivel 3**: Monitorea la velocidad vertical y tiempo de caída para detectar caídas prolongadas.
3. Si el jugador está sobre una superficie válida (de cualquier tipo), se guarda la posición como "posición segura".
4. Si el jugador está por caerse o ya está cayendo, se reposiciona a la última posición segura.
5. Antes de cada movimiento, se verifica si la posición futura tendría superficie. Si no, se reduce la magnitud del movimiento.

## Ajustes Disponibles

### Detección de Superficies Caminables
- **groundDetectionRadius**: Radio de la esfera de detección (por defecto: 0.5).
- **safeRepositionDistance**: Distancia de reposicionamiento cuando se detecta una caída (por defecto: 0.3).
- **walkableSurfaceLayer**: Capa que se considera como superficie caminable específica.
- **showDebugSphere**: Muestra esferas de depuración en el editor.

### Detección de Vacío
- **raycastDistance**: Distancia máxima del raycast para detectar terreno (por defecto: 2.0).
- **anyGroundLayer**: Capa(s) que se consideran como terreno válido (por defecto: todas).
- **useRaycastFallback**: Activa/desactiva la detección de terreno genérico con raycast.
- **fallVelocityThreshold**: Umbral de velocidad vertical para considerar que el jugador está cayendo.
- **fallCheckInterval**: Intervalo para verificar la velocidad de caída.

### Reposicionamiento
- **useDirectTeleport**: Si es true, teletransporta directamente al jugador a la posición segura.
- **repositionDuration**: Duración de la transición suave si no se usa teletransporte directo.
- **maxFallTime**: Tiempo máximo permitido de caída antes de forzar el reposicionamiento.

## Sistemas de Seguridad Redundantes

El sistema implementa múltiples capas de seguridad para garantizar que el jugador no caiga al vacío:

1. **Prevención Proactiva**: Reduce el movimiento cuando el jugador se acerca a un borde.
2. **Detección de Superficie**: Detecta cuando el jugador está sobre una superficie no válida.
3. **Detección de Caída**: Monitorea la velocidad vertical y el tiempo de caída.
4. **Reposicionamiento Forzado**: Si el jugador ha estado cayendo por demasiado tiempo, lo reposiciona automáticamente.

## Depuración

En modo de juego, verás:
- Esferas verdes: El jugador está sobre una superficie segura.
- Esferas rojas: El jugador está sobre una superficie no segura (se reposicionará).
- Esferas azules: Última posición segura registrada.
- Líneas cyan: Raycast para detección de terreno genérico.

## Notas Técnicas

- El sistema utiliza `OverlapSphereNonAlloc` y `RaycastNonAlloc` para optimizar el rendimiento.
- Se implementa un sistema de eventos para notificar cuando se detecta una posición insegura.
- La reposición se realiza mediante teletransporte directo o transición suave configurable.
- El sistema de detección en tres niveles proporciona redundancia para garantizar que el jugador nunca caiga indefinidamente. 