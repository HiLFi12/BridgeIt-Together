using UnityEngine;

public class MovementCenter : MonoBehaviour
{
    [SerializeField] private Transform centerPosition;
    [SerializeField] private float speed = 0.5f;
    void Start() { }
    
    void Update()
    {
        centerPosition.position += speed * Vector3.right * Time.deltaTime;
    }
}
