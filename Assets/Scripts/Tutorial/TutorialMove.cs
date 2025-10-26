using UnityEngine;

[CreateAssetMenu(fileName = "TutorialMove", menuName = "Tutorial/TutorialMove", order = 4)]
public class TutorialMove : TutorialSO
{
    public override void UpdateTutorial()
    {
        if (player != null && player.PlayerController != null)
        {
            if (player.PlayerController.MovementInput.magnitude > 0.1f)
            {
                CompleteTutorial();
            }
        }
    }
}
