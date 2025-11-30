using UnityEngine;

public class RotateTowardsPlayer : MonoBehaviour
{
    private Transform player;

    void Start()
    {
        // Find the player by tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Rotate towards player on Y axis
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // Keep rotation only on Y axis

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
    }
}
