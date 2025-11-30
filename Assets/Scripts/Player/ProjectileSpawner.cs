using Game.Audio;
using Game.Common;
using UnityEngine;

[System.Serializable]
public class ProjectileStyleSettings
{
    [Tooltip("Size multiplier for the projectile")]
    public float size = 0.4f;

    [Tooltip("Color of the projectile")]
    public Color color = Color.gray;

    [Tooltip("Damage dealt by the projectile")]
    public int damage = 10;
}

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject waveProjectile;
    [SerializeField] private GameObject dropBeatProjectile;

    [Header("Projectile Styles")]
    [SerializeField] private ProjectileStyleSettings perfectStyle = new ProjectileStyleSettings
    {
        size = 0.6f,
        color = Color.yellow,
        damage = 15
    };

    [SerializeField] private ProjectileStyleSettings goodStyle = new ProjectileStyleSettings
    {
        size = 0.4f,
        color = Color.white,
        damage = 10
    };

    [SerializeField] private ProjectileStyleSettings missedStyle = new ProjectileStyleSettings
    {
        size = 0.2f,
        color = Color.gray,
        damage = 5
    };

    public GameObject Spawn(ShotQuality quality)
    {
        if (waveProjectile == null)
        {
            Debug.LogWarning("No prefab assigned to spawn!");
            return null;
        }

        ProjectileStyleSettings style = quality switch
        {
            ShotQuality.Perfect => perfectStyle,
            ShotQuality.Good => goodStyle,
            ShotQuality.Missed => missedStyle,
            _ => goodStyle
        };


        GameObject obj = ObjectPooler.Instance.GetPooledObject(waveProjectile);
        if (obj != null)
        {
            obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
            obj.gameObject.SetActive(true);

            SoundWaveProjectile projectile = obj.GetComponent<SoundWaveProjectile>();
            if (projectile != null)
            {
                // Add combo bonus damage
                int comboBonus = BeatDetector.Instance?.CurrentCombo ?? 0;
                int totalDamage = style.damage + comboBonus;

                // Calculate combo-scaled size (0-10% boost based on combo)
                int maxCombo = BeatDetector.Instance?.MaxCombo ?? 20;
                float comboScaleMultiplier = 1.0f + (comboBonus / (float)maxCombo) * 0.1f;
                float finalSize = style.size * comboScaleMultiplier;

                projectile.Initialize(finalSize, style.color, totalDamage, quality);
            }
        }

        return obj;
    }

    public GameObject SpawnAlternate(ShotQuality quality)
    {
        if (dropBeatProjectile == null)
        {
            Debug.LogWarning("No dropBeatProjectile prefab assigned to spawn!");
            return null;
        }

        ProjectileStyleSettings style = quality switch
        {
            ShotQuality.Perfect => perfectStyle,
            ShotQuality.Good => goodStyle,
            ShotQuality.Missed => missedStyle,
            _ => goodStyle
        };


        GameObject obj = ObjectPooler.Instance.GetPooledObject(dropBeatProjectile);
        if (obj != null)
        {
            obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
            obj.gameObject.SetActive(true);

            DropBeatProjectile projectile = obj.GetComponent<DropBeatProjectile>();
            if (projectile != null)
            {
                // Add combo bonus damage
                int comboBonus = BeatDetector.Instance?.CurrentCombo ?? 0;
                int comboDamage = quality == ShotQuality.Perfect ? comboBonus * 2 : comboBonus;
                int totalDamage = 20 + comboDamage;

                projectile.Initialize(style.color, totalDamage, quality);
            }
        }

        return obj;
    }
}
