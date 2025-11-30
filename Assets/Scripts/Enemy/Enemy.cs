using deVoid.Utils;
using Game.Audio;
using Game.Common;
using Game.Encounter;
using MoreMountains.Feedbacks;
using Pathfinding;
using System.Collections;
using UnityEngine;  

namespace Game.CharacterEnemy
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] float maxHealth = 15f;
        [SerializeField] DamageFlash damageFlash;
        [SerializeField] Transform floatingTextSpawnPoint;

        [SerializeField] private float health;

        [SerializeField] float chanceToDropPizza;
        [SerializeField] GameObject pizza;

        MMF_Player floatingTextPlayer;

        RichAI pathfinding;
        Renderer enemyRenderer;
        MaterialPropertyBlock materialPropertyBlock;

        // Cached shader property ID
        private static readonly int GlowColorProperty = Shader.PropertyToID("_GlowColor");

        void Awake()
        {
            health = maxHealth;
            pathfinding = GetComponent<RichAI>();
            floatingTextPlayer = GameObject.Find("Floating Text Feedback").GetComponent<MMF_Player>();

            // Cache renderer and create material property block
            enemyRenderer = GetComponentInChildren<Renderer>();
            materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void TakeDamage(float hit, ShotQuality shotQuality = ShotQuality.Good, bool forceCritical = false)
        {
            //Debug.Log($"Enemy took dmg: from {health} to {health - hit}");

            health -= hit;

            damageFlash.TriggerOnDamage();
            floatingTextPlayer.PlayFeedbacks(floatingTextSpawnPoint.position, hit);

            if (health <= 0)
            {
                Die(shotQuality, forceCritical);
            }
        }

        void Die(ShotQuality killingQuality = ShotQuality.Good, bool forceCritical = false)
        {
            string deathTrigger = (forceCritical || killingQuality == ShotQuality.Perfect) ? "DieCritical" : "Die";
            animator.SetTrigger(deathTrigger);

            pathfinding.enabled = false;
            gameObject.layer = LayerMask.NameToLayer("NoHit");

            Signals.Get<Enemy_Died>().Dispatch(gameObject);

            var random = Random.Range(0.0f, 100f);
            if (random <= chanceToDropPizza)
            {
                GameObject obj = ObjectPooler.Instance.GetPooledObject(pizza);
                if (obj != null)
                {
                    obj.transform.SetPositionAndRotation(transform.position + (Vector3.up * 0.75f), Quaternion.identity);
                }
            }

            StartCoroutine(DestroyAfterDelay(5f));
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            ObjectPooler.Instance.ReturnToPool(gameObject);
        }

        void OnCollisionEnter(Collision collision)
        {
            DamagePlayer(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            DamagePlayer(collision);
        }

        private void DamagePlayer(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null && !playerHealth.IsInvulnerable())
                {
                    playerHealth.RemoveHealth(1);
                }
            }
        }

        public void ReviveEnemy(int enemyLevel, Vector3 v3Pos)
        {
            pathfinding.enabled = true;
            health = maxHealth;
            transform.position = v3Pos;

            gameObject.layer = LayerMask.NameToLayer("Enemy");

            // Apply current theme color to enemy
            ApplyThemeColor();
        }

        private void ApplyThemeColor()
        {
            if (ThemeSwitcher.Instance != null && enemyRenderer != null)
            {
                Color currentGlowColor = ThemeSwitcher.Instance.GetCurrentEnemyGlowColor();

                // Get existing property block to preserve other properties
                enemyRenderer.GetPropertyBlock(materialPropertyBlock);

                // Set the glow color
                materialPropertyBlock.SetColor(GlowColorProperty, currentGlowColor);

                // Apply the property block back to the renderer
                enemyRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }

        void ReactToEncounterEnd(int id)
        {
            Die(ShotQuality.Good);
        }

        void OnEnable()
        {
            Signals.Get<Encounter_End>().AddListener(ReactToEncounterEnd);
        }

        void OnDisable()
        {
            Signals.Get<Encounter_End>().RemoveListener(ReactToEncounterEnd);
        }
    }
}
