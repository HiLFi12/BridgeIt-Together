using System.Collections;
using UnityEngine;

public class TutorialPopupActivator : MonoBehaviour
{
    [Header("Referencia (dejar vacío para autolocalizar)")]
    [SerializeField] private TutorialPopup tutorialPopup;

    [Header("Opciones")]
    [SerializeField] private bool onlyFirstSession = false;
    [SerializeField] private string playerPrefsKey = "TutorialShown";

    private void Awake()
    {
        if (tutorialPopup == null)
        {
            tutorialPopup = FindObjectOfType<TutorialPopup>(true);
        }
    }

    private IEnumerator Start()
    {
        yield return null;

        if (tutorialPopup == null) yield break;

        if (onlyFirstSession && PlayerPrefs.GetInt(playerPrefsKey, 0) == 1)
            yield break; 

        if (!tutorialPopup.gameObject.activeSelf)
        {
            tutorialPopup.gameObject.SetActive(true); 
            if (onlyFirstSession)
            {
                PlayerPrefs.SetInt(playerPrefsKey, 1);
                PlayerPrefs.Save();
            }
        }
    }

    [ContextMenu("Reset Tutorial Flag")]
    public void ResetTutorialFlag()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
    }
}
