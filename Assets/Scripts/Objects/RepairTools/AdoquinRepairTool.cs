using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Herramienta de reparación de adoquines
/// </summary>
public class AdoquinRepairTool : MonoBehaviour
{
    [Header("Configuración de Herramienta")]
    [SerializeField] private int cantidadAdoquin = 1;
    [SerializeField] private BridgeQuadrantSO.EraType era = BridgeQuadrantSO.EraType.Prehistoric;
    
    [Header("Visual")]
    [SerializeField] private GameObject modeloVisual;
    
    private bool fueUsada = false;
    
    /// <summary>
    /// Cantidad de adoquín que proporciona esta herramienta
    /// </summary>
    public int CantidadAdoquin => cantidadAdoquin;
    
    /// <summary>
    /// Era de construcción compatible con esta herramienta
    /// </summary>
    public BridgeQuadrantSO.EraType Era => era;
    
    /// <summary>
    /// Indica si la herramienta ya fue utilizada
    /// </summary>
    public bool FueUsada => fueUsada;
      private void Start()
    {
        ConfigurarModeloVisual();
    }
    
    private void ConfigurarModeloVisual()
    {
        if (modeloVisual != null)
        {
            // Aquí se puede configurar la apariencia visual de la herramienta
            // Por ejemplo, cambiar materiales según la era
        }
    }
      /// <summary>
    /// Información para debugging
    /// </summary>
    public void MostrarInfo()
    {
        Debug.Log($"Herramienta de Adoquín - Cantidad: {cantidadAdoquin}, Era: {era}, Usada: {fueUsada}");
    }
}
