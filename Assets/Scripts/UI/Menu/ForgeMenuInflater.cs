using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ForgeSite))]
public class ForgeMenuInflater : MenuInflater
{
    ForgeSite m_Site;

    Dictionary<MenuButton, ForgeableObject> buttonToForgeDictionary;

    protected override void Awake()
    {
        base.Awake();

        m_Site = GetComponent<ForgeSite>();
    }

    void Start()
    {
        buttonToForgeDictionary = new Dictionary<MenuButton, ForgeableObject>();
    }

    //public override bool Interact(PlayerController _player)
    //{
    //    return base.Interact(_player);
    //}

    public override void DeflateMenu()
    {
        base.DeflateMenu();

        Dictionary<MenuButton, ForgeableObject>.Enumerator enumerator = buttonToForgeDictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            Destroy(enumerator.Current.Key.gameObject);
        }

        buttonToForgeDictionary.Clear();
    }

    protected override void AddButtons()
    {
        buttonToForgeDictionary.Clear();

        List<ForgeableObject> forgeableComponents = m_Site.Forgeables;

        forgeableComponents.ForEach(f =>
        {
            GameObject _buttonObject = Instantiate(buttonPrefab) as GameObject;
            MenuButton _button = _buttonObject.GetComponent<MenuButton>();

            ItemPrice _price = f.gameObject.GetComponent<ItemPrice>();

            _button.Initialize(f.Name, _price.CreditValue.ToString());
            _button.OnActionMain += AttemptForge;

           
            m_Menu.AddButton(_buttonObject);

            buttonToForgeDictionary.Add(_button, f);
        });
    }

    private void AttemptForge(MenuButton selectedButton)
    {
        if (!buttonToForgeDictionary.ContainsKey(selectedButton))
            return;

        ForgeableObject forgeable = buttonToForgeDictionary[selectedButton];

        ItemPrice price =  forgeable.gameObject.GetComponent<ItemPrice>();
        if (!activatingPlayer.CreditArithmetic(-price.CreditValue))
        {
            return;
        }

        GameObject g = Instantiate(forgeable.gameObject) as GameObject;
        m_Site.Forge(activatingPlayer, g.GetComponent<ForgeableObject>(), activatingPlayer.GetComponent<Team>());

        DeflateMenu();
    }

    protected override bool CanInflateMenu()
    {
        return base.CanInflateMenu() && m_Site.CanRemoveActive();
    }
}
