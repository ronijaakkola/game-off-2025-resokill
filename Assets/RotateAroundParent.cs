using UnityEngine;

public class RotateAroundParent : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f; // Degrees per second

    void Update()
    {
        if (transform.parent == null)
        {
            Debug.LogWarning("RotateAroundObject: Camera has no parent object to rotate around!");
            return;
        }

        transform.RotateAround(transform.parent.position, Vector3.up, rotationSpeed * Time.deltaTime);

        transform.LookAt(transform.parent);
    }
}
