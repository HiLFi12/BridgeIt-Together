using System.Collections;
using UnityEngine;

public class DroppedObjectCollisionHandler : MonoBehaviour
{
    private Collider[] objColliders;
    private Collider[] playerColliders;
    private int walkableLayer;
    private bool collisionsRestored;
    private Rigidbody rb;

    private bool freezeAfterDrop;
    private float settleSpeedThreshold;
    private float settleStableTime;
    private float settleMaxTime;

    public void Setup(Collider[] objCols,
                      Collider[] playerCols,
                      int walkableLayerIndex,
                      bool freezeAfter,
                      float speedThreshold,
                      float stableTime,
                      float maxTime)
    {
        objColliders = objCols;
        playerColliders = playerCols;
        walkableLayer = walkableLayerIndex;
        freezeAfterDrop = freezeAfter;
        settleSpeedThreshold = speedThreshold;
        settleStableTime = stableTime;
        settleMaxTime = maxTime;
        rb = GetComponent<Rigidbody>();

        if (walkableLayer < 0)
        {
            // Fallback: si la capa no existe, restaurar tras un frame
            StartCoroutine(RestoreNextFrame());
        }
    }

    private IEnumerator RestoreNextFrame()
    {
        yield return null;
        RestoreCollisionsAndMaybeFreeze();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionsRestored) return;
        if (walkableLayer >= 0 && collision.collider.gameObject.layer == walkableLayer)
        {
            RestoreCollisionsAndMaybeFreeze();
        }
    }

    private void RestoreCollisionsAndMaybeFreeze()
    {
        if (collisionsRestored) return;
        collisionsRestored = true;
        if (objColliders != null && playerColliders != null)
        {
            for (int i = 0; i < objColliders.Length; i++)
            {
                var a = objColliders[i];
                if (a == null) continue;
                for (int j = 0; j < playerColliders.Length; j++)
                {
                    var b = playerColliders[j];
                    if (b == null) continue;
                    Physics.IgnoreCollision(a, b, false);
                }
            }
        }

        if (freezeAfterDrop && rb != null)
        {
            StartCoroutine(SettleThenFreeze());
        }

        // Limpieza opcional del componente luego de un tiempo
        Destroy(this, 5f);
    }

    private IEnumerator SettleThenFreeze()
    {
        float stableFor = 0f;
        float elapsed = 0f;
        float v2 = settleSpeedThreshold * settleSpeedThreshold;
        while (elapsed < settleMaxTime)
        {
            bool lowLin = rb.linearVelocity.sqrMagnitude <= v2;
            bool lowAng = rb.angularVelocity.sqrMagnitude <= (v2 * 10f);
            if (lowLin && lowAng)
            {
                stableFor += Time.deltaTime;
                if (stableFor >= settleStableTime) break;
            }
            else
            {
                stableFor = 0f;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.isKinematic = true;
    rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
