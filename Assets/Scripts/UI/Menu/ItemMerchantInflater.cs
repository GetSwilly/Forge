using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMerchantInflater : MenuInflater {

    static readonly int MAX_MERCHANDISE_CHECKS = 1000;
    

    [SerializeField]
    ListDefinitionName m_ListDefinition;

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


    List<ItemPrice> m_Prices = new List<ItemPrice>();
    HashSet<string> encounteredItems = new HashSet<string>();


     void OnDisable()
    {
        hasStocked = false;
        RemoveAllPrices();
    }


    public override bool Interact(PlayerController _player)
    {
        if (!base.Interact(_player))
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
        for(int i = 0; i < m_Prices.Count; i++)
        {
           if (merchandiseToButtonDictionary.ContainsKey(m_Prices[i].gameObject.name))
               continue;

            IIdentifier _identifier = m_Prices[i].GetComponent<IIdentifier>();


            GameObject _buttonObject = Instantiate(buttonPrefab) as GameObject;
            MenuButton _button = _buttonObject.GetComponent<MenuButton>();

            if (_button == null || _identifier == null)
            {
                Destroy(_buttonObject);
                Destroy(m_Prices[i].gameObject);

                m_Prices.RemoveAt(i);
                i--;
                continue;
            }


            //itemSet.Add(_identifier.Name);

            m_Menu.AddButton(_buttonObject);
            
            _button.OnButtonClicked += AttemptPurchase;
            _button.Initialize(m_Menu, null, _identifier.Name);
            

            merchandiseToButtonDictionary.Add(m_Prices[i].gameObject.name, _button);
        }
        
    }



    private void StockMerchant()
    {
        if (GameManager.Instance == null)
            return;
     
        for (int i = 0; i < MAX_MERCHANDISE_CHECKS && m_Prices.Count < merchandiseCount; i++)
        {
            GameObject _item = GameManager.Instance.GetItem(m_ListDefinition);

            if (_item == null)
                continue;



            _item.SetActive(false);
            _item.transform.SetParent(this.transform);
            _item.transform.localPosition = Vector3.zero;

            IIdentifier _identifier = _item.GetComponent<IIdentifier>();
            ItemPrice _price = _item.GetComponent<ItemPrice>();


            if (_identifier == null || m_Prices.Any(m => m.gameObject.name == _identifier.Name) || _price == null || (!allowDuplicates && encounteredItems.Contains(_identifier.Name)))
            {
                Destroy(_item);
                continue;
            }

            m_Prices.Add(_price);

            encounteredItems.Add(_identifier.Name);
        }
    }


    private void AttemptPurchase(MenuButton selectedButton)
    {
        ItemPrice selectedPrice = null;

       Dictionary<string, MenuButton>.Enumerator _enumerator = merchandiseToButtonDictionary.GetEnumerator();
        while (_enumerator.MoveNext())
        {
            if(_enumerator.Current.Value == selectedButton)
            {
                selectedPrice = m_Prices.Find(m => m.name == _enumerator.Current.Key);
            }
        }

        if (selectedPrice == null)
            return;


        if (!activatingPlayer.AttemptCharge(selectedPrice.Cost))
        {
            return;
        }

        GameObject genObj = GameObject.Find("Generated Objects");

        selectedPrice.gameObject.SetActive(true);
        selectedPrice.gameObject.transform.position = m_Transform.TransformPoint(spawnOffset);
        selectedPrice.gameObject.transform.SetParent(genObj == null ? null : genObj.transform);

        m_Prices.Remove(selectedPrice);
        merchandiseToButtonDictionary.Remove(selectedPrice.name);

        Destroy(selectedButton.gameObject);

        if (shouldRestock)
        {
            Restock();
        }
    }


    private void RemoveAllPrices()
    {
        while (m_Prices.Count > 0)
        {
            RemovePrice(m_Prices[0]);
        }
    }
    private void RemovePrice(ItemPrice _price)
    {
        if (!m_Prices.Any(m => m.name == _price.name))
            return;



        throw new NotImplementedException();
    }

   
}
