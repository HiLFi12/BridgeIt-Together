using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "TutorialMove", menuName = "Tutorial/TutorialMove", order = 2)]
public class TutorialMove : TutorialSO
{
    public override void UpdateTutorial()
    {
        if (player != null && player.PlayerController != null)
        {
            // Usar MovementInput de PlayerController para mayor fiabilidad
            if (player.PlayerController.MovementInput.magnitude > 0.1f)
            {
                CompleteTutorial();
            }
        }
    }
}
