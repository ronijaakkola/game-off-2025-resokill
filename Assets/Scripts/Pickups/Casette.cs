using deVoid.Utils;
using Game.Encounter;
using UnityEngine;

public class Casette : Pickup
{
    [Header("References")]
    [SerializeField] private Transform bubbleTransform;
    [SerializeField] private Collider pickupCollider;
    [SerializeField] private GameObject particles;

    void Awake()
    {
        Signals.Get<Encounter_Start>().AddListener(ReactToEncounterStart);
        Signals.Get<Encounter_End>().AddListener(ReactToEncounterEnd);

        ToggleBubble(false);
        pickupCollider.enabled = true;
        particles.SetActive(true);
    }
    
    protected override void OnPickup(GameObject player)
    {
        var encounterManager = FindAnyObjectByType<EncounterManager>();
        if (encounterManager != null)
        {
            encounterManager.GetComponent<EncounterManager>().StartEncounter();
            ToggleBubble(true);
        }
    }

    private void ToggleBubble(bool isActive)
    {
        if (bubbleTransform != null)
        {
            bubbleTransform.gameObject.SetActive(isActive);
        }
    }

    void ReactToEncounterStart(int encounterId)
    {
        ToggleBubble(true);
        pickupCollider.enabled = false;
        particles.SetActive(false);
    }

    void ReactToEncounterEnd(int encounterId)
    {
        ToggleBubble(false);
        pickupCollider.enabled = true;
        particles.SetActive(true);
    }

    void OnDestroy()
    {
        Signals.Get<Encounter_Start>().RemoveListener(ReactToEncounterStart); 
        Signals.Get<Encounter_End>().RemoveListener(ReactToEncounterEnd);
    }
}
