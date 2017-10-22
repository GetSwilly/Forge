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

            _button.OnButtonClicked += AttemptForge;
            m_Menu.AddButton(_buttonObject);
            
            _button.Initialize(m_Menu, null, f.Name);

            ItemPrice _price = f.gameObject.GetComponent<ItemPrice>();

            _button.SecondaryText = "Credits: " + _price.Value;

            buttonToForgeDictionary.Add(_button, f);
        });
    }

    private void AttemptForge(MenuButton selectedButton)
    {
        if (!buttonToForgeDictionary.ContainsKey(selectedButton))
            return;

        ForgeableObject forgeable = buttonToForgeDictionary[selectedButton];

        ItemPrice price =  forgeable.gameObject.GetComponent<ItemPrice>();
        if (!activatingPlayer.Charge(-price.Value))
        {
            return;
        }

        GameObject g = Instantiate(forgeable.gameObject) as GameObject;
        m_Site.Forge(g.GetComponent<ForgeableObject>(), activatingPlayer.GetComponent<Team>());

        DeflateMenu();
    }

    protected override bool CanInflateMenu()
    {
        return base.CanInflateMenu() && m_Site.CanRemoveActive();
    }
}
