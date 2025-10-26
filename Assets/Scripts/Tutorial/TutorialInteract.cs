using UnityEngine;

[CreateAssetMenu(fileName = "TutorialInteract", menuName = "Tutorial/TutorialInteract", order = 2)]
public class TutorialInteract : TutorialSO
{
    public override void Initialize()
    {
        base.Initialize();
        if (player != null)
        {
            player.OnPlayerInteracted += CompleteTutorial;
        }
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        if (player != null)
        {
            player.OnPlayerInteracted -= CompleteTutorial;
        }
    }
}
