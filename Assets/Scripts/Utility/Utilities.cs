using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

public static class Utilities
{
    public static readonly float EPSILON = 0.0001f;

    public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        DrawCube(position, rotation, scale, Vector3.one);
    }
    public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 size)
    {
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawCube(Vector3.zero, size);

        Gizmos.matrix = oldGizmosMatrix;
    }

    public static void DrawWireCube(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        DrawWireCube(position, rotation, scale, Vector3.one);
    }
    public static void DrawWireCube(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 size)
    {
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawWireCube(Vector3.zero, size);

        Gizmos.matrix = oldGizmosMatrix;
    }



    public static double GetRandomGaussian(DeviatingFloat value)
    {
        return GetRandomGaussian(value.Mean, value.Sigma);
    }
    public static double GetRandomGaussian(float mean, float stdDev)
    {

        if (stdDev == 0)
            return mean;


        double u1 = UnityEngine.Random.value;
        double u2 = UnityEngine.Random.value;
        double randStdNormal = Math.Sqrt(-2.0f * Math.Log(u1)) * Math.Sin(2.0f * Math.PI * u2); //random normal(0,1)
        double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

        return randNormal;
    }

    public static T WeightedSelection<T>(T[] objectArray, int[] probabilities)
    {
        return WeightedSelection(objectArray, probabilities, 0);
    }
    public static T WeightedSelection<T>(T[] objectArray, int[] probabilities, float bonus)
    {
        if (objectArray.Length == 0 || (objectArray.Length != probabilities.Length))
            return default(T);

        int totalProb = 0;
        for (int i = 0; i < probabilities.Length; i++)
        {
            totalProb += probabilities[i];
        }

        int randVal = UnityEngine.Random.Range(0, totalProb) + (int)(bonus * totalProb);

        for (int i = 0; i < probabilities.Length; i++)
        {

            randVal -= probabilities[i];

            if (randVal <= 0)
            {
                return objectArray[i];
            }
        }

        return objectArray[objectArray.Length - 1];
    }


    public static T WeightedSelection<T>(WeightedObject<T>[] _objs, float luckBonus)
    {
        T[] weightedObjs = new T[_objs.Length];
        int[] weightedProbs = new int[_objs.Length];

        for (int i = 0; i < _objs.Length; i++)
        {
            weightedObjs[i] = _objs[i].Item1;
            weightedProbs[i] = (int)_objs[i].Item2;
        }

        return WeightedSelection(weightedObjs, weightedProbs, luckBonus);
    }
    public static T WeightedSelection<T>(CustomTuple2<T, int>[] _objs, float luckBonus)
    {
        T[] weightedObjs = new T[_objs.Length];
        int[] weightedProbs = new int[_objs.Length];

        for (int i = 0; i < _objs.Length; i++)
        {
            weightedObjs[i] = _objs[i].Item1;
            weightedProbs[i] = _objs[i].Item2;
        }

        return WeightedSelection(weightedObjs, weightedProbs, luckBonus);
    }

    /*
	public static AudioClip WeightedSelection(WeightedObjectOfAudioClip[] _objs, float luckBonus){
		AudioClip[] weightedObjs = new AudioClip[_objs.Length];
		int[] weightedProbs = new int[_objs.Length];
		
		for(int i = 0; i < _objs.Length; i++){
			weightedObjs[i] = _objs[i].myObject;
			weightedProbs[i] = _objs[i].weight;
		}
		
		return WeightedSelection(weightedObjs, weightedProbs, luckBonus);
	}
	public static LootPool WeightedSelection(WeightedObjectOfLootPool[] _objs, float luckBonus){
		LootPool[] weightedObjs = new LootPool[_objs.Length];
		int[] weightedProbs = new int[_objs.Length];
		
		for(int i = 0; i < _objs.Length; i++){
			weightedObjs[i] = _objs[i].myObject;
			weightedProbs[i] = _objs[i].weight;
		}
		
		return WeightedSelection(weightedObjs, weightedProbs, luckBonus);
	}*/

    public static T WeightedSelection<T>(T[] objectArray, float[] probabilities) { return WeightedSelection(objectArray, probabilities, 0f); }
    public static T WeightedSelection<T>(T[] objectArray, float[] probabilities, float bonus)
    {
        if (objectArray.Length == 0 || (objectArray.Length != probabilities.Length))
            return default(T);

        float totalProb = 0;
        for (int i = 0; i < probabilities.Length; i++)
        {
            totalProb += probabilities[i];
        }

        float randVal = UnityEngine.Random.Range(0, totalProb) + (bonus * totalProb);

        for (int i = 0; i < probabilities.Length; i++)
        {

            randVal -= probabilities[i];


            if (randVal <= 0)
                return objectArray[i];

        }

        return objectArray[objectArray.Length - 1];
    }

    public static Bounds CalculateObjectBounds(Transform t)
    {
        return CalculateObjectBounds(t.gameObject);
    }
    public static Bounds CalculateObjectBounds(GameObject obj)
    {
        Renderer _renderer = obj.GetComponent<Renderer>();

        if (_renderer == null)
            _renderer = obj.GetComponentInChildren<Renderer>();

        if (_renderer == null)
            return new Bounds();

        return _renderer.bounds;
        //return GetMaxBounds(obj);
        // return CalculateObjectBounds(obj, 2, checkTriggers);
    }
    public static Vector3 CalculateObjectBounds(GameObject obj, bool checkTriggers)
    {
        Renderer _renderer = obj.GetComponent<Renderer>();

        if (_renderer == null)
            _renderer = obj.GetComponentInChildren<Renderer>();

        if (_renderer == null)
            return Vector3.zero;

        return _renderer.bounds.size;
        //return GetMaxBounds(obj);
        // return CalculateObjectBounds(obj, 2, checkTriggers);
    }
    public static Vector3 CalculateObjectBounds(GameObject obj, int checkType, bool checkTriggers)
    {
        float x = 0;
        float y = 0;
        float z = 0;

        Vector3 localScale;

        if (checkType == 0 || checkType == 2)
        {
            Renderer[] objRenderers = obj.GetComponents<Renderer>();
            for (int i = 0; i < objRenderers.Length; i++)
            {
                Vector3 objExtents = objRenderers[i].bounds.extents;
                localScale = objRenderers[i].transform.localScale;

                x = Mathf.Max(objExtents.x * localScale.x, x);
                y = Mathf.Max(objExtents.y * localScale.y, y);
                z = Mathf.Max(objExtents.z * localScale.z, z);
            }
        }

        if (checkType == 1 || checkType == 2)
        {
            Collider[] objColls = obj.GetComponents<Collider>();
            for (int i = 0; i < objColls.Length; i++)
            {

                if (objColls[i].isTrigger && !checkTriggers)
                    continue;

                Vector3 objExtents = objColls[i].bounds.extents;
                localScale = objColls[i].transform.localScale;

                x = Mathf.Max(objExtents.x * localScale.x, x);
                y = Mathf.Max(objExtents.y * localScale.y, y);
                z = Mathf.Max(objExtents.z * localScale.z, z);
            }
        }



        for (int i = 0; i < obj.transform.childCount; i++)
        {
            Vector3 temp = Utilities.CalculateObjectBounds(obj.transform.GetChild(i).gameObject, checkTriggers);

            Vector3 toVector = obj.transform.GetChild(i).position - obj.transform.position;

            x = Mathf.Max(x, temp.x + toVector.x);
            y = Mathf.Max(y, temp.y + toVector.y);
            z = Mathf.Max(z, temp.z + toVector.z);
        }


        return new Vector3(x, y, z);
    }

    public static Vector3 GetMaxBounds(Transform t)
    {
        return GetMaxBounds(t.gameObject);
    }
    public static Vector3 GetMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b.size;
    }

    public static Vector3 CalculateInterceptCourse(Vector3 aTargetPos, Vector3 aTargetSpeed, Vector3 aInterceptorPos, float aInterceptorSpeed)
    {
        Vector3 targetDir = aTargetPos - aInterceptorPos;
        float iSpeed2 = aInterceptorSpeed * aInterceptorSpeed;
        float tSpeed2 = aTargetSpeed.sqrMagnitude;
        float fDot1 = Vector3.Dot(targetDir, aTargetSpeed);
        float targetDist2 = targetDir.sqrMagnitude;
        float d = (fDot1 * fDot1) - targetDist2 * (tSpeed2 - iSpeed2);
        if (d < 0.1f)  // negative == no possible course because the interceptor isn't fast enough
            return Vector3.zero;
        float sqrt = Mathf.Sqrt(d);
        float S1 = (-fDot1 - sqrt) / targetDist2;
        float S2 = (-fDot1 + sqrt) / targetDist2;
        if (S1 < 0.0001f)
        {
            if (S2 < 0.0001f)
                return Vector3.zero;
            else
                return (S2) * targetDir + aTargetSpeed;
        }
        else if (S2 < 0.0001f)
            return (S1) * targetDir + aTargetSpeed;
        else if (S1 < S2)
            return (S2) * targetDir + aTargetSpeed;
        else
            return (S1) * targetDir + aTargetSpeed;
    }

    public static float FindClosestPointOfApproach(Vector3 aPos1, Vector3 aSpeed1, Vector3 aPos2, Vector3 aSpeed2)
    {
        Vector3 PVec = aPos1 - aPos2;
        Vector3 SVec = aSpeed1 - aSpeed2;
        float d = SVec.sqrMagnitude;
        // if d is 0 then the distance between Pos1 and Pos2 is never changing
        // so there is no point of closest approach... return 0
        // 0 means the closest approach is now!
        if (d >= -0.0001f && d <= 0.0002f)
            return 0.0f;
        return (-Vector3.Dot(PVec, SVec) / d);
    }






    public static bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return obj == null ? false : IsInLayerMask(obj.layer, mask);
    }
    public static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return ((mask.value & 1 << layer) > 0);
    }
    
    public static bool HasFlag(StatType a, StatType b)
    {
        return (a & b) == b;
    }
    public static bool HasFlag(SpawnWave.CompletionMetric a, SpawnWave.CompletionMetric b)
    {
        return (a & b) == b;
    }
    public static bool HasFlag(Wander.WanderType a, Wander.WanderType b)
    {
        return (a & b) == b;
    }
    public static bool HasFlag(InputType a, InputType b)
    {
        return (a & b) == b;
    }
  

    public static List<StatType> AggregateFlags(StatType s)
    {
        List<StatType> _stats = new List<StatType>();

        if (HasFlag(s, StatType.CriticalDamage))
        {
            _stats.Add(StatType.CriticalDamage);
        }
        if (HasFlag(s, StatType.Damage))
        {
            _stats.Add(StatType.Damage);
        }
        if (HasFlag(s, StatType.Dexterity))
        {
            _stats.Add(StatType.Dexterity);
        }
        if (HasFlag(s, StatType.Health))
        {
            _stats.Add(StatType.Health);
        }
        if (HasFlag(s, StatType.Luck))
        {
            _stats.Add(StatType.Luck);
        }
        if (HasFlag(s, StatType.Speed))
        {
            _stats.Add(StatType.Speed);
        }



        return _stats;
    }






    public static Color GetAttributeColor(string attrString)
    {

        switch (attrString)
        {
            case "Fire":
                return GetAttributeColor(Attribute.Fire);
            case "Ice":
                return GetAttributeColor(Attribute.Ice);
            case "Poison":
                return GetAttributeColor(Attribute.Poison);
            case "Shock":
                return GetAttributeColor(Attribute.Shock);
            case "Water":
                return GetAttributeColor(Attribute.Water);
            case "Visibility":
                return GetAttributeColor(Attribute.Visibility);
            case "Health":
                return GetAttributeColor(Attribute.Health);
            case "Experience":
                return GetAttributeColor(Attribute.Experience);
        }

        return Color.cyan;
    }
    public static Color GetAttributeColor(Attribute attr)
    {
        switch (attr)
        {
            case Attribute.Fire:
                return Values.FIRE_COLOR;
            case Attribute.Ice:
                return Values.ICE_COLOR;
            case Attribute.Poison:
                return Values.POISON_COLOR;
            case Attribute.Shock:
                return Values.SHOCK_COLOR;
            case Attribute.Water:
                return Values.WATER_COLOR;
            case Attribute.Visibility:
                return Values.VISIBILITY_COLOR;
            case Attribute.Health:
                return Values.HEALTH_COLOR;
            case Attribute.Experience:
                return Values.EXPERIENCE_COLOR;
        }

        return Color.magenta;
    }


    public static bool Equals(float a, float b)
    {
        return Mathf.Abs(a - b) <= EPSILON;
    }



    public static Color FadeColor(Color fadeColor, float fadeRate)
    {
        return FadeColor(fadeColor, fadeRate, 0, 1);
    }

    public static Color FadeColor(Color fadeColor, float fadeRate, float alphaMin, float alphaMax)
    {
        Color newColor = fadeColor;
        newColor.a = Mathf.Clamp(newColor.a + fadeRate, alphaMin, alphaMax);

        return newColor;
    }

    public static Color GetComplementaryColor(Color originalColor, float hueVariation)
    {
        Vector3 hsvColor = RGBtoHSV(originalColor);
        hsvColor.x += 180 + UnityEngine.Random.Range(-hueVariation, hueVariation);

        return HSVtoRGB(hsvColor);
    }
    public static Color GetSimilarColor(Color originalColor, float hueVariation)
    {
        return GetSimilarColor(originalColor, hueVariation, 0f, 0f);
    }
    public static Color GetSimilarColor(Color originalColor, float hueVariation, float saturationVariation, float valueVariation)
    {

        //Debug.Log("Original Color: " + originalColor.ToString());

        Vector3 hsvColor = RGBtoHSV(originalColor);

        //Debug.Log("HSV Color : " + hsvColor.ToString());

        //Color a = HSVtoRGB(hsvColor);

        //Debug.Log("HSV to RGB: " + a.ToString());

        hsvColor.x += UnityEngine.Random.Range(-hueVariation, hueVariation);

        if (saturationVariation != 0)
            hsvColor.y += UnityEngine.Random.Range(-saturationVariation, saturationVariation) / 255f;

        if (valueVariation != 0)
            hsvColor.z += UnityEngine.Random.Range(-valueVariation, valueVariation) / 255f;


        if (float.IsNaN(hsvColor.x))
            hsvColor.x = 0.001f;


        if (float.IsNaN(hsvColor.y))
            hsvColor.y = 0.001f;

        if (float.IsNaN(hsvColor.z))
            hsvColor.z = 0.001f;


        Color c = HSVtoRGB(hsvColor);

        //Debug.Log("Similar Color: " + c.ToString());

        return c;
    }

    public static bool IsInOppositeDirection(Vector3 primaryDir, Vector3 secondaryDir)
    {
        return Vector3.Angle(primaryDir, secondaryDir) >= 135f;
    }

    #region HSV
    public static Vector3 RGBtoHSV(Color _color)
    {

        float min, max, delta, h, s, v;

        min = (_color.r < _color.b) ? ((_color.r < _color.g) ? _color.r : _color.g) : ((_color.b < _color.g) ? _color.b : _color.g);
        max = (_color.r > _color.b) ? ((_color.r > _color.g) ? _color.r : _color.g) : ((_color.b > _color.g) ? _color.b : _color.g);

        v = max;
        delta = max - min;

        if (max != 0)
            s = delta / max;        // s
        else
        {
            // r = g = b = 0		// s = 0, v is undefined
            s = 0;
            h = -1;
            return new Vector3(h, s, v);
        }
        if (_color.r == max)
            h = (_color.g - _color.b) / delta;      // between yellow & magenta
        else if (_color.g == max)
            h = 2 + (_color.b - _color.r) / delta;  // between cyan & yellow
        else
            h = 4 + (_color.r - _color.g) / delta;  // between magenta & cyan
        h *= 60;                // degrees
        if (h < 0)
            h += 360;

        return new Vector3(h, s, v);
    }
    public static Color HSVtoRGB(Vector3 hsvVector)
    {
        float r, g, b;
        int i;
        float f, p, q, t;
        if (hsvVector.y == 0)
        {
            // achromatic (grey)
            r = g = b = hsvVector.z;
            return new Color(r, g, b);
        }

        hsvVector.x = (hsvVector.x % 360) / 60f;            // sector 0 to 5
        i = Mathf.FloorToInt(hsvVector.x);
        f = hsvVector.x - i;            // factorial part of h
        p = hsvVector.z * (1 - hsvVector.y);
        q = hsvVector.z * (1 - hsvVector.y * f);
        t = hsvVector.z * (1 - hsvVector.y * (1 - f));
        switch (i)
        {
            case 0:
                r = hsvVector.z;
                g = t;
                b = p;
                break;
            case 1:
                r = q;
                g = hsvVector.z;
                b = p;
                break;
            case 2:
                r = p;
                g = hsvVector.z;
                b = t;
                break;
            case 3:
                r = p;
                g = q;
                b = hsvVector.z;
                break;
            case 4:
                r = t;
                g = p;
                b = hsvVector.z;
                break;
            default:        // case 5:
                r = hsvVector.z;
                g = p;
                b = q;
                break;
        }

        return new Color(r, g, b);
    }

    #endregion


    public static void ActivateAll(GameObject g)
    {
        g.SetActive(true);

        for (int i = 0; i < g.transform.childCount; i++)
        {
            ActivateAll(g.transform.GetChild(i).gameObject);
        }
    }

    public static List<T> GetAllComponents<T>(GameObject obj) where T : Component
    {
        return GetAllComponents<T>(obj.transform);
    }
    public static List<T> GetAllComponents<T>(Transform _transform) where T : Component
    {
        List<T> retList = _transform.GetComponents<T>().ToList();

        for (int i = 0; i < _transform.childCount; i++)
        {
            retList.AddRange(GetAllComponents<T>(_transform.GetChild(i)));
        }


        return retList;
    }


    //public static void SetAll<T>(GameObject obj, bool status) where T:
    //{
    //    T[] components = obj.GetComponents<T>();

    //    for (int i = 0; i < components.Length; i++)
    //    {
    //        components[i].
    //    }

    //}
    public static void SetCollidersEnabled(Transform _transform, bool status)
    {
        SetCollidersEnabled(_transform.gameObject, status);
    }
    public static void SetCollidersEnabled(GameObject obj, bool status)
    {
        Collider[] colliders = obj.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = status;
        }
    }

    public static void SetRenderersEnabled(Transform _transform, bool status)
    {
        SetRenderersEnabled(_transform.gameObject, status);
    }
    public static void SetRenderersEnabled(GameObject obj, bool status)
    {
        MeshRenderer[] renderers = obj.GetComponents<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = status;
        }
    }


    public static Component CopyComponent(Component original, GameObject destination)
    {
        Type type = original.GetType();
        Component copy = destination.AddComponent(type);

        // Copied fields can be restricted with BindingFlags
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy;
    }

    public static void ValidateCurve(AnimationCurve _curve, float _timeMin, float _timeMax, float _valMin, float _valMax)
    {
        ValidateCurve_Times(_curve, _timeMin, _timeMax);
        ValidateCurve_Values(_curve, _valMin, _valMax);
    }


    public static void ValidateCurve_Times(AnimationCurve _curve, float _timeMin, float _timeMax)
    {
        if (_curve == null || _timeMax <= _timeMin)
            return;


        List<Keyframe> _keys = new List<Keyframe>(_curve.keys);

        float minTime = _timeMax;
        float maxTime = _timeMin;

        for (int i = 0; i < _keys.Count; i++)
        {
            Keyframe _frame = _keys[i];
            _frame.time = Mathf.Clamp(_keys[i].time, _timeMin, _timeMax);


            if (_frame.time < minTime)
            {
                minTime = _frame.time;
            }

            if (_frame.time > maxTime)
            {
                maxTime = _frame.time;
            }

            _keys[i] = _frame;
        }



        if (minTime > _timeMin)
        {
            _keys.Add(new Keyframe(_timeMin, 0));
        }

        if (maxTime < _timeMax)
        {
            _keys.Add(new Keyframe(_timeMax, 0));
        }

        _curve.keys = _keys.ToArray();
    }

    public static void ValidateCurve_Values(AnimationCurve _curve, float _valueMin, float _valueMax)
    {
        if (_curve == null || _valueMax <= _valueMin)
            return;


        List<Keyframe> _keys = new List<Keyframe>(_curve.keys);

        for (int i = 0; i < _keys.Count; i++)
        {
            Keyframe _frame = _keys[i];
            _frame.value = Mathf.Clamp(_keys[i].value, _valueMin, _valueMax);

            _keys[i] = _frame;
        }
        _curve.keys = _keys.ToArray();
    }





    public static MeshFilter MakeMeshLowPoly(MeshFilter _filter)
    {
        Mesh _mesh = _filter.mesh;
        var oldVerts = _mesh.vertices;
        var triangles = _mesh.triangles;
        var vertices = new Vector3[triangles.Length];
        for (var i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];
            triangles[i] = i;
        }
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();

        return _filter;
    }
}
