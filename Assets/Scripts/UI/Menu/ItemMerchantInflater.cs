using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMerchantInflater : MenuInflater {

    static readonly int MAX_MERCHANDISE_CHECKS = 1000;

    [Serializable]
    struct Merchandise {

        [SerializeField]
        GameObject m_Object;

        [SerializeField]
        Cost activeCost;

        ItemPrice m_Price;

        public Merchandise(GameObject obj, ItemPrice _price, CurrencyType _currency, StatType _stat)
        {
            m_Object = obj;
            m_Price = _price;

            activeCost = m_Price.GetCost(_currency, _stat);
        }


        public void SetCost(CurrencyType _currency, StatType _stat)
        {
            ActiveCost = Price.GetCost(_currency, _stat);
        }



        public string Name
        {
            get { return m_Object.name; }
        }
        public GameObject Object
        {
            get { return m_Object; }
            set { m_Object = value; }
        }
        public Cost ActiveCost
        {
            get { return activeCost; }
            set { activeCost = value; }
        }
        public ItemPrice Price
        {
            get { return m_Price; }
        }
    }






    [SerializeField]
    ListDefinitionName m_ListDefinition;


    [SerializeField]
    [EnumFlags]
    CurrencyType acceptedCurrencies;

    [SerializeField]
    [EnumFlags]
    StatType acceptedStatTypes;

    [SerializeField]
    int merchandiseCount = 5;

    [SerializeField]
    bool allowDuplicates = false;

    [SerializeField]
    bool shouldRestock = true;


    [SerializeField]
    Vector3 spawnOffset;


    bool hasStocked = false;

    Dictionary<string, MenuButton> merchandiseToButtonDictionary = new Dictionary<string, MenuButton>();


    List<Merchandise> m_Merchandise = new List<Merchandise>();
    HashSet<string> encounteredItems = new HashSet<string>();


     void OnDisable()
    {
        hasStocked = false;
        RemoveAllMerchandise();
    }


    public override bool Use(PlayerController _player)
    {
        if (!base.Use(_player))
        {
            return false;
        }

        if (hasStocked == true && !shouldRestock)
            return false;

        hasStocked = true;

        Restock();

        return true;
    }

    public void Restock()
    {
        StockMerchant();
        AddButtons();
    }

    protected override void AddButtons()
    {
        for(int i = 0; i < m_Merchandise.Count; i++)
        {
           if (merchandiseToButtonDictionary.ContainsKey(m_Merchandise[i].Name))
               continue;

            IIdentifier _identifier = m_Merchandise[i].Object.GetComponent<IIdentifier>();
            ItemPrice _price = m_Merchandise[i].Object.GetComponent<ItemPrice>();


            GameObject _buttonObject = Instantiate(buttonPrefab) as GameObject;
            MenuButton _button = _buttonObject.GetComponent<MenuButton>();

            if (_button == null || _identifier == null || _price == null)
            {
                Destroy(_buttonObject);
                Destroy(m_Merchandise[i].Object);

                m_Merchandise.RemoveAt(i);
                i--;
                continue;
            }


            //itemSet.Add(_identifier.Name);

            m_Menu.AddButton(_buttonObject);
            
            _button.OnButtonClicked += AttemptPurchase;
            _button.Initialize(m_Menu, null, _identifier.Name);
            

            merchandiseToButtonDictionary.Add(m_Merchandise[i].Name, _button);
        }
        
    }



    private void StockMerchant()
    {
        if (GameManager.Instance == null)
            return;

        List<CurrencyType> currencyList = Utilities.AggregateFlags(acceptedCurrencies);

        if (currencyList.Count == 0)
            return;

        for (int i = 0; i < MAX_MERCHANDISE_CHECKS && m_Merchandise.Count < merchandiseCount; i++)
        {
            GameObject _item = GameManager.Instance.GetItem(m_ListDefinition);

            if (_item == null)
                continue;



            _item.SetActive(false);
            _item.transform.SetParent(this.transform);
            _item.transform.localPosition = Vector3.zero;

            IIdentifier _identifier = _item.GetComponent<IIdentifier>();
            ItemPrice _price = _item.GetComponent<ItemPrice>();


            if (_identifier == null || m_Merchandise.Any(m => m.Name == _identifier.Name) || _price == null || (!allowDuplicates && encounteredItems.Contains(_identifier.Name)))
            {
                Destroy(_item);
                continue;
            }

            CurrencyType chosenCurrency = currencyList[UnityEngine.Random.Range(0, currencyList.Count)];
            StatType chosenStat = StatType.Health;


            if (chosenCurrency == CurrencyType.StatLevel)
            {
                List<StatType> _stats = Utilities.AggregateFlags(acceptedStatTypes);

                if (_stats.Count > 0)
                {
                    chosenStat = _stats[UnityEngine.Random.Range(0, _stats.Count)];
                }
            }


            Merchandise _merchandise = new Merchandise(_item, _price, chosenCurrency, chosenStat);
            m_Merchandise.Add(_merchandise);

            encounteredItems.Add(_identifier.Name);
        }
    }


    private void AttemptPurchase(MenuButton selectedButton)
    {
        Merchandise selectedMerchandise = new Merchandise();
        bool foundMerchandise = false;

       Dictionary<string, MenuButton>.Enumerator _enumerator = merchandiseToButtonDictionary.GetEnumerator();
        while (_enumerator.MoveNext())
        {
            if(_enumerator.Current.Value == selectedButton)
            {
                selectedMerchandise = m_Merchandise.Find(m => m.Name == _enumerator.Current.Key);
                foundMerchandise = true;
            }
        }

        if (!foundMerchandise)
            return;

        


        Cost purchaseCost = selectedMerchandise.ActiveCost;

        if (!activatingPlayer.CanAfford(purchaseCost))
        {
            return;
        }

        GameObject genObj = GameObject.Find("Generated Objects");

        selectedMerchandise.Object.SetActive(true);
        selectedMerchandise.Object.transform.position = m_Transform.TransformPoint(spawnOffset);
        selectedMerchandise.Object.transform.SetParent(genObj == null ? null : genObj.transform);

        m_Merchandise.Remove(selectedMerchandise);
        merchandiseToButtonDictionary.Remove(selectedMerchandise.Name);

        Destroy(selectedButton.gameObject);

        if (shouldRestock)
        {
            Restock();
        }
    }


    private void RemoveAllMerchandise()
    {
        while (m_Merchandise.Count > 0)
        {
            RemoveMerchandise(m_Merchandise[0]);
        }
    }
    private void RemoveMerchandise(Merchandise _merchandise)
    {
        if (!m_Merchandise.Any(m => m.Name == _merchandise.Name))
            return;



        throw new NotImplementedException();
    }

   
}
