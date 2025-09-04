using UnityEngine;

/// <summary>
/// Adaptador ProbabilityAction para PropelledCarriageAction.
/// Permite mantener el flujo de probabilidad llamando al AutoController.
/// </summary>
[DisallowMultipleComponent]
public class PropelledCarriageProbabilityAction : ProbabilityAction
{
    [SerializeField] private PropelledCarriageAction target;

    private void Awake()
    {
        if (target == null) target = GetComponent<PropelledCarriageAction>();
        if (target == null) target = GetComponentInChildren<PropelledCarriageAction>(true);
    }

    public override void Execute(int quadrantX = -1, int quadrantZ = -1)
    {
        if (target != null)
        {
            target.Execute(quadrantX, quadrantZ);
        }
    }
}
