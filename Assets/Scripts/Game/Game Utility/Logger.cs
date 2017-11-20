using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Logger {

	public static void Log(MonoBehaviour behavior, string logInfo)
    {
        UnityEngine.Debug.Log(logInfo);
    }
}
