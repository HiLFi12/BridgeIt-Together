using UnityEngine;

public class CameraCinematicMove : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float speed = 2f;

    private enum MoveDirection { Left, Right }
    [SerializeField] private MoveDirection moveDirection = MoveDirection.Left;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (targetCamera == null) return;

        Vector3 dir = moveDirection == MoveDirection.Left ? Vector3.left : Vector3.right;
        targetCamera.transform.position += dir * speed * Time.deltaTime;
    }
}
