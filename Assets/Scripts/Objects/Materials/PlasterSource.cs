namespace Objects.Materials
{
    using UnityEngine;

    public class PlasterSource : MonoBehaviour, IInteractable
    {
        [SerializeField] private InteractPriority interactPriority = InteractPriority.High;
        public InteractPriority InteractPriority => interactPriority;

        public void Interact(GameObject player)
        {
            var holder = player.GetComponent<PlayerObjectHolder>();
            if (holder == null || !holder.HasObjectInHand()) return;

            GameObject heldObj = holder.GetHeldObject();
            if (heldObj == null) return;

            var matReady = heldObj.GetComponent<MaterialTipo2Ready>();
            if (matReady != null && !matReady.IsReady)
            {
                matReady.ActivateMaterial();
                Debug.Log("MaterialTipo2Ready activado por PlasterSource");
            }
        }
        
        public void TurnOnShadow()
        {
            // TODO: Implementar visualización de sombra/highlight
        }
    }
}