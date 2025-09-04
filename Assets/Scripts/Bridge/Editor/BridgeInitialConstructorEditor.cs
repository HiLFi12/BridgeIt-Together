using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor personalizado para BridgeInitialConstructor que proporciona una interfaz más amigable
/// </summary>
[CustomEditor(typeof(BridgeInitialConstructor))]
public class BridgeInitialConstructorEditor : Editor
{
    private BridgeInitialConstructor script;
    private bool showQuadrantGrid = false;
    
    private void OnEnable()
    {
        script = (BridgeInitialConstructor)target;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bridge Initial Constructor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Este script permite configurar el puente para que aparezca construido hasta diferentes capas al inicio del nivel.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Configuración básica
        EditorGUILayout.LabelField("Configuración Básica", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bridgeGrid"));
        
        // Selector de capas inicial con descripción visual
        SerializedProperty layersProperty = serializedObject.FindProperty("initialConstructedLayers");
        EditorGUILayout.LabelField("Capas Iniciales a Construir", EditorStyles.boldLabel);
        
        int currentLayers = layersProperty.intValue;
        string[] layerDescriptions = {
            "0 - Sin construir (puente vacío)",
            "1 - Solo capa base (fundación)",
            "2 - Base + soporte (estructura básica)",
            "3 - Base + soporte + estructura (casi completo)",
            "4 - Puente completo (todas las capas)"
        };
        
        currentLayers = EditorGUILayout.Popup("Capas a Construir", currentLayers, layerDescriptions);
        layersProperty.intValue = currentLayers;
        
        // Mostrar descripción de lo que se construirá
        EditorGUILayout.HelpBox(GetLayerDescription(currentLayers), MessageType.None);
        
        EditorGUILayout.Space();
        
        // Configuración de cuadrantes
        EditorGUILayout.LabelField("Configuración de Cuadrantes", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("constructAllQuadrants"));
        
        SerializedProperty constructAllProperty = serializedObject.FindProperty("constructAllQuadrants");
        if (!constructAllProperty.boolValue)
        {
            EditorGUILayout.Space();
            showQuadrantGrid = EditorGUILayout.Foldout(showQuadrantGrid, "Seleccionar Cuadrantes Específicos", true);
            
            if (showQuadrantGrid)
            {
                DrawQuadrantGrid();
            }
            
            if (GUILayout.Button("Configurar Cuadrantes Específicos"))
            {
                script.ConfigureSpecificQuadrants();
                serializedObject.Update();
            }
        }
        
        EditorGUILayout.Space();
        
        // Estado de la última capa (solo si se construyen 4 capas)
        if (currentLayers == 4)
        {
            EditorGUILayout.LabelField("Estado de la Última Capa", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lastLayerState"));
            EditorGUILayout.HelpBox("Solo aplica cuando se construyen las 4 capas completas.", MessageType.Info);
            EditorGUILayout.Space();
        }
        
        // Configuración de debug
        EditorGUILayout.LabelField("Configuración de Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugMessages"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("applyOnStart"));
        
        EditorGUILayout.Space();
        
        // Botones de acción
        EditorGUILayout.LabelField("Acciones", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Construir Puente Inicial", GUILayout.Height(30)))
        {
            script.ConstructInitialBridge();
        }
        if (GUILayout.Button("Reinicializar Puente", GUILayout.Height(30)))
        {
            script.ResetBridge();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Validar Configuración"))
        {
            script.ValidateConfiguration();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private string GetLayerDescription(int layers)
    {
        switch (layers)
        {
            case 0: return "El puente no se construirá. Los jugadores deberán construir desde cero.";
            case 1: return "Solo se construirá la capa base (fundación). Los jugadores necesitarán añadir soporte, estructura y superficie.";
            case 2: return "Se construirán la base y el soporte. Los jugadores necesitarán añadir estructura y superficie.";
            case 3: return "Se construirán base, soporte y estructura. Los jugadores solo necesitarán añadir la superficie final.";
            case 4: return "El puente estará completamente construido. Útil para niveles de reparación o mantenimiento.";
            default: return "";
        }
    }
      private void DrawQuadrantGrid()
    {
        SerializedProperty specificQuadrantsProperty = serializedObject.FindProperty("specificQuadrants");
        
        if (specificQuadrantsProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox("Presiona 'Configurar Cuadrantes Específicos' para crear la grilla.", MessageType.Warning);
            return;
        }
        
        // Obtener dimensiones de la grilla usando SerializedProperty
        SerializedProperty bridgeGridProperty = serializedObject.FindProperty("bridgeGrid");
        BridgeConstructionGrid grid = bridgeGridProperty.objectReferenceValue as BridgeConstructionGrid;
        if (grid == null)
        {
            EditorGUILayout.HelpBox("Asigna primero el BridgeConstructionGrid.", MessageType.Warning);
            return;
        }
        
        EditorGUILayout.LabelField($"Grilla {grid.gridWidth}x{grid.gridLength}:", EditorStyles.boldLabel);
        
        // Dibujar la grilla de cuadrantes
        for (int x = 0; x < grid.gridWidth; x++)
        {
            EditorGUILayout.BeginHorizontal();
            
            for (int z = 0; z < grid.gridLength; z++)
            {
                int index = x * grid.gridLength + z;
                
                if (index < specificQuadrantsProperty.arraySize)
                {
                    SerializedProperty quadrantProperty = specificQuadrantsProperty.GetArrayElementAtIndex(index);
                    
                    // Cambiar color basado en si está seleccionado
                    Color originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = quadrantProperty.boolValue ? Color.green : Color.red;
                    
                    string buttonText = $"[{x},{z}]";
                    if (GUILayout.Toggle(quadrantProperty.boolValue, buttonText, "Button", GUILayout.Width(60)))
                    {
                        quadrantProperty.boolValue = true;
                    }
                    else
                    {
                        quadrantProperty.boolValue = false;
                    }
                    
                    GUI.backgroundColor = originalColor;
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space();
        
        // Botones de selección rápida
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Seleccionar Todos"))
        {
            for (int i = 0; i < specificQuadrantsProperty.arraySize; i++)
            {
                specificQuadrantsProperty.GetArrayElementAtIndex(i).boolValue = true;
            }
        }
        if (GUILayout.Button("Deseleccionar Todos"))
        {
            for (int i = 0; i < specificQuadrantsProperty.arraySize; i++)
            {
                specificQuadrantsProperty.GetArrayElementAtIndex(i).boolValue = false;
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
