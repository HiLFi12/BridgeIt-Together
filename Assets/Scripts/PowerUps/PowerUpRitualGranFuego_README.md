# Power Up Ritual del Gran Fuego - Era Prehistórica

## Descripción

El **Power Up Ritual del Gran Fuego** es el power up específico de la era prehistórica que sigue las especificaciones del documento de diseño. Los jugadores deben cooperar para encender ambas antorchas del tótem usando palos ignífugos encendidos.

## Funcionamiento según Especificaciones

### Mecánica de Activación
1. **Aparición**: Un tótem ritual aparece detrás del puente en la zona inicial del mapa
2. **Antorchas**: El tótem cuenta con dos antorchas, una a cada lado
3. **Interacción**: Los jugadores deben interactuar con cada antorcha usando un **PaloIgnifugo encendido**
4. **Consumo**: Al interactuar, la antorcha del tótem se enciende y la del jugador se destruye
5. **Sincronización**: Cada antorcha permanece encendida durante 1 segundo (margen de error)
6. **Activación**: Si ambas antorchas están encendidas simultáneamente, se activa el power up

### Efecto
- Construye automáticamente todos los cuadrantes del puente hasta la **capa 3** (índices 0, 1, 2)
- Solo afecta cuadrantes que no estén ya completos o no hayan alcanzado dicha capa
- El efecto es instantáneo al activarse

## Configuración del Prefab

### 1. Estructura del Tótem
```
PowerUpRitualGranFuego (GameObject principal)
├── Modelo3D_Totem
├── LeftTorchCollider (GameObject con Collider)
│   └── [Modelo de la antorcha izquierda]
└── RightTorchCollider (GameObject con Collider)
    └── [Modelo de la antorcha derecha]
```

### 2. Componente PowerUpRitualGranFuego
En el GameObject principal, configurar:

- **Left Torch Collider**: Asignar el GameObject con collider de la antorcha izquierda
- **Right Torch Collider**: Asignar el GameObject con collider de la antorcha derecha
- **Bridge Grid**: Se asigna automáticamente por el PowerUpSpawner
- **Torch Active Time**: 1.0 segundos (margen de error)
- **Torch Fire Effect Prefab**: Prefab del efecto visual de fuego
- **Duration**: 10 segundos (duración del efecto)
- **Time To Live**: 15 segundos (tiempo en escena)

### 3. Colliders de las Antorchas
Cada antorcha debe tener:
- **Collider**: Para detectar interacciones (Trigger activado)
- **TorchInteractable**: Se añade automáticamente por el script principal

## Dependencias

### Scripts Requeridos
- `PowerUpBase.cs` - Clase base de todos los power ups
- `TorchInteractable.cs` - Maneja la interacción con las antorchas
- `PaloIgnifugo.cs` - Objeto que los jugadores usan para encender las antorchas
- `PlayerObjectHolder.cs` - Sistema de inventario del jugador
- `BridgeConstructionGrid.cs` - Sistema de construcción de puentes

### Sistemas Relacionados
- **Sistema de Materiales**: Para obtener palos ignífugos
- **Sistema de Fogatas**: Para encender los palos ignífugos
- **Sistema de Puentes**: Para aplicar el efecto de construcción automática

## Flujo de Juego Completo

1. **Obtención de Palos**: Los jugadores obtienen palos ignífugos de generadores específicos
2. **Encendido**: Los jugadores encienden sus palos acercándose a fogatas
3. **Power Up Spawn**: El tótem ritual aparece aleatoriamente en el mapa
4. **Coordinación**: Ambos jugadores deben dirigirse al tótem
5. **Interacción**: Cada jugador interactúa con una antorcha diferente
6. **Sincronización**: Deben hacerlo dentro del margen de 1 segundo
7. **Activación**: El power up se activa y construye el puente automáticamente
8. **Efecto**: Todos los cuadrantes se construyen hasta la capa 3

## Consideraciones de Diseño

### Cooperación Forzada
- Requiere que ambos jugadores tengan palos ignífugos encendidos
- Necesita coordinación temporal precisa
- Fomenta la comunicación entre jugadores

### Balance
- **Preparación**: Requiere tiempo para obtener y encender los palos
- **Riesgo**: Los palos se apagan con el tiempo
- **Recompensa**: Construcción automática significativa del puente

### Feedback Visual/Sonoro
- Efectos de fuego en las antorchas del tótem
- Mensajes de debug para guiar a los jugadores
- Destrucción visual de los palos ignífugos al usarlos

## Extensibilidad

El sistema está diseñado para ser fácilmente extensible:

### Efectos Adicionales
- Añadir sonidos específicos en `LightLeftTorch()` y `LightRightTorch()`
- Implementar partículas de humo y chispas
- Añadir animaciones del tótem

### Configuración Avanzada
- Tiempo de antorcha encendida configurable
- Diferentes efectos de fuego por antorcha
- Múltiples tipos de materiales combustibles

## Troubleshooting

### Debug Step-by-Step

#### **PASO 1: Verificar Interacción Básica**
1. Abre la **Consola** de Unity
2. Juega y acércate a una antorcha con un PaloIgnifugo encendido
3. Presiona la tecla de interacción (E o P)
4. **¿Aparece algún mensaje en consola?** Deberías ver:
   - `"Necesitas un palo ignífugo encendido..."` (si no tienes palo)
   - `"¡Antorcha [izquierda/derecha] del tótem encendida!"` (si funciona)

#### **PASO 2: Verificar Configuración de Colliders**
**En cada antorcha (Left/Right objects):**
- ✅ **Is Trigger**: ACTIVADO
- ✅ **Layer**: Default o InteractionLayer 
- ✅ **Size**: Suficientemente grande para detectar al jugador
- ✅ **TorchInteractable**: Se añade automáticamente al jugar

#### **PASO 3: Verificar PaloIgnifugo**
- ¿El palo tiene llamas visibles?
- ¿El método `EstaEncendido()` devuelve true?
- Añade debug: `Debug.Log($"Palo encendido: {paloIgnifugo.EstaEncendido()}");`

#### **PASO 4: Verificar PlayerObjectHolder**
- ¿El jugador tiene el componente `PlayerObjectHolder`?
- ¿El método `HasObjectInHand()` devuelve true?
- ¿El método `GetHeldObject()` devuelve el palo?

### Problemas Comunes

#### **"No pasa nada al presionar E cerca de la antorcha"**
**Causas posibles:**
1. **El collider no es trigger** → Activar "Is Trigger"
2. **El jugador no tiene InteractionLayer** → Verificar LayerMask en Player
3. **El radio de interacción es muy pequeño** → Aumentar "Interaction Radius" en Player
4. **No hay TorchInteractable** → Se añade automáticamente, verificar en Play Mode

#### **"Sale mensaje pero la antorcha no se enciende"**
**Causas posibles:**
1. **PaloIgnifugo no está encendido** → Verificar efectos visuales de fuego
2. **Problemas con PlayerObjectHolder** → Verificar componente en jugador
3. **El power up no está disponible** → `isAvailable = false`

#### **"Las antorchas se encienden pero el power up no se activa"**
**Causas posibles:**
1. **Timing incorrecto** → Ambas deben estar encendidas simultáneamente (1 segundo)
2. **BridgeGrid no asignado** → Verificar referencia en inspector
3. **Power up ya usado** → `isAvailable = false` después de activarse

### Configuración de Debug

#### **En TorchInteractable.cs, añadir más logs:**
```csharp
public void Interact(GameObject interactor)
{
    Debug.Log($"🔥 INTERACCIÓN CON ANTORCHA {torchSide}");
    
    if (ritualPowerUp == null)
    {
        Debug.LogError("❌ No hay referencia al PowerUpRitualGranFuego");
        return;
    }

    PlayerObjectHolder playerObjectHolder = interactor.GetComponent<PlayerObjectHolder>();
    Debug.Log($"🎒 PlayerObjectHolder: {(playerObjectHolder != null ? "✅" : "❌")}");
    Debug.Log($"🖐️ Tiene objeto: {(playerObjectHolder?.HasObjectInHand() ?? false)}");
    
    if (playerObjectHolder?.HasObjectInHand() == true)
    {
        GameObject heldObject = playerObjectHolder.GetHeldObject();
        PaloIgnifugo palo = heldObject?.GetComponent<PaloIgnifugo>();
        Debug.Log($"🏒 Es PaloIgnifugo: {(palo != null ? "✅" : "❌")}");
        Debug.Log($"🔥 Está encendido: {(palo?.EstaEncendido() ?? false)}");
    }
}
```

#### **En PowerUpRitualGranFuego.cs, añadir en Update():**
```csharp
private void Update()
{
    if (!isAvailable) return;

    // Debug estado de antorchas
    if (leftTorchLit || rightTorchLit)
    {
        Debug.Log($"🕯️ Antorchas - Izq: {leftTorchLit} ({leftTorchTimer:F1}s), Der: {rightTorchLit} ({rightTorchTimer:F1}s)");
    }
    
    // ...resto del código...
}
```
