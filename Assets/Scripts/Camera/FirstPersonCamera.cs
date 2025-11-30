using UnityEngine;

public class FirstPersonCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Usually the player
    public Vector3 offset = new Vector3(0f, 1.7f, 0f); // Camera height from player origin
    public float followSpeed = 10f;

    private void LateUpdate()
    {
        if (!target)
            return;

        // Smoothly follow target position
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Match target rotation exactly
        transform.rotation = target.rotation;
    }
}
