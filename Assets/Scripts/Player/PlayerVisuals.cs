using UnityEngine;
using deVoid.Utils;
using Game.Encounter;

namespace Game.Player
{
    public class PlayerVisuals : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject hands;

        private void OnEnable()
        {
            Signals.Get<PlayerDiedEvent>().AddListener(OnPlayerDied);
            Signals.Get<Encounter_Start>().AddListener(OnEncounterStart);
        }

        private void OnDisable()
        {
            Signals.Get<PlayerDiedEvent>().RemoveListener(OnPlayerDied);
            Signals.Get<Encounter_Start>().RemoveListener(OnEncounterStart);
        }

        private void OnPlayerDied()
        {
            DisableVisuals();
        }

        private void OnEncounterStart(int encounterId)
        {
            EnableVisuals();
        }

        private void DisableVisuals()
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            if (hands != null)
            {
                hands.SetActive(false);
            }
        }

        private void EnableVisuals()
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }

            if (hands != null)
            {
                hands.SetActive(true);
            }
        }
    }
}
