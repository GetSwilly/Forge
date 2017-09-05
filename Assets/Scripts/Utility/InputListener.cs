using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputListener : MonoBehaviour
{
    [System.Serializable]
    struct InputEvent
    {

        [SerializeField]
        UnityEvent OnEvent;

        [SerializeField]
        [EnumFlags]
        InputType inputType;


        [SerializeField]
        List<string> validInputs;
        
        [SerializeField]
        List<AxisHolder> validAxes;

        public InputEvent(UnityEvent _event, InputType _type, List<string> _inputs, List<AxisHolder> _axes)
        {
            OnEvent = _event;
            inputType = _type;
            validInputs = _inputs;
            validAxes = _axes;
            
        }


        public UnityEvent Event
        {
            get { return OnEvent; }
        }
        public InputType InputType
        {
            get { return inputType; }
        }
        public List<string> InputButtons
        {
            get { return validInputs; }
        }
        public List<AxisHolder> InputAxes
        {
            get { return validAxes; }
        }
    }

    [System.Serializable]
    struct AxisHolder
    {
        [SerializeField]
        string axisName;

        [SerializeField]
        float minValue;

        [SerializeField]
        float maxValue;

        
        public AxisHolder(string _name, float _min, float _max)
        {
            axisName = _name;
            minValue = _min;
            maxValue = _max;
        }


        public string AxisName
        {
            get { return axisName; }
        }
        public float MinValue
        {
            get { return minValue; }
        }
        public float MaxValue
        {
            get { return maxValue; }
        }
    }


    [SerializeField]
    List<InputEvent> inputs = new List<InputEvent>();

    [SerializeField]
    float inputDelay = 0.6f;

    bool canAcceptInput = true;


    void Update()
    {
        if (!canAcceptInput)
            return;

        bool startDelay = false;

        for (int i = 0; i < inputs.Count; i++)
        {
            InputType _type = inputs[i].InputType;
            List<string> inputButtons = inputs[i].InputButtons;

            for(int k = 0; k < inputButtons.Count; k++)
            {
                try
                {
                    if (Utilities.HasFlag(_type, InputType.Press))
                    {
                       
                        if (Input.GetButtonDown(inputButtons[k]))
                        {
                            inputs[i].Event.Invoke();
                            startDelay = true;
                        }
                    }

                    if (Utilities.HasFlag(_type, InputType.Hold))
                    {
                        if (Input.GetButton(inputButtons[k]))
                        {
                            inputs[i].Event.Invoke();
                            startDelay = true;
                        }
                    }
                }
                catch { continue; }
            }


            List<AxisHolder> inputAxes = inputs[i].InputAxes;
            for(int k = 0; k < inputAxes.Count; k++)
            {
                try
                {
                    float _value = Input.GetAxis(inputAxes[k].AxisName);

                    if (_value >= inputAxes[k].MinValue && _value <= inputAxes[k].MaxValue)
                    {
                        inputs[i].Event.Invoke();
                        startDelay = true;
                    }
                }
                catch { continue; }
            }
        }

        if(startDelay)
            StartCoroutine(InputDelayRoutine());
    }



    IEnumerator InputDelayRoutine()
    {
        canAcceptInput = false;

        yield return new WaitForSeconds(inputDelay);

        canAcceptInput = true;
    }
}
