using UnityEngine;

public class ContemporaryMaterial2 : MaterialTipo2Base
{
    [Header("Visual Object")]
    [SerializeField] private GameObject visualObject;

    public bool IsReady => isReady; // expone el campo heredado

    private void Awake()
    {
        if (visualObject != null)
            visualObject.SetActive(isReady);
    }

    public void SetReady()
    {
        if (!isReady)
        {
            isReady = true;
            if (visualObject != null)
                visualObject.SetActive(true);
        }
    }

    public void ResetReady()
    {
        if (isReady)
        {
            isReady = false;
            if (visualObject != null)
                visualObject.SetActive(false);
        }
    }
}