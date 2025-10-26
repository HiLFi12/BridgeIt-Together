using UnityEngine;

[CreateAssetMenu(fileName = "TutorialTorch", menuName = "Tutorial/TutorialTorch", order =4)]
public class TutorialTorch : TutorialSO
{
 private PlayerObjectHolder holder;

 public override void Initialize()
 {
 base.Initialize();

 if (player != null)
 {
 holder = player.GetComponent<PlayerObjectHolder>();
 if (holder != null)
 {
 // Chequear al inicializar por si ya tiene el palo
 CheckAndComplete();
 }
 }
 }

 public override void UpdateTutorial()
 {
 if (TutorialFinished || holder == null) return;

 CheckAndComplete();
 }

 private void CheckAndComplete()
 {
 if (holder == null) return;
 if (!holder.HasObjectInHand()) return;

 var held = holder.GetHeldObject();
 if (held != null && held.GetComponent<PaloIgnifugo>() != null)
 {
 CompleteTutorial();
 }
 }

 public override void ResetTutorial()
 {
 base.ResetTutorial();
 holder = null;
 }
}
