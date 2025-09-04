using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneReference
{
    [SerializeField] private int buildIndex = -1;
    [SerializeField] private string sceneName = "";

    public bool IsValid()
    {
        return buildIndex >= 0 || !string.IsNullOrEmpty(sceneName);
    }

    public void Load()
    {
        if (buildIndex >= 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(buildIndex);
            return;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            return;
        }

        Debug.LogError("SceneReference: no scene assigned to load");
    }

    public void SetFromSceneName(string name)
    {
        sceneName = name;
        buildIndex = -1; // Reset build index when setting by name
    }

#if UNITY_EDITOR
    public void SetFromSceneAsset(SceneAsset sceneAsset)
    {
        if (sceneAsset == null)
        {
            buildIndex = -1;
            sceneName = "";
            return;
        }

        var path = AssetDatabase.GetAssetPath(sceneAsset);
        sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

        // Buscar índice en BuildSettings
        var scenes = EditorBuildSettings.scenes;
        buildIndex = -1;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path == path)
            {
                buildIndex = i;
                break;
            }
        }
    }

    public void SetFromSceneNameEditor(string name)
    {
        sceneName = name;
        // intentar encontrar buildIndex por nombre en BuildSettings
        buildIndex = -1;
        var scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            var p = scenes[i].path;
            var n = System.IO.Path.GetFileNameWithoutExtension(p);
            if (n == name)
            {
                buildIndex = i;
                break;
            }
        }
    }
#endif
}
