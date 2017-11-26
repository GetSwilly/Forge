using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectInflater : MenuInflater
{

    [SerializeField]
    Vector3 spawnOffset = Vector3.zero;

    [SerializeField]
    List<GameObject> characterPrefabs = new List<GameObject>();


    List<MenuButton> m_Buttons = new List<MenuButton>();


    protected override void AddButtons()
    {
        m_Buttons.Clear();


        for (int i = 0; i < characterPrefabs.Count; i++)
        {

            if (characterPrefabs[i] == null)
                continue;


            GameObject _object = Instantiate(buttonPrefab) as GameObject;
            MenuButton _button = _object.GetComponent<MenuButton>();
            CharacterMenuButton _characterButton = _button as CharacterMenuButton;


            if (_button == null || _characterButton == null)
            {
                Destroy(_object);
                continue;
            }
            

            m_Menu.AddButton(_object);

            _object.transform.localScale = Vector3.one;
            _object.transform.localPosition = Vector3.zero;

            _button.OnActionMain += CharacterSelected;
            

            m_Buttons.Add(_button);



            _button.Initialize(characterPrefabs[i].GetComponent<PlayerController>().Name);



            if (activatingPlayer.Equals(characterPrefabs[i]))
            {
                _button.Deactivate();
            }
        }



        m_Menu.OnAccept += CharacterSelected;
        m_Menu.OnCancel += DeflateMenu;
        

        //TODO -- Select the current character
    }


    private void CharacterSelected(MenuButton selectedButton)
    {
        for(int i = 0; i < m_Buttons.Count; i++)
        {
            if (m_Buttons[i].Equals(selectedButton))
            {
                if (activatingPlayer.Equals(characterPrefabs[i].GetComponent<PlayerController>()))
                    break;



                GameObject newPlayerObject = Instantiate(characterPrefabs[i]) as GameObject;
                PlayerController newController = newPlayerObject.GetComponent<PlayerController>();
                    newPlayerObject.SetActive(true);

                

                if (activatingPlayer == null)
                {
                    newPlayerObject.transform.position = transform.TransformPoint(spawnOffset);
                    newPlayerObject.transform.rotation = Quaternion.identity;
                }
                else
                {
                    if (activatingPlayer.Equals(newController))
                    {
                        Destroy(newPlayerObject);
                        continue;
                    }
                    
                    newController.Copy(activatingPlayer);
                }

                if (activatingPlayer != null)
                {
                    newPlayerObject.transform.position = activatingPlayer.transform.position;
                    newPlayerObject.transform.rotation = activatingPlayer.transform.rotation;

                    activatingPlayer.gameObject.SetActive(false);


                    Destroy(activatingPlayer.gameObject);
                }
                

               
                activatingPlayer = newController;
            }
        }


        DeflateMenu();
    }

}
