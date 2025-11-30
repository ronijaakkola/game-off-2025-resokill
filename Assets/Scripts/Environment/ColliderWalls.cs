using UnityEngine;

namespace Game.Environment
{
    public class ColliderWalls : MonoBehaviour
    {
        public int segments = 12;          // Number of walls
        public float radius = 5f;          // Radius of the tube
        public float height = 3f;          // Height of the tube
        public float wallThickness = 0.2f; // Thickness of each wall segment
        public float segmentWidth = 1f;    // Width along circumference

        void Start()
        {
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 360f / segments;
                float rad = angle * Mathf.Deg2Rad;

                GameObject wall = new GameObject("Wall_" + i);
                wall.transform.parent = transform;

                // Position around circle
                Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, height / 2f, Mathf.Sin(rad) * radius);
                wall.transform.localPosition = pos;

                // Rotate to face center
                wall.transform.LookAt(new Vector3(0, height / 2f, 0));

                // Add BoxCollider
                BoxCollider col = wall.AddComponent<BoxCollider>();
                col.size = new Vector3(segmentWidth, height, wallThickness);
            }
        }
    }
}
