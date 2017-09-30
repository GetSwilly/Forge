using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStat
{
    event Delegates.StatChanged OnLevelChanged;

    int GetCurrentStatLevel(StatType _type);
    int GetMaxStatLevel(StatType _type);

    bool CanChangeStatLevel(StatType _type, int _delta);
    bool CanChangeStatLevel(StatType _type, int _delta, bool canOvershootMax);

    void ChangeStat(StatType _type, int _delta);

    //bool HasStat(StatType _type);
    //float GetStatValue(StatType _type);
}
