using UnityEngine;
using TMPro;
using System;

public class Paso11Rock : MonoBehaviour
{
    [Header("Texto del Paso Actual")]
    [SerializeField] private TMP_Text textoPrompt;

    [Header("Flechas hacia el source (verde)")]
    [SerializeField] private GameObject flechaSourceP1;
    [SerializeField] private GameObject flechaSourceP2;

    [Header("Jugadores (chequeo automático)")]
    [SerializeField] private Transform jugador1;
    [SerializeField] private Transform jugador2;
    [Tooltip("Si los items se parentan a una mano/holder específico, asignarlo aquí para cada jugador.")]
    [SerializeField] private Transform raizChequeoP1;
    [SerializeField] private Transform raizChequeoP2;

    [Header("Flujo de Tutorial")]
    [SerializeField] private GameObject proximoPaso;
    [Tooltip("layerIndex del material requerido (0=Base, 1=Soporte, 2=Superficie/Estructura).")]
    [Range(0, 2)]
    [SerializeField] private int tipoMaterialObjetivo = 2;   // Objetivo: layer 2
    [SerializeField] private bool detectarAutomaticamente = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool _p1Tiene;
    private bool _p2Tiene;
    private bool _completado;

    private void OnEnable()
    {
        if (textoPrompt) textoPrompt.gameObject.SetActive(true);
        if (flechaSourceP1) flechaSourceP1.SetActive(true);
        if (flechaSourceP2) flechaSourceP2.SetActive(true);

        _p1Tiene = false;
        _p2Tiene = false;
        _completado = false;
    }

    private void OnValidate()
    {
        tipoMaterialObjetivo = Mathf.Clamp(tipoMaterialObjetivo, 0, 2);
    }

    private void Update()
    {
        if (_completado) return;

        if (detectarAutomaticamente)
        {
            if (jugador1 || raizChequeoP1)
                _p1Tiene = JugadorTieneMaterial(raizChequeoP1 ? raizChequeoP1 : jugador1, tipoMaterialObjetivo, debugLogs);
            if (jugador2 || raizChequeoP2)
                _p2Tiene = JugadorTieneMaterial(raizChequeoP2 ? raizChequeoP2 : jugador2, tipoMaterialObjetivo, debugLogs);
        }

        if (debugLogs)
            Debug.Log($"[Paso11Rock] P1Tiene={_p1Tiene} | P2Tiene={_p2Tiene} | layer={tipoMaterialObjetivo}", this);

        if (_p1Tiene && _p2Tiene)
            CompletarPaso();
    }

    // Úsalo si tu flujo no parenta el item al jugador.
    public void NotificarPickup(int playerIndex, BridgeMaterialPickup pickup, bool agarrado)
    {
        if (pickup == null) return;
        bool esObjetivo = pickup.layerIndex == tipoMaterialObjetivo;

        if (playerIndex == 1 && esObjetivo) _p1Tiene = agarrado;
        else if (playerIndex == 2 && esObjetivo) _p2Tiene = agarrado;

        if (_p1Tiene && _p2Tiene)
            CompletarPaso();
    }

    private static bool JugadorTieneMaterial(Transform raiz, int layerIndexObjetivo, bool debug)
    {
        if (raiz == null) return false;

        // 1) BridgeMaterialInfo (si lo usan al emparentar en mano)
        var infos = raiz.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < infos.Length; i++)
        {
            var comp = infos[i];
            if (comp == null) continue;
            var t = comp.GetType();
            if (t.Name != "BridgeMaterialInfo") continue;

            if (TryGetLayerIndexFromInfo(comp, out int idx))
            {
                if (debug) Debug.Log($"[Paso11Rock] BridgeMaterialInfo en {comp.gameObject.name} -> layerIndex={idx}");
                if (idx == layerIndexObjetivo) return true;
            }
        }

        // 2) Fallback: BridgeMaterialPickup
        var pickups = raiz.GetComponentsInChildren<BridgeMaterialPickup>(true);
        for (int i = 0; i < pickups.Length; i++)
        {
            var p = pickups[i];
            if (p != null && p.layerIndex == layerIndexObjetivo)
            {
                if (debug) Debug.Log($"[Paso11Rock] Pickup {p.gameObject.name} -> layerIndex={p.layerIndex}");
                return true;
            }
        }

        return false;
    }

    private static bool TryGetLayerIndexFromInfo(MonoBehaviour info, out int index)
    {
        index = 0;
        var t = info.GetType();

        var f = t.GetField("layerIndex") ?? t.GetField("LayerIndex");
        if (f != null && f.FieldType == typeof(int))
        {
            index = (int)f.GetValue(info);
            return true;
        }

        var p = t.GetProperty("layerIndex") ?? t.GetProperty("LayerIndex");
        if (p != null && p.PropertyType == typeof(int))
        {
            index = (int)p.GetValue(info);
            return true;
        }

        var m = t.GetMethod("GetLayerIndex", Type.EmptyTypes);
        if (m != null && m.ReturnType == typeof(int))
        {
            index = (int)m.Invoke(info, null);
            return true;
        }

        return false;
    }

    private void CompletarPaso()
    {
        if (_completado) return;
        _completado = true;

        if (proximoPaso) proximoPaso.SetActive(true);

        if (textoPrompt) textoPrompt.gameObject.SetActive(false);
        if (flechaSourceP1) flechaSourceP1.SetActive(false);
        if (flechaSourceP2) flechaSourceP2.SetActive(false);

        gameObject.SetActive(false);
    }
}
