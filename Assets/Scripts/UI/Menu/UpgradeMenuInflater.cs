﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeMenuInflater : MenuInflater {

    
    List<StatButtonController> m_StatButtons = new List<StatButtonController>();



    protected override void AddButtons()
    {
        StatType[] _types = (StatType[])Enum.GetValues(typeof(StatType));

        for (int i = 0; i < _types.Length; i++)
        {

            if (!activatingPlayer.HasStat(_types[i]))
                continue;

            GameObject _object = Instantiate(buttonPrefab) as GameObject;
            MenuButton _button = _object.GetComponent<MenuButton>();

            if (_button == null)
            {
                Destroy(_object);
                continue;
            }
            
            m_Menu.AddButton(_object);

            _object.transform.localScale = Vector3.one;

            StatButtonController _controller = _button as StatButtonController;
            _controller.Initialize(m_Menu, _types[i], activatingPlayer.GetStatLevel_Current(_types[i]), activatingPlayer.GetStatLevel_Max(_types[i]));
            _controller.OnUpgradeClicked += AttemptStatUpgrade;


            m_StatButtons.Add(_controller);
        }
    }





    void AttemptStatUpgrade(StatType _type)
    {
        int availableCurrency = GameManager.Instance.LevelPoints;


        if (availableCurrency > 0 && activatingPlayer.CanChangeStatLevel(_type, 1))
        {
            activatingPlayer.ChangeStat(_type, 1);
            GameManager.Instance.AddLevelPoints(-1);
            UpdatePanel(_type);
        }
    }

    void UpdatePanel(StatType _type)
    {
        for (int i = 0; i < m_StatButtons.Count; i++)
        {
            if (m_StatButtons[i].StatType == _type)
            {
                m_StatButtons[i].UpdateLevels(activatingPlayer.GetStatLevel_Current(_type), activatingPlayer.GetStatLevel_Max(_type));
            }
        }
    }
}