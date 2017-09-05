using UnityEngine;
using System.Collections;
using System;

public class Weave : BaseUtilityBehavior {

    [SerializeField]
    [Range(0f, 180f)]
    float minAngle = 0f;

    [SerializeField]
    [Range(0f, 180f)]
    float maxAngle = 0f;

    [SerializeField]
    float weaveDistanceMean = 1f;

    [SerializeField]
    float weaveDistanceSigma = 0f;



    IEnumerator WeaveRoutine()
    {

        float _angle = UnityEngine.Random.Range(minAngle, maxAngle);
        _angle *= UnityEngine.Random.value <= 0.5f ? 1f : -1f;

        Vector3 moveDir = Quaternion.AngleAxis(_angle, m_Transform.up) * m_Transform.forward;


        Node _node = A_Star_Pathfinding.Instance.NodeFromWorldPoint(m_Transform.position + (moveDir.normalized * (float)Utilities.GetRandomGaussian(weaveDistanceMean, weaveDistanceSigma)));


        if (_node != null && !_node.IsWalkable(m_Actor.WalkableNodes))
        {
            m_Actor.FindPathTo(_node.WorldPosition);

            float timer = m_Actor.UpdatePathTime;
            while (timer > 0 && m_Actor.MoveAlongPath())
            {
                yield return null;
                timer -= Time.deltaTime;
            }
        }
        
        EndBehavior(true, true);
    }



    public override void StartBehavior()
    {
        IsActive = true;

        StartCoroutine(WeaveRoutine());
    }

    


    public override bool CanStartSubBehavior
    {
       get { throw new NotImplementedException(); }
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }

    public override float GetBehaviorScore()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return "Weave";
    }
}
