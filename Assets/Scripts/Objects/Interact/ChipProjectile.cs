using UnityEngine;

public class ChipProjectile : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 1.5f;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 2f;
    [SerializeField] private int maxAttacks = 3;
    [SerializeField] private float damageAmount = 1f;

    [Header("Visual")]
    [SerializeField] private GameObject destroyEffectPrefab;

    private readonly Collider[] overlap = new Collider[32];
    private int attackCount;
    private float cooldownTimer;
    private bool canAttack = true;

    private void FixedUpdate()
    {
        if (canAttack)
        {
            int quadrantsDamaged = TryAttack();
            
            if (quadrantsDamaged > 0)
            {
                attackCount++;

                if (attackCount >= maxAttacks)
                {
                    DestroyProjectile();
                    return;
                }
                
                canAttack = false;
                cooldownTimer = damageCooldown;
            }
        }
        else
        {
            cooldownTimer -= Time.fixedDeltaTime;
            if (cooldownTimer <= 0f)
            {
                canAttack = true;
               
            }
        }
    }

    private int TryAttack()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            overlap
        );
        

        int quadrantsDamaged = 0;

        for (int i = 0; i < count; i++)
        {
            var col = overlap[i];
            if (col == null) continue;

            BridgeQuadrantInstance quadrant = col.GetComponent<BridgeQuadrantInstance>();
            if (quadrant != null && quadrant.quadrantSO != null)
            {
                if (quadrant.quadrantSO.era == BridgeQuadrantSO.EraType.Futuristic)
                {
                    quadrant.quadrantSO.batteryLife -= damageAmount;
                    quadrantsDamaged++;
                }
            }
        }

        return quadrantsDamaged;
    }

    private void DestroyProjectile()
    {
        if (destroyEffectPrefab != null)
            Instantiate(destroyEffectPrefab, transform.position, transform.rotation);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}