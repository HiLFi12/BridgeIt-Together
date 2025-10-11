using UnityEngine;

public class PlasterBucketSpawnerWithCooldown : PlasterBucketSpawner
{
    [Header("Cooldown en segundos para agarrar el objeto")]
    [SerializeField] private float cooldownTime = 2f;
    [Header("Referencia visual que se apaga/prende")]
    [SerializeField] private GameObject visualReference;

    private float lastInteractTime = -Mathf.Infinity;

    private void Update()
    {
        bool available = IsAvailable();
        if (visualReference != null)
        {
            visualReference.SetActive(available);
        }
    }

    private bool IsAvailable()
    {
        return Time.time - lastInteractTime >= cooldownTime;
    }

    public override void Interact(GameObject interactor)
    {
        if (!IsAvailable())
        {
            Debug.Log("Cooldown activo, espera para agarrar el objeto.");
            return;
        }
        base.Interact(interactor);
        lastInteractTime = Time.time;
    }
}

