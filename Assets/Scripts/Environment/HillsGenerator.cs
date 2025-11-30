using deVoid.Utils;
using Unity.AI.Navigation;
using UnityEngine;

namespace Game.Environment
{
    //[RequireComponent(typeof(MeshFilter))]
    //[RequireComponent(typeof(MeshCollider))]
    public class HillsGenerator : MonoBehaviour
    {
        [SerializeField] NavMeshSurface surface;

        [Header("Hill Shape")]
        [SerializeField] private float height = 5f;
        [SerializeField] private float noiseScale = 0.1f;
        [SerializeField] private float steepnessPower = 1.3f;

        [Header("Noise Layers (Octaves)")]
        [SerializeField] private int octaves = 4;
        [SerializeField] private float persistence = 0.5f;
        [SerializeField] private float lacunarity = 2f;

        [Header("Randomization")]
        [SerializeField] private bool randomizeOnStart = true;
        [SerializeField] private Vector2 offset;

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private Mesh mesh;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();

            // Clone the mesh so the original asset isn't modified
            mesh = Instantiate(meshFilter.sharedMesh);
            meshFilter.mesh = mesh;
        }

        private void Start()
        {
            if (randomizeOnStart)
            {
                offset = new Vector2(Random.Range(0f, 10000f), Random.Range(0f, 10000f));
            }

            GenerateHills();

            UpdateTerrainAndNavMesh();

            Signals.Get<Environment_Changed>().Dispatch();
        }

        public void UpdateTerrainAndNavMesh()
        {
            GenerateHills();
            mesh.MarkDynamic();

            meshCollider.enabled = false;
            meshCollider.enabled = true;

            // rebuild NavMesh
            surface.BuildNavMesh();
            meshCollider.sharedMesh = mesh;

            var recast = AstarPath.active.data.recastGraph;
            recast.SnapBoundsToScene();
            AstarPath.active.Scan(recast);
        }

        public void GenerateHills()
        {
            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                float amplitude = 1f;
                float frequency = noiseScale;
                float noiseHeight = 0f;

                for (int o = 0; o < octaves; o++)
                {
                    float x = (vertices[i].x + offset.x) * frequency;
                    float z = (vertices[i].z + offset.y) * frequency;
                    float perlinValue = Mathf.PerlinNoise(x, z);

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseHeight = Mathf.Pow(noiseHeight, steepnessPower);
                vertices[i].y = noiseHeight * height;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Update collider to match the new shape
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    public class Environment_Changed : ASignal { }
}
