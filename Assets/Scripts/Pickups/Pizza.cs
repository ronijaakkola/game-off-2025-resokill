using deVoid.Utils;
using Game.Audio;
using Game.Common;
using Game.Player;
using UnityEngine;

public class Pizza : Pickup
{
    [SerializeField] private int healAmount = 1;

    protected override void OnPickup(GameObject player)
    {
        // Heal the player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Don't pick up pizza if player is at full health
            if (playerHealth.IsFullHealth())
            {
                return;
            }

            playerHealth.AddHealth(healAmount);
        }

        AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.PlayerHeal, transform.position);

        ObjectPooler.Instance.ReturnToPool(gameObject);
    }

    void ReactToPlayerDeath()
    {
        if (gameObject.activeInHierarchy && ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(gameObject);
        }
    }

    void OnEnable()
    {
        Signals.Get<PlayerDiedEvent>().AddListener(ReactToPlayerDeath);
    }

    void OnDisable()
    {
        Signals.Get<PlayerDiedEvent>().RemoveListener(ReactToPlayerDeath);
    }
}
