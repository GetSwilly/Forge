using SensorToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRangeSensor : RangeSensor {

    [SerializeField]
    public SensorEventHandler OnStayDetected;

    void Update()
    {
        var detectedEnumerator = DetectedObjects.GetEnumerator();
        while (detectedEnumerator.MoveNext())
        {
            OnStayDetected.Invoke(detectedEnumerator.Current);
        }
    }
}
