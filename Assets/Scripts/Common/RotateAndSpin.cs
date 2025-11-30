using UnityEngine;

public class RotateAndSpin : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float riseHeight = 3f;
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopAnimation = true;
    
    private Vector3 startPosition;
    private Vector3 startRotation;
    private bool isAnimating = false;
    private float totalAnimationTime = 0f; // Track total time for continuous movement
    private float totalDuration;
    
    void Start()
    {
        // Store the initial position and rotation
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        
        // Calculate total duration based on speed
        totalDuration = 4f / animationSpeed; // 4 seconds base duration
        
        if (playOnStart)
        {
            StartAnimation();
        }
    }
    
    void Update()
    {
        if (isAnimating)
        {
            UpdateAnimation();
        }
    }
    
    private void UpdateAnimation()
    {
        totalAnimationTime += Time.deltaTime;
        
        if (!loopAnimation)
        {
            // For non-looping, check if we've completed one cycle
            float progress = totalAnimationTime / totalDuration;
            if (progress >= 1f)
            {
                isAnimating = false;
                // Complete the final position
                transform.position = startPosition;
                transform.rotation = Quaternion.Euler(startRotation + Vector3.up * 360f);
                return;
            }
        }
        
        // Calculate continuous vertical movement using sine wave
        // This creates smooth up-down motion without pauses at the bottom
        float heightProgress = Mathf.Sin(totalAnimationTime * (2f * Mathf.PI) / totalDuration);
        // Transform to positive values: -1 to 1 becomes 0 to 1
        heightProgress = (heightProgress + 1f) * 0.5f;
        Vector3 targetPosition = startPosition + Vector3.up * (heightProgress * riseHeight);
        transform.position = targetPosition;
        
        // Calculate rotation (continuous rotation)
        float currentRotationDegrees = (totalAnimationTime / totalDuration) * 360f;
        Vector3 targetRotation = startRotation + Vector3.up * currentRotationDegrees;
        transform.rotation = Quaternion.Euler(targetRotation);
    }
    
    [ContextMenu("Start Animation")]
    public void StartAnimation()
    {
        totalAnimationTime = 0f;
        isAnimating = true;
        
        // Reset to start position
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(startRotation);
    }
    
    [ContextMenu("Stop Animation")]
    public void StopAnimation()
    {
        isAnimating = false;
        
        // Return to start position and rotation
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(startRotation);
    }
    
    public void SetRiseHeight(float height)
    {
        riseHeight = height;
    }
    
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Max(0.1f, speed);
        totalDuration = 4f / animationSpeed;
    }
    
    public bool IsAnimating()
    {
        return isAnimating;
    }
}
