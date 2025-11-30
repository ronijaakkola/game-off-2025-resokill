using UnityEngine;

public class MenuSphereRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 2f;

    void Update()
    {
        transform.Rotate(-rotationSpeed * Time.deltaTime, 0f, 0f);
    }
}
