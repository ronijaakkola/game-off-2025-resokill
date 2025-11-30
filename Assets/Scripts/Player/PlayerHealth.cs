using deVoid.UIFramework.Examples;
using deVoid.Utils;
using Game.Audio;
using Game.GameScreen;
using Game.Player;
using MoreMountains.Feedbacks;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private MMFeedbacks healingFeedback;
    [SerializeField] private MMFeedbacks damageFeedback;
    [SerializeField] private float invulnerabilityDuration = 2f;
    [Tooltip("Disable Game Over")]
    [SerializeField] private bool disableGameOver = false;
    [SerializeField] private bool disableDamage = false;

    private const int MAX_HEALTH = 3;
    private int currentHealth;
    private float invulnerabilityTimer = 0f;

    void Start()
    {
        currentHealth = MAX_HEALTH;
        FireHealthChangedEvent();
    }

    void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    public void AddHealth(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, MAX_HEALTH);
        healingFeedback?.PlayFeedbacks();
        FireHealthChangedEvent();
    }

    public void RemoveHealth(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        if (invulnerabilityTimer > 0f || disableDamage)
            return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        invulnerabilityTimer = invulnerabilityDuration;
        damageFeedback?.PlayFeedbacks();
        FireHealthChangedEvent();

        AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.PlayerDamaged, transform.position);

        if (currentHealth <= 0 && !disableGameOver)
        {
            Signals.Get<PlayerDiedEvent>().Dispatch();
            Signals.Get<Screen_OpenRequest>().Dispatch(ScreenIds.DeathScreen);
        }
    }

    public void ResetHealth()
    {
        currentHealth = MAX_HEALTH;
        invulnerabilityTimer = 0f;
        FireHealthChangedEvent();
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return MAX_HEALTH;
    }

    public bool IsInvulnerable()
    {
        return invulnerabilityTimer > 0f;
    }

    public bool IsFullHealth()
    {
        return currentHealth >= MAX_HEALTH;
    }

    [ContextMenu("Add 1 HP")]
    private void TestAddHealth()
    {
        AddHealth(1);
    }

    [ContextMenu("Remove 1 HP")]
    private void TestRemoveHealth()
    {
        RemoveHealth(1);
    }

    private void FireHealthChangedEvent()
    {
        PlayerHealthData data = new PlayerHealthData
        {
            CurrentHealth = currentHealth,
            MaxHealth = MAX_HEALTH
        };

        Signals.Get<PlayerHealthChangedEvent>().Dispatch(data);
    }
}
