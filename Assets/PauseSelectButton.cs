using UnityEngine;
using UnityEngine.UI;

public class PauseSelectButton : MonoBehaviour
{
    public Button primaryButton;

    private void OnEnable()
    {
        if (primaryButton != null)
        {
            primaryButton.Select();
        }
    }
}
