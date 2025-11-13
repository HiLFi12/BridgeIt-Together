using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Arrow : MonoBehaviour
{
    [Header("Collision Effect")]
    [SerializeField] private GameObject collisionEffectPrefab;

    [Header("Audio - Destrucción (AudioManager)")]
    [Tooltip("Índice en AudioManager.soundEffects para reproducir cuando la flecha se destruye al colisionar. -1 desactiva.")]
    [SerializeField] private int destroySfxIndex = -1;

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionEffectPrefab && collision.contacts.Length > 0)
        {
            Vector3 contactPoint = collision.contacts[0].point;
            Vector3 contactNormal = collision.contacts[0].normal;

            GameObject effect = Instantiate(collisionEffectPrefab, contactPoint, Quaternion.LookRotation(contactNormal));
        }

        PlayDestroySfx();

        Destroy(gameObject);
    }

    private void PlayDestroySfx()
    {
        if (destroySfxIndex < 0) return;
        var audio = FindFirstObjectByType<AudioManager>();
        if (audio != null)
        {
            audio.PlaySFX(destroySfxIndex);
        }
    }
}