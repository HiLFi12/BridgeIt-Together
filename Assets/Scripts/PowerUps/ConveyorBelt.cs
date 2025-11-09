namespace PowerUps
{
    using UnityEngine;
    using System.Collections;

    public class ConveyorBelt : MonoBehaviour, IInteractable
    {
        [Header("Puntos de la cinta")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [Header("Velocidad de movimiento")]
        public float velocidad = 2f;
        [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;
        [Header("Objetos que rotan con la cinta")]
        [SerializeField] private GameObject[] objetosRotativos;

        public InteractPriority InteractPriority => interactPriority;

        public void Interact(GameObject player)
        {
            var holder = player.GetComponent<PlayerObjectHolder>();
            if (holder == null || !holder.HasObjectInHand()) return;
            GameObject objeto = holder.GetHeldObject();
            // Soltar el objeto sin destruirlo
            if (holder.GetType().GetMethod("DropObject") != null)
                holder.DropObject();
            else
                holder.PickUpExistingInstance(null);
            if (objeto == null || startPoint == null || endPoint == null) return;
            objeto.SetActive(true);
            objeto.transform.position = startPoint.position;
            objeto.transform.rotation = startPoint.rotation;
            StartCoroutine(MoverObjeto(objeto));
        }

        private IEnumerator MoverObjeto(GameObject objeto)
        {
            var rb = objeto.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            while (Vector3.Distance(objeto.transform.position, endPoint.position) > 0.05f)
            {
                objeto.transform.position = Vector3.MoveTowards(objeto.transform.position, endPoint.position, velocidad * Time.deltaTime);
                yield return null;
            }
            objeto.transform.position = endPoint.position;
            if (rb != null) rb.isKinematic = false;
        }

        private void Update()
        {
            if (objetosRotativos != null && objetosRotativos.Length > 0)
            {
                float rotacion = velocidad / 2f * 360f * Time.deltaTime;
                foreach (var obj in objetosRotativos)
                {
                    if (obj != null)
                        obj.transform.Rotate(rotacion, 0f, 0f, Space.Self);
                }
            }
        }

        public void TurnOnShadow()
        {
            // TODO: Implementar visualización de sombra/highlight
        }
    }
}