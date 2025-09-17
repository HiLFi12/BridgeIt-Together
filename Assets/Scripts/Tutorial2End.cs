// csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class Tutorial2End : MonoBehaviour
{
    [Header("Condición")]
    [SerializeField] private string requiredItemTag = "BridgeMaterial1";

    [Header("Entrada del jugador")]
    [SerializeField] private KeyCode activationKeyPrimary = KeyCode.F;
    [SerializeField] private KeyCode activationKeySecondary = KeyCode.L;

    [Header("Siguiente nivel")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float loadDelaySeconds = 0.25f;

    private bool triggered;
    private int materialsInside;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(requiredItemTag))
            materialsInside++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(requiredItemTag))
            materialsInside = Mathf.Max(0, materialsInside - 1);
    }

    private void Update()
    {
        if (triggered) return;
        if (string.IsNullOrWhiteSpace(nextSceneName)) return;
        if (materialsInside <= 0) return;

        if (Input.GetKeyDown(activationKeyPrimary) || Input.GetKeyDown(activationKeySecondary))
        {
            triggered = true;
            StartCoroutine(LoadNextSceneAfterDelay());
        }
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        if (loadDelaySeconds > 0f)
            yield return new WaitForSeconds(loadDelaySeconds);

        SceneManager.LoadScene(nextSceneName);
    }
}