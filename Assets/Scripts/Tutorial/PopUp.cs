using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    public GameObject[] paginas;      
    public TMP_Text contador;        
    public Button botonSiguiente;
    public Button botonAnterior;

    private int paginaActual = 0;

    private float previousTimeScale = 1f;
    private bool pausedByPopup = false;

    void Start()
    {
        ActualizarPagina();

        botonSiguiente.onClick.AddListener(PaginaSiguiente);
        botonAnterior.onClick.AddListener(PaginaAnterior);
    }

    private void OnEnable()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        pausedByPopup = true;

        paginaActual = 0;
        if (paginas != null && paginas.Length > 0)
        {
            ActualizarPagina();
        }
    }

    private void OnDisable()
    {
        if (pausedByPopup)
        {
            Time.timeScale = previousTimeScale;
            pausedByPopup = false;
        }
    }

    void ActualizarPagina()
    {
        for (int i = 0; i < paginas.Length; i++)
            paginas[i].SetActive(i == paginaActual);

        if (contador != null)
            contador.text = $"Página {paginaActual + 1} de {paginas.Length}";

        if (botonAnterior != null)
            botonAnterior.gameObject.SetActive(paginaActual > 0);

        if (botonSiguiente != null)
        {
            TMP_Text btnTxt = botonSiguiente.GetComponentInChildren<TMP_Text>();
            if (btnTxt != null)
                btnTxt.text = (paginaActual == paginas.Length - 1) ? "Cerrar" : "Siguiente";
        }
    }

    public void PaginaSiguiente()
    {
        if (paginaActual < paginas.Length - 1)
        {
            paginaActual++;
            ActualizarPagina();
        }
        else
        {
            CloseTutorial();
        }
    }

    public void PaginaAnterior()
    {
        if (paginaActual > 0)
        {
            paginaActual--;
            ActualizarPagina();
        }
    }

    public void CloseTutorial()
    {
        gameObject.SetActive(false);
    }
}
