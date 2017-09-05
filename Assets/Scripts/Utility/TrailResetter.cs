using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TrailRenderer))]
public class TrailResetter : MonoBehaviour {

    void OnEnable()
    {
        StartCoroutine(ResetTrail());
    }


    IEnumerator ResetTrail()
    {
        TrailRenderer myTrail = GetComponent<TrailRenderer>();

        float trailTime = myTrail.time;
        myTrail.time = 0;

        yield return new WaitForSeconds(0f);

        myTrail.time = trailTime;
    }
}
