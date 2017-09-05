using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PauseMenuController : MonoBehaviour {


    [SerializeField]
    float triggerDelay = 0.25f;

    bool isPaused = false;



    public static PauseMenuController Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
    }


   void Update()
    {
       // if(GameManager.Instance.CurrentGameState == GameManager.GameState.PLAYING)
        //{
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetPause(!IsPaused);
            }
      //  }
    }




    public void InflateMenu()
    {
        StopAllCoroutines();
        StartCoroutine(SetAnimatorTriggers("Inflate"));
    }
    public void DeflateMenu()
    {
        StopAllCoroutines();
        StartCoroutine(SetAnimatorTriggers("Deflate"));
    }

    IEnumerator SetAnimatorTriggers(string triggerName)
    {
        Debug.Log("Setting triggers : " + triggerName);
        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).gameObject.SetActive(true);
            Animator _animator = this.transform.GetChild(i).GetComponent<Animator>();

            if (_animator == null)
                continue;


            _animator.SetTrigger(triggerName);

           // Debug.Log("Time1 : " + Time.realtimeSinceStartup);
            yield return new WaitForSeconds(TriggerDelay);
           // Debug.Log("Time2 : " + Time.realtimeSinceStartup);
        }
    }


    public void SetPause(bool pauseVal)
    {
        IsPaused = pauseVal;
        
        if (IsPaused)
        {
            //Time.timeScale = 0f;
            InflateMenu();
        }
        else
        {
           // Time.timeScale = 1f;
            DeflateMenu();
        }
    }



    public bool IsPaused
    {
        get { return isPaused; }
        private set { isPaused = value; }
    }
    public float TriggerDelay
    {
        get { return triggerDelay; }
        private set { triggerDelay = Mathf.Clamp(value, 0f, value); }
    }
	

    void OnValidate()
    {
        TriggerDelay = TriggerDelay;
    }
}
