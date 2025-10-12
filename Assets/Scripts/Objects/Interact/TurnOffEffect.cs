using System.Collections.Generic;
using UnityEngine;

public class TurnOffEffect : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;

    private readonly Collider[] overlap = new Collider[32];
    private readonly HashSet<BridgeQuadrantInstance> affectedQuadrants = new HashSet<BridgeQuadrantInstance>();

    private void Update()
    {
        ApplyEffect();
    }

    public void ApplyEffect()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            overlap
        );

        for (int i = 0; i < count; i++)
        {
            var col = overlap[i];
            if (col == null) continue;

            BatterySystem battery = col.GetComponent<BatterySystem>();
            if (battery != null)
            {
                battery.ForzarDescarga();
            }

            BridgeQuadrantInstance quadrant = col.GetComponent<BridgeQuadrantInstance>();
            if (quadrant != null && quadrant.quadrantSO != null)
            {
                if (quadrant.quadrantSO.era == BridgeQuadrantSO.EraType.Futuristic)
                {
                    if (!affectedQuadrants.Contains(quadrant))
                    {
                        quadrant.quadrantSO.batteryLife /= 2f;
                        affectedQuadrants.Add(quadrant);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
