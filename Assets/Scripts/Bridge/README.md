# Sistema de Puentes - Bridge it Together!

## Descripción General
Este sistema implementa el mecanismo de construcción de puentes para el juego "Bridge it Together!". Los puentes se construyen por cuadrantes, cada uno compuesto por 4 capas que los jugadores deben colocar en orden específico.

## Características Principales
- Puentes divididos en cuadrantes configurables
- Cada cuadrante requiere 4 capas de materiales (base, soporte, estructura, superficie)
- Los cuadrantes solo tienen colisión cuando están completamente construidos
- Sistema de daño que varía según la época histórica
- Interacción cooperativa entre dos jugadores

## Configuración del Sistema

### 1. Crear el Prefab del Cuadrante
1. Crea un objeto vacío en la escena y nómbralo "BridgeQuadrantPrefab"
2. Añade un componente `BoxCollider` y configura su tamaño (por ejemplo, 1x0.1x1)
3. Añade el script `BridgeQuadrant.cs`
4. Establece el tag "BridgeQuadrant" (o será establecido automáticamente al iniciar)
5. Convierte el objeto en prefab arrastrándolo a la carpeta Prefabs

### 2. Crear ScriptableObjects para los Cuadrantes por Era
1. En el Project, haz clic derecho > Create > Bridge > Quadrant
2. Crea un SO para cada era (Prehistoric, Medieval, Industrial, Contemporary, Futuristic)
3. Configura los parámetros específicos de cada era

### 3. Configurar la Grilla de Construcción
1. Crea un objeto vacío en la escena y nómbralo "BridgeGrid"
2. Añade el componente `BridgeConstructionGrid.cs`
3. Configura:
   - El tamaño de la grilla (ancho y largo)
   - El tamaño de cada cuadrante
   - Referencia al ScriptableObject del cuadrante por defecto
   - Referencia al prefab del cuadrante
   - Un Transform vacío como padre para los cuadrantes

### 4. Configurar los Jugadores
1. Para cada objeto de jugador, añade el componente `PlayerBridgeInteraction.cs`
2. Configura:
   - Referencia a la grilla de puente (BridgeGrid)
   - Un punto de construcción (Transform vacío en las manos del jugador)
   - Radio de interacción
   - Capa de interacción con el puente

### 5. Crear Materiales de Construcción
1. Crea prefabs para cada tipo de material de construcción (por cada capa y era)
2. Para cada objeto recolectable en el nivel:
   - Añade el componente `BridgeMaterialPickup.cs`
   - Configura el prefab del material que se dará al jugador
   - Establece el índice de capa (0-3) que representa
   - Selecciona la era correspondiente
   - Configura el tiempo de reaparición

### 6. Configurar Vehículos (opcional)
1. Para cada vehículo en el juego, añade el componente `VehicleBridgeCollision.cs`
2. Asegúrate de que tengan Rigidbody y Collider
3. Configura el umbral de fuerza para causar daño

## Sistema de Controles
- Jugador 1: E para interactuar, Q para soltar objetos, F para construir
- Jugador 2: P para interactuar, O para soltar objetos, L para construir

## Épocas y Mecánicas Específicas

### Prehistoria y Medieval
- Desgaste por uso
- La última capa se daña después de cierta cantidad de usos
- Si ya está dañada, un uso adicional la destruye

### Era Industrial
- Sistema de temperatura
- La última capa pierde temperatura con el tiempo
- Los jugadores deben usar objetos calentadores para mantener la temperatura

### Era Contemporánea
- Sistema de daño probabilístico
- Cada vehículo que pasa tiene una probabilidad de dañar la capa superior
- Si ya está dañada, una segunda pasada la destruye

### Era Futurista
- Sistema de baterías
- La última capa funciona con baterías que se agotan con el tiempo
- Los jugadores deben reemplazar las baterías agotadas

## Consejos de Implementación
- Asegúrate de configurar correctamente las capas y etiquetas en Unity
- Crea materiales distintivos para cada capa y sus estados (normal, dañado, destruido)
- Utiliza efectos de partículas para los eventos de construcción y destrucción
- Configura sonidos para la construcción, daño y reparación 