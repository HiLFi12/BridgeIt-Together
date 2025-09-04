using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Obsoleto: usar MaterialTipo2Base + scripts de mecánicas (MaterialTipo2CombinarPalo, etc.)
[AddComponentMenu("")]
public class MaterialTipo2 : MonoBehaviour
{
    private void Awake()
    {
        if (GetComponent<MaterialTipo2Base>() == null)
        {
            gameObject.AddComponent<MaterialTipo2Base>();
        }
        if (GetComponent<MaterialTipo2CombinarPalo>() == null)
        {
            gameObject.AddComponent<MaterialTipo2CombinarPalo>();
        }
        Debug.LogWarning("MaterialTipo2 obsoleto. Se añadieron MaterialTipo2Base y MaterialTipo2CombinarPalo.");
        Destroy(this);
    }
}