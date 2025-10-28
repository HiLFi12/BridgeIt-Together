using UnityEngine;

public class Tutorial2 : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private GameObject activateOnZero;

    private float _timer;

    private void Start()
    {
        _timer = Mathf.Max(0f, duration);
    }

    private void Update()
    {
        if (_timer <= 0f) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = 0f;
            if (activateOnZero != null)
                activateOnZero.SetActive(false);
            enabled = false;
        }
    }
}
