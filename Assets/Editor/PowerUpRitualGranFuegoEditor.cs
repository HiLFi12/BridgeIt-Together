#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PowerUpRitualGranFuego))]
public class PowerUpRitualGranFuegoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        // Campo Script (solo lectura)
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        }

        // Sección de Configuración General: solo lifeDuration
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Configuración General", EditorStyles.boldLabel);
        var lifeDurationProp = serializedObject.FindProperty("lifeDuration");
        if (lifeDurationProp != null)
        {
            EditorGUILayout.PropertyField(lifeDurationProp);
        }

        EditorGUILayout.Space();

        // Dibujar el resto de propiedades excluyendo las heredadas que no aplican
        DrawPropertiesExcluding(serializedObject, "m_Script", "duration", "timeToLive", "lifeDuration");
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
