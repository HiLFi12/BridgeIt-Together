using UnityEngine;

public class ToggleObjectOnEnable : MonoBehaviour
{
    [SerializeField] private GameObject objectToDisable;

    private void OnEnable()
    {
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(true);
        }
    }
}

