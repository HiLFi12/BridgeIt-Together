using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialTrunk", menuName = "Tutorial/TutorialTrunk", order =7)]
public class TutorialTrunk : TutorialSO
{
 public override void Initialize()
 {
 base.Initialize();

 // Subscribe to the global event when resin is extracted
 CortezaResistente.OnResinExtracted -= OnResinExtracted;
 CortezaResistente.OnResinExtracted += OnResinExtracted;
 }

 private void OnResinExtracted(GameObject interactor)
 {
 if (this.TutorialFinished) return;

 if (this.player == null) return;

 if (interactor == this.player.gameObject)
 {
 this.CompleteTutorial();
 }
 }

 public override void ResetTutorial()
 {
 base.ResetTutorial();
 CortezaResistente.OnResinExtracted -= OnResinExtracted;
 }
}
