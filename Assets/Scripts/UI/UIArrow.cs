using UnityEngine;
using UnityEngine.UI;

public class UIArrow : MonoBehaviour
{
    [SerializeField] private float amplitude = 10f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float smoothness = 8f;

    private RectTransform _rectTransform;
    private Vector2 _initialAnchoredPos;
    private float _elapsed;
    private float _currentOffset;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _initialAnchoredPos = _rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        _elapsed = 0f;
        _currentOffset = 0f;
    }

    private void Update()
    {
        // Use unscaled time so the UI animation still plays when the game is paused
        _elapsed += Time.unscaledDeltaTime;

        // Target offset follows a sine wave
        float targetOffset = Mathf.Sin(_elapsed * speed) * amplitude;

        // Smoothly approach the target offset
        float t = 1f - Mathf.Exp(-smoothness * Time.unscaledDeltaTime);
        _currentOffset = Mathf.Lerp(_currentOffset, targetOffset, t);

        // Apply vertical offset to the rectTransform's anchoredPosition
        _rectTransform.anchoredPosition = _initialAnchoredPos + new Vector2(0f, _currentOffset);
    }

    // Public setters if needed
    public void SetAmplitude(float a) => amplitude = a;
    public void SetSpeed(float s) => speed = s;
    public void SetSmoothness(float s) => smoothness = s;
}
