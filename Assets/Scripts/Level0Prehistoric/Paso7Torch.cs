using UnityEngine;
using TMPro;

public class Paso7Torch : MonoBehaviour
{
    [Header("Texto del Paso Actual")]
    [SerializeField] private TMP_Text textoPrompt;

    [Header("Flechas hacia fuentes (verde)")]
    [SerializeField] private GameObject flechaFuenteP1;
    [SerializeField] private GameObject flechaFuenteP2;

    [Header("Jugadores (chequeo automático)")]
    [SerializeField] private Transform jugador1;
    [SerializeField] private Transform jugador2;
    [Tooltip("Si el item se parenta a una mano/holder, asignar aquí (sino deja null para usar el root del jugador).")]
    [SerializeField] private Transform raizChequeoP1;
    [SerializeField] private Transform raizChequeoP2;

    [Header("Siguiente Paso")]
    [SerializeField] private GameObject proximoPaso;

    private bool _p1Tiene;
    private bool _p2Tiene;
    private bool _completado;

    private void OnEnable()
    {
        if (textoPrompt) textoPrompt.gameObject.SetActive(true);
        if (flechaFuenteP1) flechaFuenteP1.SetActive(true);
        if (flechaFuenteP2) flechaFuenteP2.SetActive(true);

        _p1Tiene = false;
        _p2Tiene = false;
        _completado = false;
    }

    private void Update()
    {
        if (_completado) return;

        var raiz1 = raizChequeoP1 ? raizChequeoP1 : jugador1;
        var raiz2 = raizChequeoP2 ? raizChequeoP2 : jugador2;

        if (raiz1) _p1Tiene = TienePaloIgnifugo(raiz1);
        if (raiz2) _p2Tiene = TienePaloIgnifugo(raiz2);

        if (_p1Tiene && _p2Tiene)
            CompletarPaso();
    }

    private static bool TienePaloIgnifugo(Transform raiz)
    {
        if (raiz == null) return false;
        // Busca el script PaloIgnifugo en cualquier hijo (incluye inactivos)
        var palo = raiz.GetComponentInChildren<PaloIgnifugo>(true);
        return palo != null;
    }

    private void CompletarPaso()
    {
        if (_completado) return;
        _completado = true;

        if (proximoPaso) proximoPaso.SetActive(true);

        if (textoPrompt) textoPrompt.gameObject.SetActive(false);
        if (flechaFuenteP1) flechaFuenteP1.SetActive(false);
        if (flechaFuenteP2) flechaFuenteP2.SetActive(false);

        gameObject.SetActive(false);
    }

    // Opcional para flujos por eventos (si no se parenta el objeto al jugador):
    public void NotificarTienePaloIgnifugo(int playerIndex, bool tiene)
    {
        if (playerIndex == 1) _p1Tiene = tiene;
        else if (playerIndex == 2) _p2Tiene = tiene;

        if (_p1Tiene && _p2Tiene)
            CompletarPaso();
    }
}
