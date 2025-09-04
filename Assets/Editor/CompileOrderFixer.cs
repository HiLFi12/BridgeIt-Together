using UnityEditor;

// Este script es solo para asegurar que VehiclePool se compile antes que AutoGenerator
[InitializeOnLoad]
public class CompileOrderFixer
{
    static CompileOrderFixer()
    {
        // No hacemos nada en el constructor, solo queremos que el script sea compilado
    }
}
