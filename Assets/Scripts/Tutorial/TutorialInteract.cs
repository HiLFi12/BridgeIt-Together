using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tutorial
{
    [CreateAssetMenu(fileName = "TutorialInteract", menuName = "Tutorial/TutorialInteract", order = 2)]
    public class TutorialInteract : TutorialSO
    {
        private Collider[] _overlapBuffer;

        public override void UpdateTutorial()
        {
            base.UpdateTutorial();
            if (TutorialFinished || player == null) return;

            // Detectar input de interacción usando los campos privados del Player (interactAction e interactKey)
            bool pressed = false;
            var playerType = typeof(Player);

            var interactActionField = playerType.GetField("interactAction", BindingFlags.NonPublic | BindingFlags.Instance);
            var interactKeyField = playerType.GetField("interactKey", BindingFlags.NonPublic | BindingFlags.Instance);

            if (interactActionField != null)
            {
                var action = (InputAction)interactActionField.GetValue(player);
                if (action != null && action.triggered) pressed = true;
            }

            if (!pressed && interactKeyField != null)
            {
                var key = (KeyCode)interactKeyField.GetValue(player);
                if (Input.GetKeyDown(key)) pressed = true;
            }

            if (!pressed)
            {
                var pInput = player.PlayerInput;
                if (pInput != null)
                {
                    var action = pInput.actions.FindAction("Interact");
                    if (action != null && action.triggered) pressed = true;
                }
            }

            if (!pressed) return;

            // Obtener campos privados de Player por reflexión
            var lmField = playerType.GetField("interactionLayer", BindingFlags.NonPublic | BindingFlags.Instance);
            var ipField = playerType.GetField("interactionPoint", BindingFlags.NonPublic | BindingFlags.Instance);
            var interactablesField = playerType.GetField("interactables", BindingFlags.NonPublic | BindingFlags.Instance);

            LayerMask layerMask = ~0;
            Transform interactionPoint = null;
            Collider[] playerBuffer = null;
            if (lmField != null) layerMask = (LayerMask)lmField.GetValue(player);
            if (ipField != null) interactionPoint = (Transform)ipField.GetValue(player);
            if (interactablesField != null) playerBuffer = (Collider[])interactablesField.GetValue(player);

            Vector3 center = interactionPoint != null ? interactionPoint.position : player.transform.position;
            float radius = player.interactionRadius;
            int bufferSize = playerBuffer != null ? playerBuffer.Length : 8;
            if (_overlapBuffer == null || _overlapBuffer.Length != bufferSize) _overlapBuffer = new Collider[bufferSize];

            int found = Physics.OverlapSphereNonAlloc(center, radius, _overlapBuffer, layerMask.value, QueryTriggerInteraction.Collide);
            if (found == 0) return;

            var holder = player.GetComponent<PlayerObjectHolder>();
            bool paloEncendido = false;
            if (holder != null && holder.HasObjectInHand())
            {
                var palo = holder.GetHeldObject()?.GetComponent<PaloIgnifugo>();
                paloEncendido = palo != null && palo.EstaEncendido();
            }

            var candidatos = new System.Collections.Generic.List<IInteractable>();
            InteractPriority mejorPrioridad = InteractPriority.VeryLow;
            for (int i = 0; i < found; i++)
            {
                var col = _overlapBuffer[i];
                if (col == null) continue;
                if (holder != null && holder.HasObjectInHand())
                {
                    var heldObj = holder.GetHeldObject();
                    if (heldObj != null && (col.gameObject == heldObj || col.transform.IsChildOf(heldObj.transform)))
                        continue;
                }
                var candidato = col.GetComponentInParent<IInteractable>();
                if (candidato == null) continue;
                var torch = col.GetComponentInParent<TorchInteractable>();
                var prioridadEfectiva = candidato.InteractPriority;
                if (paloEncendido && torch != null) prioridadEfectiva = InteractPriority.VeryHigh;
                if (prioridadEfectiva > mejorPrioridad)
                {
                    mejorPrioridad = prioridadEfectiva;
                    candidatos.Clear();
                    candidatos.Add(candidato);
                }
                else if (prioridadEfectiva == mejorPrioridad)
                {
                    candidatos.Add(candidato);
                }
            }
            if (candidatos.Count > 0)
            {
                CompleteTutorial();
            }
        }
    }
}
