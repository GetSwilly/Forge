using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnWave
{
    static readonly float MINIMUM_SPAWN_DELAY = 0.25f;

    #region Internal Classes

    public enum CompletionMetric
    {
        Time,
        Population
    }

    [System.Serializable]
    class Burst
    {
        public enum Type
        {
            Delay,
            Wave
        }

        [SerializeField]
        Type m_Type;

        [SerializeField]
        float m_Delay;


        [SerializeField]
        GameObject m_Prefab;

        [SerializeField]
        int numberOfSpawns;

        [SerializeField]
        float spawnDelay;
        

        public Type MyType
        {
            get { return m_Type; }
        }
        public float Delay
        {
            get { return m_Delay; }
            private set { m_Delay = Mathf.Clamp(value, 0f, value); }
        }

        public GameObject Prefab
        {
            get { return m_Prefab; }
        }
        public int NumberOfSpawns
        {
            get { return numberOfSpawns; }
            private set { numberOfSpawns = Mathf.Clamp(value, 0, value); }
        }
        public float SpawnDelay
        {
            get { return spawnDelay; }
            private set { spawnDelay = Mathf.Clamp(value, 0, value); }
        }


        public void Validate()
        {
            Delay = Delay;
            NumberOfSpawns = NumberOfSpawns;
            SpawnDelay = SpawnDelay;
        }
    }

    [System.Serializable]
    class CompletionCriteria
    {
        [EnumFlags]
        public CompletionMetric m_Finishers;

        [SerializeField]
        public float time;
        bool isTimeCompleted = false;

        [SerializeField]
        public int kills = 0;


        public bool IsComplete()
        {
            CompletionMetric[] finisherTypes = System.Enum.GetValues(typeof(CompletionMetric)) as CompletionMetric[];

            for (int i = 0; i < finisherTypes.Length; i++)
            {
                if (!Utilities.HasFlag(m_Finishers, finisherTypes[i]))
                    continue;

                switch (finisherTypes[i])
                {
                    case CompletionMetric.Population:

                        break;
                    case CompletionMetric.Time:
                        if (!IsTimeCompleted)
                        {
                            return false;
                        }

                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        public float Time
        {
            get { return time; }
            private set { time = Mathf.Clamp(value, 0f, value); }
        }
        public bool IsTimeCompleted
        {
            private get { return isTimeCompleted; }
            set { isTimeCompleted = value; }
        }
        public int Kills
        {
            get { return kills; }
            private set { kills = Mathf.Clamp(value, 0, value); }
        }

        public void Validate()
        {
            Time = Time;
            Kills = Kills;
        }
    }

    #endregion


    //[EnumFlags]
    //[SerializeField]
    //WaveFinishers m_Finishers;

    [SerializeField]
    SpawnArea m_SpawnArea;
    
    [SerializeField]
    List<Burst> m_Bursts;

    [SerializeField]
    CompletionCriteria finishCriteria;

    TestRealm.LevelController m_Controller;


    public void Initialize(TestRealm.LevelController _controller)
    {
        m_Controller = _controller;
        m_Controller.StartCoroutine(Wait(finishCriteria.Time));
        m_Controller.StartCoroutine(SpawnRoutine());
    }


    IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        
        finishCriteria.IsTimeCompleted = true;
    }
    IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < m_Bursts.Count; i++)
        {
            if (m_Bursts[i].MyType == Burst.Type.Delay)
            {
                yield return new WaitForSeconds(m_Bursts[i].Delay);
            }
            else if (m_Bursts[i].MyType == Burst.Type.Wave)
            {
                for (int k = 0; k < m_Bursts[i].NumberOfSpawns; k++)
                {
                    GameObject g = m_SpawnArea.SpawnObject(m_Bursts[i].Prefab, null);

                    Health _health = g.GetComponent<Health>();
                    if (_health != null)
                    {
                        _health.OnKilled += SpawnKilled;
                    }
                    InitializeNewSpawn(g, m_Bursts[i]);

                    yield return new WaitForSeconds(m_Bursts[i].SpawnDelay);
                }
            }
            yield return null;
        }
    }

    void InitializeNewSpawn(GameObject obj, Burst burst)
    {
        UtilityActor ai = obj.GetComponent<UtilityActor>();
        if (ai != null)
        {
            ai.AlertObjects(m_Controller.ActiveCores, false);
        }
    }

    void SpawnKilled(Health _health)
    {

    }


    public bool IsWaveComplete()
    {
        return finishCriteria.IsComplete();
    }

    //#region Accessors
    //public SpawnArea SpawnArea
    //{
    //    get { return m_SpawnArea; }
    //}
    //public float RequiredWaveTime
    //{
    //    get { return finisherTime; }
    //    private set { finisherTime = Mathf.Clamp(value, 0f, value); }
    //}
    //public int RequiredFinisherPopulation
    //{
    //    get { return requiredFinisherPopulation; }
    //    private set { requiredFinisherPopulation = Mathf.Clamp(value, 0, value); }
    //}

    //#endregion

    public void Validate()
    {
        finishCriteria.Validate();
        m_Bursts.ForEach(b => b.Validate());
    }

}
