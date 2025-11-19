using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Detectar si el objeto que entró es un Player
        Player player = other.GetComponent<Player>();
        
        if (player != null)
        {
            Debug.Log($"[RespawnTrigger] Player detectado, llamando a Respawn()");
            player.Respawn();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        
        // Intentar obtener el collider para mostrar su tamaño
        Collider col = GetComponent<Collider>();
        
        if (col != null)
        {
            if (col is BoxCollider boxCol)
            {
                // Dibujar el box collider con su tamaño y posición
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                // Si es un sphere collider, dibujarlo como esfera
                Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
            }
            else
            {
                // Para cualquier otro tipo de collider, dibujar un cubo genérico
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }
        }
        else
        {
            // Si no hay collider, dibujar un cubo de 1x1x1 en la posición del objeto
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}
