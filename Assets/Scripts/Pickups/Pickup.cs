using UnityEngine;

[RequireComponent(typeof(RotateAndSpin))]
public abstract class Pickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] protected bool destroyOnPickup = true;

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Check if the player picked up the item
        if (other.CompareTag("Player"))
        {
            OnPickup(other.gameObject);

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }

    // Abstract method that derived classes must implement
    protected abstract void OnPickup(GameObject player);
}
