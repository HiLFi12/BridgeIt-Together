using UnityEngine;

/// <summary>
/// Adaptador para conservar la compatibilidad con ProbabilityAction.
/// Reenvía Execute al componente SoldiersCaravanAction del mismo GameObject.
/// </summary>
[DisallowMultipleComponent]
public class SoldiersCaravanProbabilityAction : ProbabilityAction
{
    [SerializeField] private SoldiersCaravanAction target;

    private void Awake()
    {
        if (target == null) target = GetComponent<SoldiersCaravanAction>();
        if (target == null) target = GetComponentInChildren<SoldiersCaravanAction>(true);
    }

    public override void Execute(int quadrantX = -1, int quadrantZ = -1)
    {
        if (target != null)
        {
            target.Execute(quadrantX, quadrantZ);
        }
    }
}
