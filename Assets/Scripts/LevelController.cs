using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestRealm
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField]
        List<GameObject> m_Cores = new List<GameObject>();
        
        [SerializeField]
        List<SpawnWave> m_Waves;

        [SerializeField]
        Dictionary<string, int> creditTracker = new Dictionary<string, int>();

        void Start()
        {
            StartWaves();
        }
        


        public void StartWaves()
        {
            m_Cores.ForEach(c =>
            {
                UIManager.Instance.AddAllyUI(c);
            });

            StartCoroutine(WaveRoutine());
        }


        IEnumerator WaveRoutine()
        {
            for (int i = 0; i < m_Waves.Count; i++)
            {
                m_Waves[i].Initialize(this);

                while (!m_Waves[i].IsWaveComplete())
                {
                    yield return null;
                }
            }


            //IEnumerator SpawnWaveRoutine(SpawnWave wave)
            //{
            //    bool isWaveActive = true;
            //    float waveTimer = 0f;
            //    float spawnTimer = 0f;

            //    yield return null;

            //    while (isWaveActive)
            //    {
            //        spawnTimer += Time.deltaTime;
            //        waveTimer += Time.deltaTime;

            //        if (spawnTimer >= wave.DelayBetweenSpawns)
            //        {
            //            GameObject spawnedObject = wave.SpawnArea.SpawnObject(wave.SpawnPrefab, null);

            //            UtilityActor ai = spawnedObject.GetComponent<UtilityActor>();
            //            if (ai != null)
            //            {
            //                ai.CheckNewObjects(targets, false);
            //            }

            //            spawnTimer = 0f;
            //        }
            //        if (Utilities.HasFlag(wave.Finishers, SpawnWave.WaveFinishers.Time) && waveTimer > wave.MaximumWaveTime)
            //        {
            //            isWaveActive = false;
            //            break;
            //        }


            //        yield return null;
            //    }
            //}
        }


        public List<GameObject> ActiveCores
        {
            get { return m_Cores; }
        }

        void OnValidate()
        {
            m_Waves.ForEach(w => w.Validate());
        }
    }
}
