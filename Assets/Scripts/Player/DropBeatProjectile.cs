using Game.Audio;
using Game.CharacterEnemy;
using Game.Common;
using UnityEngine;

public class DropBeatProjectile : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 40f;
    [SerializeField] private float lifetime = 2f;

    [Header("Hit Effect Prefabs")]
    [SerializeField] private GameObject perfectHitEffect;
    [SerializeField] private GameObject goodHitEffect;
    [SerializeField] private GameObject missedHitEffect;

    private Vector3 size = Vector3.one;
    private int damage = 10;
    private ShotQuality shotQuality;

    private Renderer projectileRenderer;

    private TimeLeftClock liveTimer;

    // Initialize the projectile with custom settings
    public void Initialize(Color color, int damage, ShotQuality quality)
    {
        this.damage = damage;
        this.shotQuality = quality;

        if (projectileRenderer == null)
        {
            projectileRenderer = GetComponent<Renderer>();
        }

        if (projectileRenderer != null)
        {
            // Create a new material instance to avoid modifying the shared material
            projectileRenderer.material = new Material(projectileRenderer.material);
            if (quality == ShotQuality.Perfect)
            {
                projectileRenderer.material.color = color;
                projectileRenderer.material.SetColor("_GlowColor", color);
            }
        }

        if (liveTimer == null)
        {
            liveTimer = new TimeLeftClock(lifetime);
        }
        else
        {
            liveTimer.ResetTimer();
        }
    }

    void Start()
    {
        projectileRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        if (liveTimer.IsTimeOver())
        {
            ObjectPooler.Instance.ReturnToPool(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Spawn hit particle at collision point based on shot quality
                GameObject hitEffectPrefab = shotQuality switch
                {
                    ShotQuality.Perfect => perfectHitEffect,
                    ShotQuality.Good => goodHitEffect,
                    ShotQuality.Missed => missedHitEffect,
                    _ => goodHitEffect
                };

                if (hitEffectPrefab != null)
                {
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
                }

                enemy.TakeDamage(damage, shotQuality, forceCritical: true);
            }
        }
    }
}
