using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDisable : MonoBehaviour {

    [SerializeField]
    float lifetime = 1f;

    void OnEnable()
    {
        StartCoroutine(Delay());
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(Lifetime);

        this.gameObject.SetActive(false);
    }


    float Lifetime
    {
        get { return lifetime; }
        set { lifetime = Mathf.Clamp(value, 0.1f, value); }
    }

    void OnValidate()
    {
        Lifetime = Lifetime;
    }
}
