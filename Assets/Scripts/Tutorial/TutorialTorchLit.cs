using UnityEngine;

[CreateAssetMenu(fileName = "TutorialTorchLit", menuName = "Tutorial/TutorialTorchLit", order =6)]
public class TutorialTorchLit : TutorialSO
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
 // Chequear al inicializar por si ya tiene el palo encendido
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
 if (held == null) return;

 var palo = held.GetComponent<PaloIgnifugo>();
 if (palo != null && palo.EstaEncendido())
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
