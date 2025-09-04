using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BridgeConstructionGrid))]
public class BridgeConstructionGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Dibujar el inspector por defecto
        DrawDefaultInspector();

        // Añadir separador
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Herramientas de Escalado", EditorStyles.boldLabel);

        BridgeConstructionGrid grid = (BridgeConstructionGrid)target;

        // Botón para reescalar la grilla
        if (GUILayout.Button("Reescalar Grilla"))
        {
            grid.RescaleGrid();
            EditorUtility.SetDirty(target);
        }

        // Información útil
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Usa 'Reescalar Grilla' después de cambiar el Quadrant Size para ajustar " +
            "automáticamente el tamaño de todos los cuadrantes y capas existentes.",
            MessageType.Info
        );        // Mostrar información de la grilla si está inicializada
        if (Application.isPlaying && grid != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Estado de la Grilla", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Ancho: {grid.gridWidth}");
            EditorGUILayout.LabelField($"Largo: {grid.gridLength}");
            EditorGUILayout.LabelField($"Tamaño de cuadrante: {grid.quadrantSize}");
            
            // Sección de Debug Tools solo en Play Mode
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Herramientas de Debug", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Esta herramienta completa automáticamente todas las capas de todos los cuadrantes del puente. " +
                "Útil para testing rápido del sistema de vehículos.",
                MessageType.Info
            );
            
            // Botón para rellenar todo el puente
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🔧 Rellenar Todo el Puente", GUILayout.Height(30)))
            {
                grid.DebugRellenarTodoPuente();
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
