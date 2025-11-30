using deVoid.Utils;
using Game.CharacterEnemy;
using UnityEngine;
using System.Collections.Generic;

public enum Direction
{
    North = 0,
    East,
    South,
    West
}

public class SpectrumGenerator : MonoBehaviour
{
    [Header("Plane Reference")]
    [SerializeField] private Transform planeTransform;

    [Header("Pillar Settings")]
    [SerializeField] private GameObject spectrumBarPrefab;
    [SerializeField] private float pillarSpacing = 0f;
    [SerializeField] private Transform pillarContainer;

    // Public references to side containers for easy access
    public Transform NorthPillars { get; private set; }
    public Transform SouthPillars { get; private set; }
    public Transform EastPillars { get; private set; }
    public Transform WestPillars { get; private set; }

    // Public references to material instances for each side
    public Material NorthMaterial { get; private set; }
    public Material SouthMaterial { get; private set; }
    public Material EastMaterial { get; private set; }
    public Material WestMaterial { get; private set; }

    // Base material reference for resetting
    private Material baseMaterial;

    // Track which walls are currently glowing
    private HashSet<Direction> activeGlowingWalls = new HashSet<Direction>();

    void Start()
    {
        GeneratePillars();
    }

    void Update()
    {
        // Update all currently glowing walls with the new theme color
        foreach (Direction side in activeGlowingWalls)
        {
            Material material = GetMaterialForSide(side);
            if (material != null)
            {
                Color newActiveWallColor = ThemeSwitcher.Instance != null ?
                    ThemeSwitcher.Instance.GetCurrentActiveWallColor() : Color.red;
                material.SetColor("_GlowColor", newActiveWallColor);
            }
        }
    }

    void GeneratePillars()
    {
        if (planeTransform == null)
        {
            Debug.LogError("SpectrumGenerator: Plane transform is not assigned!");
            return;
        }

        if (spectrumBarPrefab == null)
        {
            Debug.LogError("SpectrumGenerator: SpectrumBar prefab is not assigned!");
            return;
        }

        // Use this GameObject as container if none specified
        Transform container = pillarContainer != null ? pillarContainer : transform;

        // Calculate plane bounds (Unity's default plane is 10x10 units)
        Vector3 planeScale = planeTransform.localScale;
        float halfWidth = planeScale.x * 5f;  // Half of plane width
        float halfDepth = planeScale.z * 5f;  // Half of plane depth
        Vector3 planeCenter = planeTransform.position;

        // Get the prefab's dimensions
        float prefabWidth = GetPrefabWidth(spectrumBarPrefab);
        float prefabDepth = GetPrefabDepth(spectrumBarPrefab);

        // Get the base material from the prefab
        baseMaterial = GetPrefabMaterial(spectrumBarPrefab);

        // Create material instances for each side
        if (baseMaterial != null)
        {
            NorthMaterial = new Material(baseMaterial);
            SouthMaterial = new Material(baseMaterial);
            EastMaterial = new Material(baseMaterial);
            WestMaterial = new Material(baseMaterial);
        }

        // Create parent containers for each side
        NorthPillars = CreateSideContainer(container, "North_Pillars");
        SouthPillars = CreateSideContainer(container, "South_Pillars");
        EastPillars = CreateSideContainer(container, "East_Pillars");
        WestPillars = CreateSideContainer(container, "West_Pillars");

        // Generate pillars on all four sides
        GeneratePillarSide(NorthPillars, planeCenter, halfWidth, halfDepth, prefabWidth, prefabDepth, NorthMaterial, Direction.North);
        GeneratePillarSide(SouthPillars, planeCenter, halfWidth, halfDepth, prefabWidth, prefabDepth, SouthMaterial, Direction.South);
        GeneratePillarSide(EastPillars, planeCenter, halfWidth, halfDepth, prefabWidth, prefabDepth, EastMaterial, Direction.East);
        GeneratePillarSide(WestPillars, planeCenter, halfWidth, halfDepth, prefabWidth, prefabDepth, WestMaterial, Direction.West);

        // Example: Make north wall red with glow using the new method
        Color color = ThemeSwitcher.Instance != null ?
            ThemeSwitcher.Instance.GetCurrentActiveWallColor() : Color.red;

        //SetWallGlow(Direction.North, color, 6f);
    }

    /// <summary>
    /// Sets the glow color and intensity for a specific wall
    /// </summary>
    public void SetWallGlow(Direction side, Color color, float intensity)
    {
        Material material = GetMaterialForSide(side);
        if (material != null)
        {
            material.SetColor("_GlowColor", color);
            material.SetFloat("_GlowIntensity", 4);
            activeGlowingWalls.Add(side);
        }
    }

    /// <summary>
    /// Sets the glow color and intensity for multiple walls
    /// </summary>
    public void SetWallGlow(Direction[] sides, Color color, float intensity)
    {
        foreach (Direction side in sides)
        {
            SetWallGlow(side, color, intensity);
        }
    }

    /// <summary>
    /// Resets a wall's material properties back to the base material
    /// </summary>
    public void ResetWall(Direction side)
    {
        if (baseMaterial == null) return;

        Material material = GetMaterialForSide(side);
        if (material != null)
        {
            material.CopyPropertiesFromMaterial(baseMaterial);
            activeGlowingWalls.Remove(side);
        }
    }

    /// <summary>
    /// Resets multiple walls' material properties back to the base material
    /// </summary>
    public void ResetWall(Direction[] sides)
    {
        foreach (Direction side in sides)
        {
            ResetWall(side);
        }
    }

    /// <summary>
    /// Resets all walls' material properties back to the base material
    /// </summary>
    public void ResetAllWalls()
    {
        ResetWall(Direction.North);
        ResetWall(Direction.South);
        ResetWall(Direction.East);
        ResetWall(Direction.West);
    }

    Material GetMaterialForSide(Direction side)
    {
        switch (side)
        {
            case Direction.North:
                return NorthMaterial;
            case Direction.South:
                return SouthMaterial;
            case Direction.East:
                return EastMaterial;
            case Direction.West:
                return WestMaterial;
            default:
                return null;
        }
    }

    Transform CreateSideContainer(Transform parent, string name)
    {
        GameObject sideContainer = new GameObject(name);
        sideContainer.transform.SetParent(parent);
        sideContainer.transform.localPosition = Vector3.zero;
        sideContainer.transform.localRotation = Quaternion.identity;
        return sideContainer.transform;
    }

    void GeneratePillarSide(Transform container, Vector3 center, float halfWidth, float halfDepth, float prefabWidth, float prefabDepth, Material sideMaterial, Direction side)
    {
        Vector3 startPos, direction;
        Quaternion rotation;
        float sideLength;
        float inwardOffset = prefabDepth * 0.5f; // Offset by half the depth to align outer edge with plane edge

        switch (side)
        {
            case Direction.North:
                // Top edge (positive Z) - offset inward (negative Z)
                startPos = center + new Vector3(-halfWidth, 0, halfDepth - inwardOffset);
                direction = Vector3.right;
                rotation = Quaternion.Euler(0, 180, 0); // Face inward (south)
                sideLength = halfWidth * 2;
                break;

            case Direction.South:
                // Bottom edge (negative Z) - offset inward (positive Z)
                startPos = center + new Vector3(halfWidth, 0, -halfDepth + inwardOffset);
                direction = Vector3.left;
                rotation = Quaternion.Euler(0, 0, 0); // Face inward (north)
                sideLength = halfWidth * 2;
                break;

            case Direction.East:
                // Right edge (positive X) - offset inward (negative X)
                startPos = center + new Vector3(halfWidth - inwardOffset, 0, halfDepth);
                direction = Vector3.back;
                rotation = Quaternion.Euler(0, -90, 0); // Face inward (west)
                sideLength = halfDepth * 2;
                break;

            case Direction.West:
                // Left edge (negative X) - offset inward (positive X)
                startPos = center + new Vector3(-halfWidth + inwardOffset, 0, -halfDepth);
                direction = Vector3.forward;
                rotation = Quaternion.Euler(0, 90, 0); // Face inward (east)
                sideLength = halfDepth * 2;
                break;

            default:
                return;
        }

        // Calculate number of pillars for this side
        float spacing = pillarSpacing > 0 ? pillarSpacing : prefabWidth;
        int pillarCount = Mathf.Max(1, Mathf.FloorToInt(sideLength / spacing));

        // Generate pillars along the side
        for (int i = 0; i < pillarCount; i++)
        {
            Vector3 position = startPos + direction * (i * spacing);
            GameObject pillar = Instantiate(spectrumBarPrefab, position, rotation, container);
            pillar.name = $"SpectrumBar_{side}_{i}";

            // Apply the shared material instance for this side
            if (sideMaterial != null)
            {
                Renderer renderer = pillar.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material = sideMaterial;
                }
            }
        }
    }

    float GetPrefabWidth(GameObject prefab)
    {
        // Try to get the renderer bounds from the prefab
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.x;
        }

        // Default fallback if no renderer found
        return 1f;
    }

    float GetPrefabDepth(GameObject prefab)
    {
        // Try to get the renderer bounds from the prefab
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.z;
        }

        // Default fallback if no renderer found
        return 1f;
    }

    Material GetPrefabMaterial(GameObject prefab)
    {
        // Try to get the material from the prefab
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            return renderer.sharedMaterial;
        }

        Debug.LogWarning("SpectrumGenerator: Could not find material on prefab.");
        return null;
    }

    void ReactToEnemySpawnSidesChange(List<Direction> sides)
    {
        for (Direction i = Direction.North; i <= Direction.West; ++i)
        {
            if (sides.Contains(i))
            {
                Color color = ThemeSwitcher.Instance != null ?
                    ThemeSwitcher.Instance.GetCurrentActiveWallColor() : Color.red;
                SetWallGlow(i, color, 6f);
            }
            else
            {
                ResetWall(i);
            }
        }
    }

    void OnEnable()
    {
        Signals.Get<Enemy_ChangeSpawnSide>().AddListener(ReactToEnemySpawnSidesChange);
    }

    void OnDisable()
    {
        Signals.Get<Enemy_ChangeSpawnSide>().RemoveListener(ReactToEnemySpawnSidesChange);
    }
}
