using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ForgeSite))]
public class ForgeMenuInflater : MenuInflater
{
    ForgeSite m_Site;

    Dictionary<MenuButton, IForgeable> buttonToForgeDictionary;

    protected override void Awake()
    {
        base.Awake();

        m_Site = GetComponent<ForgeSite>();
    }

    void Start()
    {
        buttonToForgeDictionary = new Dictionary<MenuButton, IForgeable>();
    }

    //public override bool Interact(PlayerController _player)
    //{
    //    return base.Interact(_player);
    //}


    protected override void AddButtons()
    {
        buttonToForgeDictionary.Clear();

        List<IForgeable> forgeableComponents = m_Site.Forgeables;

        forgeableComponents.ForEach(f =>
        {
            GameObject _buttonObject = Instantiate(buttonPrefab) as GameObject;
            MenuButton _button = _buttonObject.GetComponent<MenuButton>();

            _button.OnButtonClicked += AttemptForge;
            m_Menu.AddButton(_buttonObject);
            
            _button.Initialize(m_Menu, null, f.Name);

            buttonToForgeDictionary.Add(_button, f);
        });
    }

    private void AttemptForge(MenuButton selectedButton)
    {
        if (!buttonToForgeDictionary.ContainsKey(selectedButton))
            return;

        IForgeable forgeable = buttonToForgeDictionary[selectedButton];

        GameObject g = Instantiate(forgeable.GameObject) as GameObject;
        m_Site.Forge(g.GetComponent<IForgeable>(), activatingPlayer);

        DeflateMenu();
    }

    protected override bool CanInflateMenu()
    {
        return base.CanInflateMenu() && m_Site.CanRemoveActive();
    }
}
