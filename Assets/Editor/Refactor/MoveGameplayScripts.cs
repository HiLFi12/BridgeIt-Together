#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MoveGameplayScripts
{
    private const string TargetRoot = "Assets/Scripts/BridgeItTogether"; // nueva carpeta raíz

    [MenuItem("Tools/Refactor/Move Gameplay Scripts to BridgeItTogether Root")] 
    public static void Move()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            // Crear carpeta destino si no existe
            if (!AssetDatabase.IsValidFolder(TargetRoot))
            {
                var parts = TargetRoot.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }

            MoveIfExists("Assets/Scripts/Gameplay/Carriles", TargetRoot + "/Gameplay/Carriles");
            MoveIfExists("Assets/Scripts/Gameplay/Spawning", TargetRoot + "/Gameplay/Spawning");
            MoveIfExists("Assets/Scripts/Gameplay/Rondas", TargetRoot + "/Gameplay/Rondas");
            MoveIfExists("Assets/Scripts/Gameplay/Abstractions", TargetRoot + "/Gameplay/Abstractions");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
        Debug.Log("[Refactor] Movimiento completado a: " + TargetRoot);
    }

    private static void MoveIfExists(string src, string dst)
    {
        if (AssetDatabase.IsValidFolder(src) || AssetDatabase.LoadAssetAtPath<Object>(src) != null)
        {
            string parent = System.IO.Path.GetDirectoryName(dst).Replace("\\", "/");
            string name = System.IO.Path.GetFileName(dst);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                // Crear jerarquía intermedia
                var parts = parent.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
            AssetDatabase.MoveAsset(src, dst);
        }
    }
}
#endif
