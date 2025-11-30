using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] string propertyName = "_MyFloat";
    [SerializeField] float valueA = 0f;
    [SerializeField] float valueB = 1f;
    [SerializeField] float duration = 0.5f;
    [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);

    Renderer rend;
    MaterialPropertyBlock mpb;
    int propID;
    Coroutine flashCoroutine;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        propID = Shader.PropertyToID(propertyName);
    }

    public void TriggerOnDamage()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        float half = Mathf.Max(0.0001f, duration / 2f);
        
        // Get the property block once at the start
        rend.GetPropertyBlock(mpb);
        
        // A -> B
        for (float t = 0f; t <= half; t += Time.deltaTime)
        {
            float p = ease.Evaluate(t / half);
            float value = Mathf.Lerp(valueA, valueB, p);
            mpb.SetFloat(propID, value);
            rend.SetPropertyBlock(mpb);
            yield return null;
        }

        mpb.SetFloat(propID, valueB);
        rend.SetPropertyBlock(mpb);

        // B -> A
        for (float t = 0f; t <= half; t += Time.deltaTime)
        {
            float p = ease.Evaluate(t / half);
            float value = Mathf.Lerp(valueB, valueA, p);
            mpb.SetFloat(propID, value);
            rend.SetPropertyBlock(mpb);
            yield return null;
        }

        mpb.SetFloat(propID, valueA);
        rend.SetPropertyBlock(mpb);

        flashCoroutine = null;
    }
}
