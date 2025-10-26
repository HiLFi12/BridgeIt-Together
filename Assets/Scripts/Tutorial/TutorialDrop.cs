using UnityEngine;

[CreateAssetMenu(fileName = "TutorialDrop", menuName = "Tutorial/TutorialDrop", order =2)]
public class TutorialDrop : TutorialSO
{
    private bool wasHolding = false;
    private PlayerObjectHolder holder;

    public override void Initialize()
    {
        if (player != null)
        {
            holder = player.GetComponent<PlayerObjectHolder>();
            if (holder != null)
            {
                wasHolding = holder.HasObjectInHand();
            }
        }
    }

    public override void UpdateTutorial()
    {
        if (TutorialFinished || holder == null) return;

        bool isHolding = holder.HasObjectInHand();
        if (wasHolding && !isHolding)
        {
            CompleteTutorial();
        }
        wasHolding = isHolding;
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        wasHolding = false;
        holder = null;
    }
}
