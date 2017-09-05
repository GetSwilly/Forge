using UnityEngine;
using System.Collections;

[System.Serializable]
public class WeightedObject<T> : CustomTuple2<T, float>
{
    public WeightedObject(T _obj, float _weight) : base(_obj, _weight) { }
}

[System.Serializable]
public class WeightedObjectOfGameObject : WeightedObject<GameObject>{


    public WeightedObjectOfGameObject(GameObject _obj, float _weight) : base(_obj, _weight) { }
}


[System.Serializable]
public class WeightedObjectOfSoundClip : WeightedObject<SoundClip>
{

    public WeightedObjectOfSoundClip(SoundClip _clip, float _weight) : base(_clip, _weight) { }
}


[System.Serializable]
public class WeightedObjectOfUtilityBehavior : WeightedObject<BaseUtilityBehavior>
{

    public WeightedObjectOfUtilityBehavior(BaseUtilityBehavior _behavior, float _weight) : base(_behavior, _weight) { }
    
}

