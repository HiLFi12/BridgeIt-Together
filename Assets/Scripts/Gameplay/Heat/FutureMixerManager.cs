using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FutureMixerManager : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractPriority interactPriority = InteractPriority.High;
    [SerializeField] private BatterySystem batterySystem;
    [SerializeField] private Gameplay.Heat.FutureMixerCook futureMixerCook;
    [SerializeField] private float delayColocacion = 0.1f; // Delay antes de colocar

    [Header("Sistema Visual")]
    [SerializeField] private GameObject visualReference;
    [SerializeField] private Renderer[] objetosACambiarEmision; // Objetos con Renderer
    [SerializeField] private Color colorEmisionCargado = Color.blue;
    [SerializeField] private Color colorEmisionDescargado = Color.red;
    [SerializeField] private float intensidadEmisionCargado = 2f; // Intensidad HDR cuando está cargado
    [SerializeField] private float intensidadEmisionDescargado = 1f; // Intensidad HDR cuando está descargado
    [SerializeField] private float velocidadCambioColor = 2f;

    private Color[] coloresEmisionActuales;
    private Color colorEmisionObjetivo;
    private float intensidadEmisionActual;
    private float intensidadEmisionObjetivo;
    private Material[][] materialesInstanciados; // Materiales instanciados por objeto

    public InteractPriority InteractPriority => interactPriority;

    private void Awake()
    {
        if (batterySystem == null) batterySystem = GetComponentInChildren<BatterySystem>();
        if (futureMixerCook == null) futureMixerCook = GetComponent<Gameplay.Heat.FutureMixerCook>();
        
        // Inicializar los materiales instanciados y colores actuales
        if (objetosACambiarEmision != null && objetosACambiarEmision.Length > 0)
        {
            coloresEmisionActuales = new Color[objetosACambiarEmision.Length];
            materialesInstanciados = new Material[objetosACambiarEmision.Length][];
            
            for (int i = 0; i < objetosACambiarEmision.Length; i++)
            {
                if (objetosACambiarEmision[i] != null)
                {
                    // Crear materiales instanciados para este renderer
                    Material[] mats = objetosACambiarEmision[i].materials;
                    materialesInstanciados[i] = new Material[mats.Length];
                    
                    for (int j = 0; j < mats.Length; j++)
                    {
                        materialesInstanciados[i][j] = new Material(mats[j]); // Instanciar material
                    }
                    
                    objetosACambiarEmision[i].materials = materialesInstanciados[i];
                    
                    // Inicializar color de emisión actual
                    if (materialesInstanciados[i].Length > 0 && materialesInstanciados[i][0].HasProperty("_EmissionColor"))
                    {
                        coloresEmisionActuales[i] = materialesInstanciados[i][0].GetColor("_EmissionColor");
                    }
                    else
                    {
                        coloresEmisionActuales[i] = Color.black;
                    }
                }
            }
        }
    }

    private void Update()
    {
        ActualizarVisual();
    }
    
    public void TurnOnShadow()
    {
        // TODO: Implementar visualización de sombra/highlight
    }

    private void ActualizarVisual()
    {
        if (batterySystem == null) return;

        bool isCharged = batterySystem.IsCharged;

        // Activar/desactivar visual
        if (visualReference != null)
        {
            visualReference.SetActive(isCharged);
        }

        // Cambiar emisión de materiales progresivamente
        if (objetosACambiarEmision != null && objetosACambiarEmision.Length > 0)
        {
            colorEmisionObjetivo = isCharged ? colorEmisionCargado : colorEmisionDescargado;
            intensidadEmisionObjetivo = isCharged ? intensidadEmisionCargado : intensidadEmisionDescargado;

            for (int i = 0; i < objetosACambiarEmision.Length; i++)
            {
                if (objetosACambiarEmision[i] != null && materialesInstanciados[i] != null)
                {
                    // Interpolar el color de emisión actual hacia el color objetivo
                    coloresEmisionActuales[i] = Color.Lerp(coloresEmisionActuales[i], colorEmisionObjetivo, Time.deltaTime * velocidadCambioColor);
                    
                    // Interpolar la intensidad de emisión actual hacia la intensidad objetivo
                    intensidadEmisionActual = Mathf.Lerp(intensidadEmisionActual, intensidadEmisionObjetivo, Time.deltaTime * velocidadCambioColor);
                    
                    // Aplicar el color de emisión a todos los materiales del renderer
                    foreach (Material mat in materialesInstanciados[i])
                    {
                        if (mat != null && mat.HasProperty("_EmissionColor"))
                        {
                            mat.SetColor("_EmissionColor", coloresEmisionActuales[i] * intensidadEmisionActual);
                            
                            // Habilitar/deshabilitar emisión según si el color es negro o no
                            if (coloresEmisionActuales[i].maxColorComponent > 0.01f)
                            {
                                mat.EnableKeyword("_EMISSION");
                            }
                            else
                            {
                                mat.DisableKeyword("_EMISSION");
                            }
                        }
                    }
                }
            }
        }
    }

    public void Interact(GameObject player)
    {
        var holder = player.GetComponent<PlayerObjectHolder>();
        if (holder == null || !holder.HasObjectInHand()) 
        {
            // Si no tiene nada en la mano, intentar interactuar con la mezcladora
            // (para era medieval que requiere mantener presionado)
            futureMixerCook?.Interact(player);
            return;
        }

        GameObject heldObj = holder.GetHeldObject();
        
        // 1. Detectar el tipo INMEDIATAMENTE
        int itemType = DeterminarTipoItem(heldObj);
        
        if (itemType == 0)
        {
            Debug.Log("Objeto no válido para la mezcladora futurista");
            return;
        }

        // 2. Iniciar corutina para colocar después del delay
        StartCoroutine(ColocarConDelay(player, itemType));
    }

    private IEnumerator ColocarConDelay(GameObject player, int itemType)
    {
        Debug.Log($"Tipo detectado: {(itemType == 1 ? "Material tipo 3 (batería)" : "Material para mezclar")} - Esperando {delayColocacion}s...");
        
        // Esperar el delay configurado
        yield return new WaitForSeconds(delayColocacion);

        // 3. Ejecutar la colocación según el tipo
        switch (itemType)
        {
            case 1: // Material superficie (nuevo tipo 3) para cargar batería
                Debug.Log("Cargando batería con material tipo 3 (superficie)");
                batterySystem?.Interact(player);
                break;

            case 2: // Material tipo 1 o tipo 2 para mezclar
                Debug.Log("Colocando material en la mezcladora");
                futureMixerCook?.Interact(player);
                break;
        }
    }

    private int DeterminarTipoItem(GameObject objeto)
    {
        if (objeto == null) return 0;

        // Tipo 1: Material superficie (tipo 3) para cargar batería
        if (objeto.GetComponent<MaterialTipo4>() != null)
            return 1;

        BridgeMaterialInfo materialInfo = objeto.GetComponent<BridgeMaterialInfo>();
        
        // Verificar si es material tipo superficie (capa superior índice 2)
        if (materialInfo != null && materialInfo.layerIndex == 2)
            return 1;

        // Tipo 2: Material tipo 1 o tipo 2 (para mezclar)
        if (objeto.GetComponent<MaterialTipo1>() != null)
            return 2;
        
        if (materialInfo != null && 
            (materialInfo.layerIndex == 0 || materialInfo.layerIndex == 1))
            return 2;

        return 0;
    }
}
