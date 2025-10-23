using UnityEngine;
using UnityEngine.UI;

public class PauseSelectButton : MonoBehaviour
{
    public Button primaryButton;

    private void Start()
    {
        primaryButton.Select();
    }
}
