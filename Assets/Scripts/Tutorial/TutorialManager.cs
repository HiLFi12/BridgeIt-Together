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
    
    private void Start()
    {
        if (player == null) player = GetComponent<Player>();
        
        // Inicializar todos los TutorialSO
        foreach (var step in tutorialSteps)
        {
            if (step.tutorialSo != null)
            {
                step.tutorialSo.player = player;
                step.tutorialSo.Initialize();
                step.tutorialSo.OnTutorialCompleted += OnTutorialCompleted;
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
        if (_currentStepIndex >= 0 && _currentStepIndex < tutorialSteps.Count)
        {
            var currentSO = tutorialSteps[_currentStepIndex].tutorialSo;
            if (currentSO != null)
            {
                currentSO.UpdateTutorial();
            }
        }
    }
    
    private void OnTutorialCompleted(TutorialSO completedTutorial)
    {
        // No auto-advance
        if (_currentStepIndex == tutorialSteps.Count - 1)
        {
            // Turn off UI for the last step
            var currentStep = tutorialSteps[_currentStepIndex];
            foreach (var ui in currentStep.stepUIs)
            {
                if (ui != null)
                {
                    ui.gameObject.SetActive(false);
                }
            }
            _currentStepIndex = -1;
            Debug.Log("Tutorial completado.");
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
            
            Debug.Log($"Avanzado al paso del tutorial: {_currentStepIndex} - {newStep.tutorialSo?.TutorialName ?? "Sin nombre"}");
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
            
            if (step.tutorialSo != null)
            {
                step.tutorialSo.ResetTutorial();
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
    
    public TutorialSO CurrentTutorialSO => (_currentStepIndex >= 0 && _currentStepIndex < tutorialSteps.Count) ? tutorialSteps[_currentStepIndex].tutorialSo : null;
}