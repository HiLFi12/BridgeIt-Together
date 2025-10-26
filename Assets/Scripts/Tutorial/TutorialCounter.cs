using UnityEngine;

[CreateAssetMenu(fileName = "TutorialCounter", menuName = "Tutorial/TutorialCounter", order = 3)]
public class TutorialCounter : TutorialSO
{
    [Header("Counter Settings")]
    [SerializeField] private float initialCount = 1f;

    private float currentCount;

    public override void Initialize()
    {
        base.Initialize();
        currentCount = initialCount;
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        currentCount = initialCount;
    }

    public override void UpdateTutorial()
    {
        if (currentCount > 0)
        {
            currentCount -= Time.deltaTime;
            if (currentCount <= 0)
            {
                CompleteTutorial();
            }
        }
    }
}
