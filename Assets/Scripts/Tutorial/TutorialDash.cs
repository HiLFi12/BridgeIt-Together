using UnityEngine;

[CreateAssetMenu(fileName = "TutorialDash", menuName = "Tutorial/TutorialDash", order = 3)]
public class TutorialDash : TutorialSO
{
    private bool wasDashing = false;
    
    public override void Initialize()
    {
        base.Initialize();
        wasDashing = false;
    }
    
    public override void UpdateTutorial()
    {
        if (player != null)
        {
            bool isDashing = player.IsDashing;
            if (isDashing && !wasDashing)
            {
                CompleteTutorial();
            }
            wasDashing = isDashing;
        }
    }
    
    public override void ResetTutorial()
    {
        base.ResetTutorial();
        wasDashing = false;
    }
}
