using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private GameObject[] backgrounds;

    private void Awake()
    {
        int randomIndex = Random.Range(0, backgrounds.Length);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].SetActive(i == randomIndex);
        }
    }
}