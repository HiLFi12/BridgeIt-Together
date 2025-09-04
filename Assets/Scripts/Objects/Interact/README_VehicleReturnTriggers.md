# Sistema de Triggers de Retorno de Vehículos

## Descripción
Este sistema permite configurar triggers que automáticamente devuelven vehículos al pool cuando los tocan. Es útil para crear zonas de "despawn" automático donde los vehículos desaparecen después de cruzar el puente o llegar a ciertas áreas.

## Componentes del Sistema

### 1. VehicleReturnTriggerManager
- **Función**: Gestiona todos los triggers de retorno de un AutoGenerator
- **Ubicación**: Se agrega automáticamente al GameObject del AutoGenerator
- **Funcionalidad**: Coordina todos los triggers y maneja la comunicación con el pool

### 2. VehicleReturnTrigger
- **Función**: Componente que se agrega a cada trigger individual
- **Ubicación**: Se agrega automáticamente a cada Collider asignado como trigger
- **Funcionalidad**: Detecta cuando un vehículo entra en el trigger

## Configuración en el AutoGenerator

### Campos Nuevos en el Inspector:
- **Triggers Retorno**: Array de Colliders que actuarán como triggers de retorno
- **Activar Triggers Al Iniciar**: Si activar automáticamente los triggers al inicio

## Cómo Usar

### 1. Configuración Básica:
1. Crea objetos vacíos en tu escena donde quieres que los vehículos desaparezcan
2. Agrega Colliders a estos objetos (Box, Sphere, etc.)
3. En el AutoGenerator, arrastra estos Colliders al array "Triggers Retorno"
4. Marca "Activar Triggers Al Iniciar" si quieres que funcionen desde el inicio

### 2. Configuración Avanzada por Código:
```csharp
// Obtener referencia al AutoGenerator
AutoGenerator autoGen = GetComponent<AutoGenerator>();

// Agregar un nuevo trigger
Collider nuevoTrigger = miObjetoTrigger.GetComponent<Collider>();
autoGen.AddReturnTrigger(nuevoTrigger, true);

// Activar/desactivar todos los triggers
autoGen.SetTriggersActive(true);

// Activar/desactivar un trigger específico
autoGen.SetTriggerActive(miTrigger, false);

// Reconfigurar todos los triggers
Collider[] nuevosTriggers = {trigger1, trigger2, trigger3};
autoGen.ReconfigureTriggers(nuevosTriggers, true);
```

## Métodos Públicos Disponibles

### Control de Triggers:
- `SetTriggersActive(bool active)`: Activa/desactiva todos los triggers
- `SetTriggerActive(Collider trigger, bool active)`: Controla un trigger específico
- `AddReturnTrigger(Collider newTrigger, bool activate)`: Agrega un nuevo trigger
- `RemoveReturnTrigger(Collider trigger)`: Remueve un trigger
- `ReconfigureTriggers(Collider[] newTriggers, bool activate)`: Reconfigura todos

### Información:
- `GetActiveTriggerCount()`: Obtiene el número de triggers activos

### Métodos de Contexto (Click derecho en el componente):
- "Activar Todos los Triggers"
- "Desactivar Todos los Triggers"

## Características del Sistema

### Detección Inteligente:
- Detecta automáticamente si un objeto es un vehículo basándose en los componentes `AutoMovement` y `VehicleBridgeCollision`
- Busca los componentes tanto en el objeto como en sus padres
- Solo procesa objetos que realmente son vehículos

### Gestión Automática:
- Los triggers se configuran automáticamente como `isTrigger = true`
- Se agregan los componentes necesarios automáticamente
- Limpieza automática al destruir el AutoGenerator

### Debugging Visual:
- Los triggers se muestran como wireframes rojos en la Scene View cuando están activos
- Se muestran en verde cuando están seleccionados y activos
- Se muestran en gris cuando están desactivados

### Logs Informativos:
- El sistema registra en consola cuando se configuran triggers
- Informa cuando un vehículo toca un trigger
- Reporta cambios de estado de los triggers

## Casos de Uso Típicos

### 1. Zonas de Salida:
Colocar triggers después del puente para que los vehículos desaparezcan después de cruzar.

### 2. Límites del Mapa:
Colocar triggers en los bordes del mapa para evitar que los vehículos salgan del área de juego.

### 3. Control Dinámico:
Activar/desactivar triggers según eventos del juego (ej: solo activar después de que se complete el puente).

### 4. Triggers Temporales:
Agregar triggers dinámicamente durante el gameplay y removerlos cuando ya no se necesiten.

## Notas Importantes

- Los triggers NO afectan a otros objetos que no sean vehículos
- Los vehículos se devuelven al pool instantáneamente al tocar un trigger activo
- Los triggers pueden activarse/desactivarse en tiempo real sin afectar el rendimiento
- El sistema es compatible con pools expandibles y no expandibles

## Troubleshooting

### Los triggers no funcionan:
1. Verificar que los Colliders están marcados como `isTrigger = true`
2. Verificar que los triggers están activos (`SetTriggersActive(true)`)
3. Verificar que los vehículos tienen los componentes necesarios

### Los vehículos no se detectan:
1. Verificar que tienen `AutoMovement` o `VehicleBridgeCollision`
2. Verificar que los Colliders de los vehículos no son triggers
3. Verificar la configuración de capas (Layers) si se usan

### Rendimiento:
- El sistema está optimizado para múltiples triggers
- No hay límite práctico en el número de triggers
- Los triggers inactivos no consumen recursos de detección
