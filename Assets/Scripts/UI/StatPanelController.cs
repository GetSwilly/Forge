using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatPanelController : MenuButton {

    public delegate void ClickAction_Stat(StatType _type);
    public ClickAction_Stat OnUpgradeClicked;



    StatType myStatType;

    [SerializeField]
    Text statName;

    [SerializeField]
    Color fullColor;

    [SerializeField]
    Color disabledColor;

    [SerializeField]
    GameObject levelTickPrefab;

    [SerializeField]
    Transform levelTickParent;

   // UpgradeMenu myUpgradeMenu;
   // UpgradeMenuInflater myUpgradeMenuInflater;
    GameObject[] levelTicks;



    /*
    public override void Initialize(Menu _menu)
    {
        base.Initialize(_menu);

        myUpgradeMenu = myMenu as UpgradeMenu;
        myUpgradeMenuInflater = myMenu.Inflater as UpgradeMenuInflater;
    }
    */
    public void Initialize(Menu _menu, StatType _type, int currentLevel, int maxLevel)
    {
        base.Initialize(_menu);

        MyStatType = _type;
        SetName();
        UpdateLevel(currentLevel, maxLevel);
    }



    public override void ButtonClicked()
    {
        base.ButtonClicked();

        if (OnUpgradeClicked != null)
            OnUpgradeClicked(MyStatType);
    }


    public void UpdateLevel(int newLevel, int maxLevel)
    {
        if (levelTicks != null)
        {
            for (int i = 0; i < levelTicks.Length; i++)
                Destroy(levelTicks[i].gameObject);
        }

        levelTicks = new GameObject[maxLevel];

        for (int i = 0; i < levelTicks.Length; i++)
        {
            GameObject obj = Instantiate(levelTickPrefab);
            obj.transform.SetParent(levelTickParent);
            obj.transform.localScale = Vector3.one;
            Vector3 pos = obj.transform.localPosition;
            pos.z = 0;
            obj.transform.localPosition = pos;

            Color setColor = (i + 1 <= newLevel) ? fullColor : disabledColor;
            obj.GetComponent<Image>().color = setColor;

            levelTicks[i] = obj;
        }
    }

    /*
	void UpgradeButtonPressed(){

        if (myUpgradeMenuInflater.AttemptStatUpgrade(MyStatType))
        {

        }

        if (myUpgradeMenu != null && myUpgradeMenu.AttemptStatUpgrade(myStatType)){
            UpdateLevel(myUpgradeMenu.GetStatLevel_Current(myStatType), myUpgradeMenu.GetStatLevel_Max(myStatType));
		} 
	}




    public bool AttemptStatUpgrade(StatType _type)
    {
        int availableCurrency = GameManager.Instance.LevelPoints;

        if (availableCurrency > 0 && mainPlayer.CanChangeStatLevel(_type, 1))
        {
            mainPlayer.ChangeStat(_type, 1);
            GameManager.Instance.AddLevelPoints(-1);
            return true;
        }

        return false;
    }

    public int GetStatLevel_Current(StatType _type)
    {
        return mainPlayer.GetStatLevel_Current(_type);
    }
    public int GetStatLevel_Max(StatType _type)
    {
        return mainPlayer.GetStatLevel_Max(_type);
    }
    */


    public void SetName()
    {
        string s = "";


        switch (myStatType)
        {
            case StatType.Health:
                s = "Health";
                break;
            case StatType.Speed:
                s = "Speed";
                break;
            case StatType.Dexterity:
                s = "Dexterity";
                break;
            case StatType.Damage:
                s = "Damage";
                break;
            case StatType.CriticalDamage:
                s = "Critical Damage";
                break;
            case StatType.Luck:
                s = "Luck";
                break;

        }

        Name = s;
    }


	public string Name{
		get { return statName.text; }
		set { statName.text = value; }
	}
    public StatType MyStatType
    {
        get { return myStatType; }
        set { myStatType = value; }
    }
    /*
    public UpgradeMenu UpgradeMenu
    {
        set { myUpgradeMenu = value; }
    }
    */
}
