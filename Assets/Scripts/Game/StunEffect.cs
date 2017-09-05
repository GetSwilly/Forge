using UnityEngine;
using System.Collections;


public class StunEffect : MonoBehaviour {

    static readonly float DEFAULT_TIME = 2f;


    bool isActive = false;

    public void Initialize()
    {
        Initialize(DEFAULT_TIME);
    }
    public void Initialize(float _time)
    {
        if (isActive)
            return;

        isActive = true;

        UnitController uController = GetComponent<UnitController>();

        if (uController == null)
            Destroy(this);

        uController.IsOperational = false;
        StartCoroutine(DelayKillScript(_time));
    }
    void OnDisable()
    {
        GetComponent<UnitController>().enabled = true;
    }


    IEnumerator DelayKillScript(float _time)
    {
        yield return new WaitForSeconds(_time);
        Destroy(this);
    }
}
