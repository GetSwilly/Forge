using UnityEngine;
using System.Collections;

public static class PositionalPrediction {

    ///<summary>Uses one greater calculation to predict the position. (a small amount of time slower than predictRigidbodyVelocity in the benchmark/ maybe faster when dragValue=0).\nNot tested for dragValue=0
    public static Vector3 predictRigidbodyVelocityWithoutLoop(Vector3 startVelocity, int cycles, float dragValue, bool _2DGravity, float gravityModifier)
    {
        //timestep
        float t = Time.fixedDeltaTime;
        //drag how it is used by the unity physics
        float d = Mathf.Clamp01(1.0f - (dragValue * t));
        //gravity with modifier
        Vector3 g;
        if (_2DGravity) g = Physics2D.gravity * gravityModifier;
        else g = Physics.gravity * gravityModifier;

        if (dragValue == 0) return startVelocity + cycles * t * g;
        return Mathf.Pow(d, cycles) * startVelocity + t * g * (((1 - Mathf.Pow(d, cycles+1)) / (1f - d)) - 1f);
    }

    ///<summary>A small amount of time faster than predictRigidbodyVelocityWithoutLoop in the benchmark with cycles=100 (this method uses a lot of simple calculations which are easy and fast to calculate for your CPU).
    public static Vector3 predictRigidbodyVelocity(Vector3 startVelocity, int cycles, float dragValue, bool _2DGravity, float gravityModifier)
    {
        //timestep
        float t = Time.fixedDeltaTime;
        //drag how it is used by the unity physics
        float d = Mathf.Clamp01(1.0f - (dragValue * t));
        //gravity with modifier
        Vector3 g;
        if (_2DGravity) g = Physics2D.gravity * gravityModifier;
        else g = Physics.gravity * gravityModifier;
        
        Vector3 currentVelocity = startVelocity;

        for (int currentCycle = 0; currentCycle < cycles; currentCycle++)
        {
            currentVelocity = d * (currentVelocity + t * g);
        }

        return currentVelocity;
    }

    ///<summary>use for benchmarking
    public static void test()
    {
        /***       TEST 1: Math.Pow vs Mathf.Pow
        Debug.Log("0.995^99 is testet in different ways:\nReal Result: 0.608814509035907754665563152497006774430414393647862275565 \n");
        Debug.Log("Double Math.Pow converted to float: "+(float)System.Math.Pow(0.995, 99));
        Debug.Log("float Math.Pow: " + Mathf.Pow(0.995f, 99f));
        double manuallyCalculated = 1;
        for(int a=0; a < 99; a++)
        {
            manuallyCalculated *= 0.995;
        }
        Debug.Log("Manually lineal double converted to float: "+(float)manuallyCalculated);
        */

        //TEST 2: Calc Time of different functions in this class
        Debug.Log("predictRigidbodyPosition vs predictRigidbodyPositionWithoutLoop calculation time test");

        float start = Time.realtimeSinceStartup;
        predictRigidbodyPosition(new Vector3(9.345f, 3.9f, 0f), new Vector3(9.345f, 3.9f, 0f), 100, 1.5f, true, 1f);
        Debug.Log("predictRigidbodyPosition time: " + (Time.realtimeSinceStartup - start));

        start = Time.realtimeSinceStartup;
        predictRigidbodyPositionWithoutLoop(new Vector3(9.345f, 3.9f, 0f), new Vector3(9.345f, 3.9f, 0f), 100, 1.5f, true, 1f);
        Debug.Log("predictRigidbodyPositionWithoutLoop time: " + (Time.realtimeSinceStartup - start));
        
        start = Time.realtimeSinceStartup;
        predictAllRigidbodyPositionsTillCycle(new Vector3(9.345f, 3.9f, 0f), new Vector3(9.345f, 3.9f, 0f), 100, 1.5f, true, 1f);
        Debug.Log("predictRigidbodyPositionsTillCycle time: " + (Time.realtimeSinceStartup - start));


        start = Time.realtimeSinceStartup;
        predictRigidbodyVelocity(new Vector3(9.345f, 3.9f, 0f), 100, 1.5f, true, 1f);
        Debug.Log("predictRigidbodyVelocity time: " + (Time.realtimeSinceStartup - start));

        start = Time.realtimeSinceStartup;
        predictRigidbodyVelocityWithoutLoop(new Vector3(9.345f, 3.9f, 0f), 100, 1.5f, true, 1f);
        Debug.Log("predictRigidbodyVelocityWithoutLoop time: " + (Time.realtimeSinceStartup - start));

    }

    ///<summary>Uses one greater calculation to predict the position. (a huge amount of time slower than predictRigidbodyPosition in the benchmark).\nNot tested for dragValue=0
    public static Vector3 predictRigidbodyPositionWithoutLoop(Vector3 startPosition, Vector3 startVelocity, int cycles, float dragValue, bool _2DGravity, float gravityModifier)
    {
        //timestep
        float t = Time.fixedDeltaTime;
        //drag how it is used by the unity physics
        float d = Mathf.Clamp01(1.0f - (dragValue * t));
        //gravity with modifier
        Vector3 g;
        if (_2DGravity) g = Physics2D.gravity * gravityModifier;
        else g = Physics.gravity * gravityModifier;

        float d_raised_to_cycles = (float)System.Math.Pow(d, cycles);
        float d_raised_to_cyclesPlusOne = (float)System.Math.Pow(d, cycles + 1);

        //used for calculations
        float geometricSumOfDrag = (1 - d_raised_to_cyclesPlusOne) / (1 - d);

        if (dragValue == 0) return startPosition + cycles * t * (startVelocity + t * (cycles + 1) / 2 * g);
        return startPosition + (float)cycles * t * t * d * g + (t * d) * (startVelocity * (geometricSumOfDrag - d_raised_to_cycles) + ((float)cycles * d + 1 - geometricSumOfDrag) * (t / (1 - d)) * g);
    }

    ///<summary>A lot faster than predictRigidbodyPositionWithoutLoop in the benchmark with cycles=100 (this method uses a lot of simple calculations which are easy and fast to calculate for your CPU, incredibly fast method (like 10x faster)).
    public static Vector3 predictRigidbodyPosition(Vector3 startPosition, Vector3 startVelocity, int cycles, float dragValue, bool _2DGravity, float gravityModifier)
    {
        //timestep
        float t = Time.fixedDeltaTime;
        //drag how it is used by the unity physics
        float d = Mathf.Clamp01(1.0f - (dragValue * t));
        //gravity with modifier
        Vector3 g;
        if (_2DGravity) g = Physics2D.gravity * gravityModifier;
        else g = Physics.gravity * gravityModifier;

        Vector3 currentPosition = startPosition;
        Vector3 currentVelocity = startVelocity;


        for (int currentCycle = 0; currentCycle < cycles; currentCycle++)
        {
            currentPosition = currentPosition + t * d * currentVelocity + t * d * t * g;
            //update velocity AFTER the position to use the velocity from last cycle
            currentVelocity = d * (currentVelocity + t * g);
        }

        return currentPosition;
    }

    ///<summary>PredictedPositionData wraps all position and velocity values till the input cycle! (uses the same algorithm as predictRigidbodyPosition but saves all temporary values in the wrapper class)
    public static PredictedPositionData predictAllRigidbodyPositionsTillCycle(Vector3 startPosition, Vector3 startVelocity, int cycles, float dragValue, bool _2DGravity, float gravityModifier)
    {
        //timestep
        float t = Time.fixedDeltaTime;
        //drag how it is used by the unity physics
        float d = Mathf.Clamp01(1.0f - (dragValue * t));
        //gravity with modifier
        Vector3 g;
        if (_2DGravity) g = Physics2D.gravity * gravityModifier;
        else g = Physics.gravity * gravityModifier;

        Vector3 currentPosition = startPosition;
        Vector3 currentVelocity = startVelocity;
        PredictedPositionData data = new PredictedPositionData(cycles, _2DGravity);
        data.predictedPosition[0] = currentPosition;
        data.predictedVelocity[0] = currentVelocity;


        for (int currentCycle = 0; currentCycle < cycles; currentCycle++)
        {
            currentPosition = currentPosition + t * d * currentVelocity + t * d * t * g;
            //update velocity AFTER the position to use the velocity from last cycle
            currentVelocity = d * (currentVelocity + t * g);

            data.predictedPosition[currentCycle + 1] = currentPosition;
            data.predictedVelocity[currentCycle + 1] = currentVelocity;
        }

        return data;
    }

    /// <summary>
    /// Wraps Predicted data (EXAMPLE: predictedPosition[4] means predicted position after 4 cycles, [0] is current position)
    /// </summary>
    public class PredictedPositionData
    {
        public bool is2DData;

        ///<summary>Predicted data (EXAMPLE: predictedPosition[4] means predicted position after 4 cycles, [0] is current position)
        public Vector3[] predictedPosition;
        ///<summary>Predicted data (EXAMPLE: predictedVelocity[4] means predicted velocity after 4 cycles, [0] is current velocity)
        public Vector3[] predictedVelocity;

        public PredictedPositionData(int predictedCycles, bool is2DData)
        {
            this.is2DData = is2DData;
            predictedPosition = new Vector3[predictedCycles + 1];
            predictedVelocity = new Vector3[predictedCycles + 1];
        }
    }

    /***
    //official physics
    float rigidbodyDrag = Mathf.Clamp01(1.0f - (body.drag * Time.fixedDeltaTime));

    Vector2 velocityPerFrame = body.velocity + (Physics2D.gravity * Time.fixedDeltaTime);
    velocityPerFrame *= rigidbodyDrag;
    Vector3 posAfterFrame = transform.position + (Vector3)(velocityPerFrame * Time.fixedDeltaTime);
    */
}
