using UnityEngine;
using Unity.Cinemachine;
using deVoid.Utils;
using Game.Player;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Camera Settings")]
    [SerializeField] private string playerCameraName = "VirtualCamera";
    [SerializeField] private string deathCameraName = "DeathCamera";

    [Header("Camera Priorities")]
    [SerializeField] private int activeCameraPriority = 10;
    [SerializeField] private int inactiveCameraPriority = 0;

    private CinemachineCamera playerCamera;
    private CinemachineCamera deathCamera;
    private CinemachineBrain cinemachineBrain;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Subscribe to scene loaded event to re-find cameras when scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;

        InitializeCameras();
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Clear singleton instance if this is the current instance
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-initialize cameras when a new scene loads
        Debug.Log($"CameraController: Scene loaded: {scene.name}, re-initializing cameras");
        InitializeCameras();
    }

    private void InitializeCameras()
    {
        FindCameras();

        // Change camera priorities to ensure player camera is active
        if (playerCamera != null)
            playerCamera.Priority = activeCameraPriority;

        if (deathCamera != null)
            deathCamera.Priority = inactiveCameraPriority;

        Debug.Log($"CameraController: Cameras initialized. Player: {playerCamera != null}, Death: {deathCamera != null}");
    }

    private void FindCameras()
    {
        // Find the CinemachineBrain on the main camera
        cinemachineBrain = FindAnyObjectByType<CinemachineBrain>();
        if (cinemachineBrain == null)
        {
            Debug.LogWarning("CameraController: Could not find CinemachineBrain!");
        }

        // Find all Cinemachine cameras in the scene
        CinemachineCamera[] allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in allCameras)
        {
            if (cam.gameObject.name == playerCameraName)
            {
                playerCamera = cam;
            }
            else if (cam.gameObject.name == deathCameraName)
            {
                deathCamera = cam;
            }
        }

        if (playerCamera == null)
        {
            Debug.LogWarning($"CameraController: Could not find player camera with name '{playerCameraName}'");
        }

        if (deathCamera == null)
        {
            Debug.LogWarning($"CameraController: Could not find death camera with name '{deathCameraName}'");
        }
    }

    private void OnEnable()
    {
        Signals.Get<PlayerDiedEvent>().AddListener(OnPlayerDied);
    }

    private void OnDisable()
    {
        Signals.Get<PlayerDiedEvent>().RemoveListener(OnPlayerDied);
    }

    private void Start()
    {
        // Ensure player camera is active at start
        ResetToPlayerCamera();
    }

    private void OnPlayerDied()
    {
        SwitchToDeathCamera();
    }

    private void SwitchToDeathCamera()
    {
        if (playerCamera != null)
            playerCamera.Priority = inactiveCameraPriority;

        if (deathCamera != null)
            deathCamera.Priority = activeCameraPriority;
    }

    public void ResetToPlayerCamera()
    {
        // Store the original blend and set to instant cut
        CinemachineBlendDefinition originalBlend = default;
        if (cinemachineBrain != null)
        {
            originalBlend = cinemachineBrain.DefaultBlend;
            cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
        }

        // Change camera priorities
        if (playerCamera != null)
            playerCamera.Priority = activeCameraPriority;

        if (deathCamera != null)
            deathCamera.Priority = inactiveCameraPriority;

        // Restore original blend after a short delay
        if (cinemachineBrain != null)
        {
            StartCoroutine(RestoreBlendAfterFrame(originalBlend));
        }
    }

    private System.Collections.IEnumerator RestoreBlendAfterFrame(CinemachineBlendDefinition originalBlend)
    {
        yield return null; // Wait one frame
        if (cinemachineBrain != null)
        {
            cinemachineBrain.DefaultBlend = originalBlend;
        }
    }
}
