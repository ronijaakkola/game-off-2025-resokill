using UnityEngine;
using System.Collections;
using deVoid.Utils;

[System.Serializable]
public class ColorTheme
{
    public string themeName = "New Theme";
    public Color gridLineColor = Color.magenta;
    public Color enemyGlowColor = Color.red;
    public Color activeWallColor = Color.red;
}

public class ThemeSwitcher : MonoBehaviour
{
    public static ThemeSwitcher Instance { get; private set; }

    [Header("Theme Settings")]
    [SerializeField] private ColorTheme[] themes = new ColorTheme[]
    {
        new ColorTheme
        {
            themeName = "Default Red/Magenta",
            gridLineColor = new Color(1f, 0f, 0.8009267f, 1f),
            enemyGlowColor = new Color(0.7830189f, 0f, 0.027852468f, 1f),
            activeWallColor = new Color(0.7830189f, 0f, 0.027852468f, 1f),
        }
    };

    [Header("Material References")]
    [SerializeField] private Material synthwaveGridMaterial;
    [SerializeField] private Material hologramMaterial;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Inspector Testing")]
    [SerializeField] private int selectedThemeIndex = 0;

    private int currentThemeIndex = 0;
    private Color currentGridColor;
    private Color currentEnemyColor;
    private Color currentActiveWallColor;
    private Coroutine activeTransition;

    // Shader property IDs (cached for performance)
    private static readonly int LineColorProperty = Shader.PropertyToID("_LineColor");
    private static readonly int GlowColorProperty = Shader.PropertyToID("_GlowColor");

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize current colors from theme 0
        if (themes != null && themes.Length > 0)
        {
            currentGridColor = themes[0].gridLineColor;
            currentEnemyColor = themes[0].enemyGlowColor;
            currentActiveWallColor = themes[0].activeWallColor;
        }
    }

    private void Start()
    {
        // Apply theme 0 at start
        if (themes != null && themes.Length > 0)
        {
            SwitchToTheme(0);
        }
    }

    private void OnValidate()
    {
        // Clamp selected theme index to valid range
        if (themes != null && themes.Length > 0)
        {
            selectedThemeIndex = Mathf.Clamp(selectedThemeIndex, 0, themes.Length - 1);
        }
    }

    [ContextMenu("Apply Selected Theme")]
    public void ApplySelectedTheme()
    {
        SwitchToTheme(selectedThemeIndex);
    }

    public void SwitchToTheme(int themeIndex)
    {
        if (themes == null || themes.Length == 0)
        {
            Debug.LogWarning("ThemeSwitcher: No themes defined!");
            return;
        }

        if (themeIndex < 0 || themeIndex >= themes.Length)
        {
            Debug.LogWarning($"ThemeSwitcher: Theme index {themeIndex} out of range (0-{themes.Length - 1})");
            return;
        }

        currentThemeIndex = themeIndex;

        // Stop any active transition
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
        }

        // Start new transition
        activeTransition = StartCoroutine(TransitionToTheme(themes[themeIndex]));
    }

    private IEnumerator TransitionToTheme(ColorTheme targetTheme)
    {
        Color startGridColor = currentGridColor;
        Color startEnemyColor = currentEnemyColor;
        Color startActiveWallColor = currentActiveWallColor;
        Color targetGridColor = targetTheme.gridLineColor;
        Color targetEnemyColor = targetTheme.enemyGlowColor;
        Color targetActiveWallColor = targetTheme.activeWallColor;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            // Smooth lerp between colors
            currentGridColor = Color.Lerp(startGridColor, targetGridColor, t);
            currentEnemyColor = Color.Lerp(startEnemyColor, targetEnemyColor, t);
            currentActiveWallColor = Color.Lerp(startActiveWallColor, targetActiveWallColor, t);

            // Apply colors to materials
            if (synthwaveGridMaterial != null)
            {
                synthwaveGridMaterial.SetColor(LineColorProperty, currentGridColor);
            }
            if (hologramMaterial != null)
            {
                hologramMaterial.SetColor(GlowColorProperty, currentEnemyColor);
            }

            yield return null;
        }

        // Ensure final colors are exact
        currentGridColor = targetGridColor;
        currentEnemyColor = targetEnemyColor;
        currentActiveWallColor = targetActiveWallColor;

        if (synthwaveGridMaterial != null)
        {
            synthwaveGridMaterial.SetColor(LineColorProperty, currentGridColor);
        }
        if (hologramMaterial != null)
        {
            hologramMaterial.SetColor(GlowColorProperty, currentEnemyColor);
        }

        activeTransition = null;
    }

    public Color GetCurrentEnemyGlowColor()
    {
        return currentEnemyColor;
    }

    public Color GetCurrentActiveWallColor()
    {
        return currentActiveWallColor;
    }

    public string GetCurrentThemeName()
    {
        if (themes != null && currentThemeIndex >= 0 && currentThemeIndex < themes.Length)
        {
            return themes[currentThemeIndex].themeName;
        }
        return "Unknown";
    }
}
