using UnityEngine;

[CreateAssetMenu(fileName = "TutorialAmbar", menuName = "Tutorial/TutorialAmbar", order =8)]
public class TutorialAmbar : TutorialSO
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
 // Chequear al inicializar por si ya tiene el material
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
 if (held != null && held.GetComponent<MaterialTipo2CombinarPalo>() != null)
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
