// csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        [Tooltip("Array de Imágenes para mostrar en este paso del tutorial")]
        public Image[] stepUIs;

        [Tooltip("ScriptableObject que define la tarea de este paso")]
        public TutorialSO tutorialSo;
    }

    [Header("Tutorial Steps")]
    [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [Header("Player Reference")]
    [SerializeField] private Player player;

    private int _currentStepIndex = -1;
    private List<TutorialSO> clonedTutorials = new List<TutorialSO>();

    private void Start()
    {
        if (player == null) player = GetComponent<Player>();

        // Inicializar todos los TutorialSO clonados
        clonedTutorials.Clear();
        foreach (var step in tutorialSteps)
        {
            if (step.tutorialSo != null)
            {
                TutorialSO clone = Instantiate(step.tutorialSo);
                clone.ResetTutorial();
                clone.player = player;
                clone.Initialize();

                // Ensure no duplicate subscription
                clone.OnTutorialCompleted -= OnTutorialCompleted;
                clone.OnTutorialCompleted += OnTutorialCompleted;

                clonedTutorials.Add(clone);
            }
            else
            {
                clonedTutorials.Add(null);
            }
        }

        // Comenzar desde el primer paso si hay pasos
        if (tutorialSteps.Count > 0)
        {
            AdvanceToStep(0);
        }
    }

    private void Update()
    {
        // Actualizar el TutorialSO actual si existe
        if (_currentStepIndex >= 0 && _currentStepIndex < clonedTutorials.Count)
        {
            var currentSO = clonedTutorials[_currentStepIndex];
            if (currentSO != null)
            {
                currentSO.UpdateTutorial();
            }
        }
    }

    private void OnTutorialCompleted(TutorialSO completedTutorial)
    {
        // Find the index of the completed clone
        int index = clonedTutorials.IndexOf(completedTutorial);
        if (index < 0)
        {
            // Not one of our clones: ignore
            return;
        }

        // Only react if the completed tutorial is the currently shown step
        if (index != _currentStepIndex)
        {
            return;
        }

        // Turn off UI for the current step
        if (_currentStepIndex >= 0 && _currentStepIndex < tutorialSteps.Count)
        {
            var currentStep = tutorialSteps[_currentStepIndex];
            foreach (var ui in currentStep.stepUIs)
            {
                if (ui != null)
                {
                    ui.gameObject.SetActive(false);
                }
            }
        }

        // Advance to next step or finish
        if (_currentStepIndex >= tutorialSteps.Count - 1)
        {
            _currentStepIndex = -1;
            Debug.Log("Tutorial completado.");
        }
        else
        {
            AdvanceToStep(_currentStepIndex + 1);
        }
    }

    public void AdvanceTutorial()
    {
        int nextIndex = _currentStepIndex + 1;

        if (nextIndex >= tutorialSteps.Count)
        {
            // Turn off current UI
            if (_currentStepIndex >= 0 && _currentStepIndex < tutorialSteps.Count)
            {
                var currentStep = tutorialSteps[_currentStepIndex];
                foreach (var ui in currentStep.stepUIs)
                {
                    if (ui != null)
                    {
                        ui.gameObject.SetActive(false);
                    }
                }
            }
            _currentStepIndex = -1;
            Debug.Log("Tutorial completado.");
            return;
        }

        AdvanceToStep(nextIndex);
    }

    private void AdvanceToStep(int stepIndex)
    {
        // Apagar UI del paso actual
        if (_currentStepIndex >= 0 && _currentStepIndex < tutorialSteps.Count)
        {
            var currentStep = tutorialSteps[_currentStepIndex];
            foreach (var ui in currentStep.stepUIs)
            {
                if (ui != null)
                {
                    ui.gameObject.SetActive(false);
                }
            }
        }

        // Cambiar al nuevo paso
        _currentStepIndex = stepIndex;

        // Prender UI del nuevo paso
        if (_currentStepIndex >= 0 && _currentStepIndex < tutorialSteps.Count)
        {
            var newStep = tutorialSteps[_currentStepIndex];
            foreach (var ui in newStep.stepUIs)
            {
                if (ui != null)
                {
                    ui.gameObject.SetActive(true);
                }
            }

            Debug.Log($"Avanzado al paso del tutorial: {_currentStepIndex} - {clonedTutorials[_currentStepIndex]?.TutorialName ?? "Sin nombre"}");
        }
    }

    // Método público para forzar avance (útil para debug)
    public void ForceAdvance()
    {
        AdvanceTutorial();
    }

    // Método para resetear el tutorial
    public void ResetTutorial()
    {
        // Apagar todas las UIs
        foreach (var step in tutorialSteps)
        {
            foreach (var ui in step.stepUIs)
            {
                if (ui != null)
                {
                    ui.gameObject.SetActive(false);
                }
            }
        }

        // Destruir clones existentes y quitar suscripciones
        foreach (var clone in clonedTutorials)
        {
            if (clone != null)
            {
                clone.OnTutorialCompleted -= OnTutorialCompleted;
                Destroy(clone);
            }
        }
        clonedTutorials.Clear();

        // Recrear clones frescos
        foreach (var step in tutorialSteps)
        {
            if (step.tutorialSo != null)
            {
                TutorialSO clone = Instantiate(step.tutorialSo);
                clone.ResetTutorial();
                clone.player = player;
                clone.Initialize();

                // Ensure no duplicate subscription
                clone.OnTutorialCompleted -= OnTutorialCompleted;
                clone.OnTutorialCompleted += OnTutorialCompleted;

                clonedTutorials.Add(clone);
            }
            else
            {
                clonedTutorials.Add(null);
            }
        }

        _currentStepIndex = -1;

        // Reiniciar desde el principio si hay pasos
        if (tutorialSteps.Count > 0)
        {
            AdvanceToStep(0);
        }
    }

    // Propiedad para obtener el paso currente
    public int CurrentStepIndex => _currentStepIndex;

    public TutorialSO CurrentTutorialSO => (_currentStepIndex >= 0 && _currentStepIndex < clonedTutorials.Count) ? clonedTutorials[_currentStepIndex] : null;
}