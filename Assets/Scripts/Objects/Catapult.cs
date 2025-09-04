using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BridgeItTogether.Gameplay.Abstractions;

public class Catapult : MonoBehaviour, IInteractable
{
    [Header("Launch Settings")]
    [SerializeField] private Transform launchPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField, Range(10f, 80f)] private float launchAngleDeg = 45f;
    [SerializeField, Min(0.1f)] private float launchSpeed = 12f;
    [SerializeField, Min(0.1f)] private float launchGravity = 9.81f;
    [SerializeField] private bool kinematicDuringLaunch = true;
    [SerializeField] private float minLaunchDistance = 0.05f;

    [Header("Catapult Rotation")]
    [SerializeField] private Transform rotatingArm;
    [SerializeField] private Transform launchSocket;
    [SerializeField] private float lowAngle = -90f;
    [SerializeField] private float highAngle = 180f;
    [SerializeField] private float launchRotationSpeed = 90f;
    [SerializeField] private float reloadRotationSpeed = 45f;

    [Header("Interaction")]
    [SerializeField] private InteractPriority interactPriority = InteractPriority.Medium;

    private readonly Dictionary<Transform, Coroutine> activeLaunches = new();

    [SerializeField] private bool isReady = true;
    private GameObject loadedObject;
    private Coroutine currentOperation;

    public InteractPriority InteractPriority => interactPriority;

    public void Interact(GameObject interactor)
    {
        if (!isReady || !targetPoint) return;

        var playerHolder = interactor.GetComponent<PlayerObjectHolder>();
        if (!playerHolder || !playerHolder.HasObjectInHand()) return;

        GameObject objectInHand = playerHolder.GetHeldObject();
        if (!IsValidHandObject(objectInHand)) return;

        loadedObject = objectInHand;
        var objTransform = objectInHand.transform;

        ClearPlayerHolder(playerHolder);

        Transform parentSocket = launchSocket ? launchSocket : rotatingArm;
        objTransform.SetParent(parentSocket, true);
        objTransform.localPosition = Vector3.zero;
        objTransform.localRotation = Quaternion.identity;

        isReady = false;
        if (currentOperation != null)
            StopCoroutine(currentOperation);
        currentOperation = StartCoroutine(LaunchSequence());
    }

    private IEnumerator LaunchSequence()
    {
        yield return StartCoroutine(RotateToHighAngle());

        if (loadedObject && targetPoint)
        {
            var hitable = loadedObject.GetComponent<IHitable>();
            var targetT = loadedObject.transform;
            Vector3 destino = targetPoint.position;

            targetT.SetParent(null, true);
            hitable?.OnLaunched(destino);

            var routine = StartCoroutine(LaunchRoutine(targetT, destino));
            activeLaunches[targetT] = routine;

            loadedObject = null;
        }

        yield return StartCoroutine(RotateToLowAngle());

        isReady = true;
        currentOperation = null;
    }

    private void ClearPlayerHolder(PlayerObjectHolder holder)
    {
        var heldObjectField = typeof(PlayerObjectHolder).GetField("heldObject",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (heldObjectField != null)
            heldObjectField.SetValue(holder, null);

        var disabledCollidersField = typeof(PlayerObjectHolder).GetField("heldDisabledColliders",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (disabledCollidersField != null)
        {
            var list = disabledCollidersField.GetValue(holder) as System.Collections.IList;
            list?.Clear();
        }

        var rigidbodyField = typeof(PlayerObjectHolder).GetField("heldRigidbody",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (rigidbodyField != null)
            rigidbodyField.SetValue(holder, null);
    }

    private bool IsValidHandObject(GameObject obj)
    {
        if (!obj) return false;

        if (obj.GetComponent<IHitable>() != null) return true;
        if (obj.GetComponent<PaloIgnifugo>() != null) return true;
        if (obj.GetComponent<MaterialTipo1>() != null) return true;
        //if (obj.CompareTag("Item") || obj.CompareTag("Projectile")) return true;

        return false;
    }

    private void Awake()
    {
        if (!launchPoint) launchPoint = transform;
        if (!rotatingArm) rotatingArm = transform;
        if (!launchSocket) launchSocket = rotatingArm;
    }

    private void OnValidate()
    {
        if (!launchPoint) launchPoint = transform;
        if (!rotatingArm) rotatingArm = transform;
        if (!launchSocket) launchSocket = rotatingArm;
    }

    private void Start()
    {
        if (rotatingArm)
        {
            rotatingArm.localRotation = Quaternion.Euler(0f, 0f, lowAngle);
        }
        isReady = true;
    }

    public void SetTargetPoint(Transform target)
    {
        targetPoint = target;
    }

    private IEnumerator RotateToHighAngle()
    {
        if (!rotatingArm) yield break;

        float startAngle = rotatingArm.localEulerAngles.z;
        float targetAngle = highAngle;

        // Normalizar ángulos al rango -180 a 180
        if (startAngle > 180f) startAngle -= 360f;
        if (targetAngle > 180f) targetAngle -= 360f;

        // Calcular la diferencia y elegir el camino más corto
        float diff = targetAngle - startAngle;
        if (diff > 180f) diff -= 360f;
        if (diff < -180f) diff += 360f;

        float actualTarget = startAngle + diff;

        float duration = Mathf.Abs(diff) / launchRotationSpeed;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            float currentAngle = Mathf.Lerp(startAngle, actualTarget, progress);
            rotatingArm.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }

        rotatingArm.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    private IEnumerator RotateToLowAngle()
    {
        if (!rotatingArm) yield break;

        float startAngle = rotatingArm.localEulerAngles.z;
        float targetAngle = lowAngle;

        // Normalizar ángulos al rango -180 a 180
        if (startAngle > 180f) startAngle -= 360f;
        if (targetAngle > 180f) targetAngle -= 360f;

        // Calcular la diferencia y elegir el camino más corto
        float diff = targetAngle - startAngle;
        if (diff > 180f) diff -= 360f;
        if (diff < -180f) diff += 360f;

        float actualTarget = startAngle + diff;

        float duration = Mathf.Abs(diff) / reloadRotationSpeed;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            float currentAngle = Mathf.Lerp(startAngle, actualTarget, progress);
            rotatingArm.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }

        rotatingArm.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    private IEnumerator LaunchRoutine(Transform target, Vector3 destino)
    {
        if (!target) yield break;

        Vector3 start = launchPoint ? launchPoint.position : target.position;
        Vector3 startXZ = new Vector3(start.x, 0f, start.z);
        Vector3 endXZ = new Vector3(destino.x, 0f, destino.z);
        Vector3 flat = endXZ - startXZ;
        float distance = flat.magnitude;

        if (distance < minLaunchDistance)
        {
            target.position = destino;
            activeLaunches.Remove(target);
            yield break;
        }

        Vector3 dir = flat / Mathf.Max(distance, 0.0001f);
        float angleRad = launchAngleDeg * Mathf.Deg2Rad;
        float v = Mathf.Max(0.01f, launchSpeed);
        float g = Mathf.Max(0.01f, launchGravity);
        float sinAngle = Mathf.Sin(angleRad);
        float cosAngle = Mathf.Cos(angleRad);

        float a = -0.5f * g;
        float b = v * sinAngle;
        float c = start.y - destino.y;

        float discriminant = b * b - 4f * a * c;
        float totalTime;

        if (discriminant >= 0f)
        {
            float t1 = (-b + Mathf.Sqrt(discriminant)) / (2f * a);
            float t2 = (-b - Mathf.Sqrt(discriminant)) / (2f * a);
            totalTime = Mathf.Max(t1, t2);
        }
        else
        {
            totalTime = distance / (v * cosAngle);
        }

        totalTime = Mathf.Max(0.5f, totalTime);

        Rigidbody trb = target.GetComponent<Rigidbody>();
        bool hadRB = trb != null;
        bool prevKinematic = false;
        if (hadRB && kinematicDuringLaunch)
        {
            prevKinematic = trb.isKinematic;
            trb.isKinematic = true;
        }

        float t = 0f;
        while (t < totalTime && target != null)
        {
            t += Time.deltaTime;
            float progress = Mathf.Min(t / totalTime, 1f);

            Vector3 horizontalPos = Vector3.Lerp(start, destino, progress);
            float currentHeight = start.y + (v * sinAngle * t) - (0.5f * g * t * t);
            Vector3 currentPos = new Vector3(horizontalPos.x, currentHeight, horizontalPos.z);
            target.position = currentPos;

            yield return null;
        }

        if (target != null)
            target.position = destino;

        if (hadRB && kinematicDuringLaunch && trb != null)
            trb.isKinematic = prevKinematic;

        if (target != null)
            activeLaunches.Remove(target);
    }

    public bool IsReady() => isReady;
    public bool HasLoadedObject() => loadedObject != null;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (launchPoint && targetPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(launchPoint.position, targetPoint.position);
            Gizmos.DrawWireSphere(targetPoint.position, 0.5f);
        }

        if (rotatingArm)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rotatingArm.position, 0.2f);

            Vector3 lowDir = Quaternion.Euler(0f, 0f, lowAngle) * Vector3.right;
            Gizmos.DrawLine(rotatingArm.position, rotatingArm.position + lowDir * 1f);

            Gizmos.color = Color.cyan;
            Vector3 highDir = Quaternion.Euler(0f, 0f, highAngle) * Vector3.right;
            Gizmos.DrawLine(rotatingArm.position, rotatingArm.position + highDir * 1f);
        }

        if (launchSocket)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(launchSocket.position, Vector3.one * 0.2f);
        }
    }
#endif
}