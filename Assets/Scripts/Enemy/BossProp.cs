using deVoid.Utils;
using Game.Encounter;
using System.Collections;
using UnityEngine;

namespace Game.CharacterEnemy
{
    public class BossProp : MonoBehaviour
    {
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Vector3 targetScale;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        Animator animator;

        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        void OnEnable()
        {
            animator.Rebind();
            animator.Update(0f);
            animator.enabled = false;

            Signals.Get<Encounter_End>().AddListener(ReactToEncounterEnd);
            StartCoroutine(ScaleUp());
        }

        void OnDisable()
        {
            animator.ResetTrigger("DieCritical");
            Signals.Get<Encounter_End>().RemoveListener(ReactToEncounterEnd);
        }

        IEnumerator ScaleUp()
        {
            Vector3 startScale = new Vector3(50, 50, 50);
            transform.localScale = startScale;

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);

                float curveValue = scaleCurve.Evaluate(t);
                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);

                yield return null;
            }

            transform.localScale = targetScale;
        }

        private void ReactToEncounterEnd(int encounterId)
        {
            animator.enabled = true;
            animator.SetTrigger("DieCritical");
        }
    }
}
