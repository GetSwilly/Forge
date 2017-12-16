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
        TrailRenderer m_Trail = GetComponent<TrailRenderer>();

        float trailTime = m_Trail.time;
        m_Trail.time = 0;

        yield return new WaitForSeconds(0f);

        m_Trail.time = trailTime;
    }
}
