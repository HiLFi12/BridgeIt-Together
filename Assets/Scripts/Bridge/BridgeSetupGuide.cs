using UnityEngine;

// Este script es solo una guía, no necesita ser añadido a ningún objeto
public class BridgeSetupGuide : MonoBehaviour
{
    /*
    GUÍA COMPLETA PARA CONFIGURAR UN PUENTE PREHISTÓRICO
    ====================================================
    
    1. CONFIGURACIÓN DE TAGS Y LAYERS
    ---------------------------------
    - Crea un tag llamado "BridgeQuadrant" (Edit → Tags and Layers → Tags → + → "BridgeQuadrant")
    - Crea una layer llamada "Bridge" (Edit → Tags and Layers → Layers → User Layer 8 → "Bridge")
    
    2. CREAR EL CUADRANTE BASE
    --------------------------
    - Crea un objeto vacío y nómbralo "BridgeQuadrantPrefab"
    - Añade un BoxCollider (Add Component → Physics → Box Collider)
      * Size: X=1, Y=0.1, Z=1
    - Añade el script BridgeQuadrant.cs
    - Asigna el tag "BridgeQuadrant"
    - Asigna la capa "Bridge"
    - Arrastra a la carpeta Prefabs para crear el prefab
    
    3. CREAR MATERIALES VISUALES
    ---------------------------
    - Crea los siguientes materiales en Assets/Materials:
      * PrehistoricBase (color marrón oscuro)
      * PrehistoricSupport (color marrón medio)
      * PrehistoricStructure (color marrón)
      * PrehistoricSurface (color beige)
      * PrehistoricDamaged (color beige con tinte rojizo)
      * PrehistoricDestroyed (color negro/quemado)
    
    4. CREAR PREFABS DE VISUALIZACIÓN DE CAPAS
    -----------------------------------------
    Para cada capa (0-3), crea un modelo visual:
    
    a) Capa 0 (Base):
      - Crea un cubo (GameObject → 3D Object → Cube)
      - Escala: X=0.95, Y=0.05, Z=0.95
      - Posición Y = 0
      - Asigna material PrehistoricBase
      - Arrastra a Prefabs/BridgeLayers/Prehistoric/Base
    
    b) Capa 1 (Soporte):
      - Crea 4 cilindros pequeños en las esquinas
      - Escala cada uno: X=0.1, Y=0.2, Z=0.1
      - Agrúpalos en un objeto padre
      - Asigna material PrehistoricSupport
      - Arrastra a Prefabs/BridgeLayers/Prehistoric/Support
    
    c) Capa 2 (Estructura):
      - Crea un cubo aplanado
      - Escala: X=0.9, Y=0.05, Z=0.9
      - Posición Y = 0.25
      - Asigna material PrehistoricStructure
      - Arrastra a Prefabs/BridgeLayers/Prehistoric/Structure
    
    d) Capa 3 (Superficie):
      - Crea un cubo muy aplanado
      - Escala: X=1, Y=0.02, Z=1
      - Posición Y = 0.3
      - Asigna material PrehistoricSurface
      - Arrastra a Prefabs/BridgeLayers/Prehistoric/Surface
    
    5. CREAR SCRIPTABLE OBJECT DEL CUADRANTE
    ---------------------------------------
    - Project view → Click derecho → Create → Bridge → Quadrant
    - Nombra el archivo como "PrehistoricBridgeQuadrant"
    - Configura:
      * Era: Prehistoric
      * Required Layers:
        - Element 0:
          * Layer Name: "Base"
          * Visual Prefab: (arrastra el prefab de Base)
          * Material: PrehistoricBase
        - Element 1:
          * Layer Name: "Support"
          * Visual Prefab: (arrastra el prefab de Support)
          * Material: PrehistoricSupport
        - Element 2:
          * Layer Name: "Structure"
          * Visual Prefab: (arrastra el prefab de Structure)
          * Material: PrehistoricStructure
        - Element 3:
          * Layer Name: "Surface"
          * Visual Prefab: (arrastra el prefab de Surface)
          * Material: PrehistoricSurface
      * Has Collision: false (se activará automáticamente cuando esté completo)
      * Last Layer State: Complete
      * Max Uses Before Damage: 10
      * Current Uses: 0
      * Damaged Material: PrehistoricDamaged
      * Destroyed Material: PrehistoricDestroyed
      * (Opcional) Sonidos y efectos
    
    6. CREAR PREFABS DE MATERIALES RECOLECTABLES
    ------------------------------------------
    Para cada material de construcción (0-3), crea un objeto recogible:
    
    a) Material Base:
      - Crea un objeto vacío "BasePickup"
      - Añade un modelo visual como hijo (un montón de tierra o piedras)
      - Añade SphereCollider (radio = 0.5)
      - Añade el script BridgeMaterialPickup
      - Configura:
        * Material Prefab: (arrastra el prefab de Base)
        * Layer Index: 0
        * Era: Prehistoric
        * Respawn Time: 5
      - Arrastra a Prefabs/Pickups
    
    (Repite para Support, Structure y Surface con sus respectivos índices 1, 2 y 3)
    
    7. CONFIGURAR LA GRILLA DE CONSTRUCCIÓN
    -------------------------------------
    - Crea un objeto vacío "BridgeGrid"
    - Añade el componente BridgeConstructionGrid
    - Configura:
      * Grid Width: 3 (o el ancho deseado)
      * Grid Length: 5 (o el largo deseado)
      * Quadrant Size: 1
      * Default Quadrant SO: (arrastra PrehistoricBridgeQuadrant)
      * Quadrant Prefab: (arrastra BridgeQuadrantPrefab)
      * Quadrant Parent: (crea y arrastra un objeto vacío hijo como contenedor)
      * Show Debug Grid: true (para visualización)
    
    8. CONFIGURAR LOS JUGADORES
    -------------------------
    Para cada jugador (Player1 y Player2):
    
    - Selecciona el objeto del jugador
    - Añade el componente PlayerBridgeInteraction
    - Configura:
      * Bridge Grid: (arrastra el objeto BridgeGrid)
      * Build Point: (crea y arrastra un Transform vacío en las manos del jugador)
      * Interaction Range: 2
      * Bridge Layer: selecciona la capa "Bridge"
    
    9. COLOCAR OBJETOS RECOLECTABLES EN LA ESCENA
    -------------------------------------------
    - Coloca instancias de los prefabs BasePickup, SupportPickup, StructurePickup y SurfacePickup
      en puntos estratégicos alrededor del nivel.
    
    10. PROBAR EL SISTEMA
    -------------------
    - Asegúrate de que los controles estén configurados:
      * Jugador 1: E (interactuar), Q (soltar), F (construir)
      * Jugador 2: P (interactuar), O (soltar), L (construir)
    - Inicia el juego y verifica:
      * Recoger materiales con E/P
      * Soltar materiales con Q/O
      * Construir parte del puente con F/L cuando estés cerca de la grilla
    */
} 