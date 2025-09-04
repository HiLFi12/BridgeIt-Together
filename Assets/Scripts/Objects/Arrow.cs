using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Arrow : MonoBehaviour
{
    [Header("Collision Effect")]
    [SerializeField] private GameObject collisionEffectPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionEffectPrefab && collision.contacts.Length > 0)
        {
            Vector3 contactPoint = collision.contacts[0].point;
            Vector3 contactNormal = collision.contacts[0].normal;

            GameObject effect = Instantiate(collisionEffectPrefab, contactPoint, Quaternion.LookRotation(contactNormal));
        }

        Destroy(gameObject);
    }
}