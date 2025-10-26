using UnityEngine;

[CreateAssetMenu(fileName = "TutorialSO", menuName = "Tutorial/TutorialSO", order = 1)]
public class TutorialSO : ScriptableObject
{
    [Header("Tutorial Settings")]
    [SerializeField] private string tutorialName;
    [SerializeField] private string description;
    
    [Header("Completion")]
    private bool tutorialFinished = false;
    
    public Player player;
    
    public delegate void TutorialCompletedHandler(TutorialSO completedTutorial);
    public event TutorialCompletedHandler OnTutorialCompleted;
    
    public bool TutorialFinished => tutorialFinished;
    
    public string TutorialName => tutorialName;
    public string Description => description;
    
    // Método para marcar como completado y notificar
    public virtual void CompleteTutorial()
    {
        if (!tutorialFinished)
        {
            tutorialFinished = true;
            Debug.Log($"Tutorial '{tutorialName}' completed.");
            OnTutorialCompleted?.Invoke(this);
        }
    }
    
    // Método para resetear (útil para debug o reinicio)
    public virtual void ResetTutorial()
    {
        tutorialFinished = false;
        Debug.Log($"Tutorial '{tutorialName}' reset.");
    }
    
    // Método virtual para lógica específica de inicialización o checks
    public virtual void Initialize()
    {
        // Override in subclasses for specific logic
    }
    
    // Método virtual para checks continuos (llamado desde Update si es necesario)
    public virtual void UpdateTutorial()
    {
        // Override in subclasses for ongoing checks
    }
}
