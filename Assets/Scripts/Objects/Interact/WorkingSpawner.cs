using UnityEngine;

public class WorkingSpawner : PlasterBucketSpawner
{
    [Header("Sistema de trabajo")]
    [SerializeField] private bool isWorking = false;
    [SerializeField] private GameObject visualReference;

    public bool IsWorking => isWorking;

    private void Update()
    {
        // Actualizar la visual según el estado de isWorking
        if (visualReference != null)
        {
            visualReference.SetActive(isWorking);
        }
    }

    public override void Interact(GameObject interactor)
    {
        // Solo permitir interactuar si isWorking es true
        if (!isWorking)
        {
            Debug.Log("El spawner no está listo todavía. Espera a que se complete el proceso.");
            return;
        }

        // Llamar a la interacción base (spawner normal)
        base.Interact(interactor);

        // Después de agarrar el objeto, desactivar isWorking
        isWorking = false;
        Debug.Log("Objeto recogido. El spawner ya no está activo.");
    }

    // Método público para activar el spawner (llamado desde FutureMixerCook)
    public void ActivateSpawner()
    {
        isWorking = true;
        Debug.Log("Spawner activado. El objeto está listo para ser recogido.");
    }

    // Método público para desactivar manualmente si es necesario
    public void DeactivateSpawner()
    {
        isWorking = false;
    }
}
