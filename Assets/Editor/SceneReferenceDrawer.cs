using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var buildIndexProp = property.FindPropertyRelative("buildIndex");
        var sceneNameProp = property.FindPropertyRelative("sceneName");

        EditorGUI.BeginProperty(position, label, property);

        Rect r0 = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(r0, label);

        Rect r1 = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

        // Attempt to find a SceneAsset corresponding to stored sceneName/buildIndex
        SceneAsset sceneAsset = null;
        if (buildIndexProp != null && buildIndexProp.intValue >= 0)
        {
            var scenes = EditorBuildSettings.scenes;
            if (buildIndexProp.intValue < scenes.Length)
            {
                var path = scenes[buildIndexProp.intValue].path;
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            }
        }
        else if (!string.IsNullOrEmpty(sceneNameProp.stringValue))
        {
            var scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                var path = scenes[i].path;
                var n = System.IO.Path.GetFileNameWithoutExtension(path);
                if (n == sceneNameProp.stringValue)
                {
                    sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                    break;
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        var newAsset = (SceneAsset)EditorGUI.ObjectField(r1, "Scene", sceneAsset, typeof(SceneAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            if (newAsset == null)
            {
                buildIndexProp.intValue = -1;
                sceneNameProp.stringValue = "";
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(newAsset);
                sceneNameProp.stringValue = System.IO.Path.GetFileNameWithoutExtension(path);

                // find index in build settings
                var scenes = EditorBuildSettings.scenes;
                int idx = -1;
                for (int i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i].path == path)
                    {
                        idx = i;
                        break;
                    }
                }
                buildIndexProp.intValue = idx;
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2) * 2;
    }
}
