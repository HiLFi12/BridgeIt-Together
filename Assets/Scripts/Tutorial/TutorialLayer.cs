using UnityEngine;

[CreateAssetMenu(fileName = "TutorialLayer", menuName = "Tutorial/TutorialLayer", order =5)]
public class TutorialLayer : TutorialSO
{
    private PlayerBridgeInteraction bridgeInteraction;

    public override void Initialize()
    {
        base.Initialize();

        if (player == null) return;

        bridgeInteraction = player.GetComponent<PlayerBridgeInteraction>();
        if (bridgeInteraction != null)
        {
            bridgeInteraction.OnTryBuildAttempt -= OnTryBuildAttempt;
            bridgeInteraction.OnTryBuildAttempt += OnTryBuildAttempt;
        }
    }

    private void OnTryBuildAttempt()
    {
        if (this.TutorialFinished) return;
        CompleteTutorial();
    }

    public override void ResetTutorial()
    {
        base.ResetTutorial();
        if (bridgeInteraction != null)
        {
            bridgeInteraction.OnTryBuildAttempt -= OnTryBuildAttempt;
            bridgeInteraction = null;
        }
    }
}
