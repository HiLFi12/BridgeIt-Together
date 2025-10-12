using System.Collections.Generic;
using UnityEngine;

public class TurnOffEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float startRadius = 5f;
    [SerializeField] private float maxRadius = 10f;
    [SerializeField] private float expansionSpeed = 5f;
    
    [Header("Visual")]
    [SerializeField] private GameObject visualObject;

    private readonly Collider[] overlap = new Collider[32];
    private readonly HashSet<BridgeQuadrantInstance> affectedQuadrants = new HashSet<BridgeQuadrantInstance>();
    
    private float currentRadius;
    private bool isExpanding = true;

    private void Start()
    {
        currentRadius = startRadius;
        if (visualObject != null)
        {
            visualObject.transform.localScale = Vector3.one * startRadius;
        }
    }

    private void Update()
    {
        UpdateVisual();
        ApplyEffect();
    }

    private void UpdateVisual()
    {
        if (isExpanding)
        {
            currentRadius += expansionSpeed * Time.deltaTime;
            if (currentRadius >= maxRadius)
            {
                currentRadius = maxRadius;
                isExpanding = false;
            }
        }
        else
        {
            currentRadius -= expansionSpeed * Time.deltaTime;
            if (currentRadius <= 0f)
            {
                currentRadius = 0f;
                Destroy(gameObject);
            }
        }

        if (visualObject != null)
        {
            visualObject.transform.localScale = Vector3.one * currentRadius;
        }
    }

    public void ApplyEffect()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            currentRadius,
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
        float radiusToShow = Application.isPlaying ? currentRadius : startRadius;
        Gizmos.DrawWireSphere(transform.position, radiusToShow);
    }
}
