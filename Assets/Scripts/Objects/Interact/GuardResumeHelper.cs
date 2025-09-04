using UnityEngine;

/// <summary>
/// Helper runtime para re-habilitar colisiones y activar movimiento del guardia tras el vuelo.
/// Separado en archivo propio para evitar componentes anidados corruptos.
/// No modifica el tag del guardia.
/// </summary>
public class GuardResumeHelper : MonoBehaviour
{
    private Collider[] hostCols;
    private Collider[] guardCols;
    private float ignoreSeconds;
    private float airTime;
    private float groundSpeed;
    private Vector3 moveDirX;
    private BridgeConstructionGrid grid;

    public void Setup(Collider[] hostCols, Collider[] guardCols, float ignoreSeconds, float airTime, float groundSpeed, Vector3 moveDirX, BridgeConstructionGrid grid)
    {
        this.hostCols = hostCols;
        this.guardCols = guardCols;
        this.ignoreSeconds = ignoreSeconds;
        this.airTime = airTime;
        this.groundSpeed = groundSpeed;
        this.moveDirX = moveDirX;
        this.grid = grid;
        StartCoroutine(Flow());
    }

    private System.Collections.IEnumerator Flow()
    {
        // Rehabilitar colisiones tras el lapso (si el host sigue existiendo)
        if (ignoreSeconds > 0f && hostCols != null && guardCols != null)
        {
            yield return new WaitForSeconds(ignoreSeconds);
            for (int i = 0; i < hostCols.Length; i++)
            {
                var hc = hostCols[i];
                if (hc == null || !hc.enabled || hc.isTrigger) continue;
                for (int j = 0; j < guardCols.Length; j++)
                {
                    var gc = guardCols[j];
                    if (gc == null || !gc.enabled || gc.isTrigger) continue;
                    Physics.IgnoreCollision(hc, gc, false);
                }
            }
        }

        // Esperar tiempo de vuelo y activar como vehículo simple
        if (airTime > 0f) yield return new WaitForSeconds(airTime);

        var guardGO = this.gameObject;
        if (grid == null) grid = Object.FindFirstObjectByType<BridgeConstructionGrid>();
        if (grid != null) guardGO.SendMessage("SetBridgeGrid", grid, SendMessageOptions.DontRequireReceiver);
        guardGO.SendMessage("Initialize", moveDirX, SendMessageOptions.DontRequireReceiver);
        guardGO.SendMessage("SetSpeed", groundSpeed, SendMessageOptions.DontRequireReceiver);

        Destroy(this);
    }
}
