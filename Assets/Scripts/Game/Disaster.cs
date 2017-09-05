using UnityEngine;
using System.Collections;

public abstract class Disaster : MonoBehaviour {


    public enum DisasterActivator { TIME, FAVOR }

    [SerializeField]
    DisasterActivator myActivator;

    [SerializeField]
    float timeMean = 0f;

    [SerializeField]
    float timeSigma = 0f;

    [SerializeField]
    [Range(0f, 1f)]
    float activateChance = 1f;


    public virtual void Start()
    {
        GameObject g = GameObject.Find("Generated Objects");
        if (g != null)
            transform.parent = g.transform;

        if (myActivator == DisasterActivator.TIME)
        {
            StartCoroutine(DelayActivation());
        }
        else if (myActivator == DisasterActivator.FAVOR)
        {
            GameManager.Instance.OnFavorChange += AttemptActivate;
        }
    }


    public virtual void OnDisable()
    {
        GameManager.Instance.OnFavorChange -= AttemptActivate;
    }




    IEnumerator DelayActivation()
    {
        yield return new WaitForSeconds(GetTime());

        Activate();
    }

    void AttemptActivate()
    {
        if (Random.value <= activateChance)
            Activate();
    }


    protected abstract void Activate();

    public abstract void Reset();

    public abstract bool IsActive();


    public float GetTime()
    {
        float t = (float)Utilities.GetRandomGaussian(timeMean, timeSigma);

        return t > 0 ? t : 0;
    }


}
