using UnityEngine;

/// <summary>
/// Adaptador para conservar la compatibilidad con ProbabilityAction.
/// Reenvía Execute al componente GoodsCartChestAction del mismo GameObject.
/// </summary>
[DisallowMultipleComponent]
public class GoodsCartChestProbabilityAction : ProbabilityAction
{
    [SerializeField] private GoodsCartChestAction target;

    private void Awake()
    {
        if (target == null) target = GetComponent<GoodsCartChestAction>();
        if (target == null) target = GetComponentInChildren<GoodsCartChestAction>(true);
    }

    public override void Execute(int quadrantX = -1, int quadrantZ = -1)
    {
        if (target != null)
        {
            target.Execute(quadrantX, quadrantZ);
        }
    }
}
