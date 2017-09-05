using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemEmitter : MonoBehaviour {
    
    [System.Serializable]
    public struct ParticleSystemEmission
    {
        public enum EmissionPlacement { Pivot, Ground };
        public EmissionPlacement m_Placement;

        [SerializeField]
        private Vector3 m_Offset;

        [SerializeField]
        private ParticleSystem m_ParticleSystem;

        public ParticleSystemEmission(EmissionPlacement _placement, ParticleSystem _system, Vector3 _offset)
        {
            m_Placement = _placement;
            m_ParticleSystem = _system;
            m_Offset = _offset;
        }
    }



    [SerializeField]
    public List<ParticleSystemEmission> emissions = new List<ParticleSystemEmission>();
}
