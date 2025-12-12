using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeStarsUI : MonoBehaviour
{
    [Header("Star UI")]
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;

    private List<GameObject> stars = new List<GameObject>();
    private int currentLives;

    private void Start()
    {
        currentLives = maxLives;

        if (layoutGroup != null)
        {
            for (int i = 0; i < maxLives; i++)
            {
                var star = Instantiate(starPrefab, layoutGroup.transform);
                stars.Add(star);
            }
        }
    }

    public void LoseLife()
    {
        if (currentLives <= 0) return;

        currentLives--;

        if (currentLives >= 0 && currentLives < stars.Count)
        {
            var image = stars[currentLives].GetComponent<Image>();
            image.enabled = false;
        }
    }

    public void ResetLives()
    {
        currentLives = maxLives;
        
        for (int i = 0; i < stars.Count; i++)
        {
            stars[i].SetActive(true);
        }
    }
    
    /// <summary>
    /// Obtiene la cantidad actual de vidas restantes
    /// </summary>
    /// <returns>Número de vidas actuales</returns>
    public int GetCurrentLives()
    {
        return currentLives;
    }
}