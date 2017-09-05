using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Merchant : MonoBehaviour {//, IInteractable {
    /*

    [SerializeField]
    List<MenuItem> products = new List<MenuItem>();


    [SerializeField]
    Vector3 followOffset = Vector3.zero;


    [SerializeField]
    [Range(0f,90f)]
    float launchAngle = 60f;

    [SerializeField]
    float launchPower = 10f;

    [SerializeField]
    Vector3 launchOrigin = new Vector3(0, 3f, 0);

    [SerializeField]
    bool isMerchantOpen = true;


    PlayerController activatingController;
    GameObject menuObject;

    [SerializeField]
    Vector3 uiOffset = Vector3.zero;
  
    public void MenuItemClicked(MenuItem _item)
    {
        float experienceCost = 0;
        float healthCost = 0;

        for(int i = 0; i < _item.Costs.Count; i++)
        {
            switch (_item.Costs[i].ObjectA)
            {
                case AttributeType.Experience:
                    experienceCost += _item.Costs[i].ObjectB;   // activatingController.CanModifyExp(-(int)_item.Cost);
                    break;
                case AttributeType.Health:
                    healthCost += _item.Costs[i].ObjectB;    // activatingController.GetComponent<Health>().CurHealth > _item.Cost;
                    break;
                default:
                    break;
            }
        }

        
        if (!(activatingController.CanModifyExp(-(int)experienceCost) & activatingController.GetComponent<Health>().CurHealth > healthCost))
            return;
        

        activatingController.ModifyExp(-(int)experienceCost);
        activatingController.GetComponent<Health>().HealthArithmetic(-healthCost, false, this.transform);
    

        DropObject(_item.Product);

        _item.Quantity--;

        UpdateMenuItem(_item.Product, _item);
    }


    public void UpdateMenuItem(GameObject oldProduct, MenuItem newItem)
    {
        for(int i = 0; i < products.Count; i++)
        {
            if(products[i].Product == oldProduct)
            {
                products[i] = newItem;
                break;
            }
        }

    }

    void DropObject(GameObject objPrefab)
    {
        Vector3 startPosition = transform.TransformPoint(launchOrigin);

        GameObject newObj = (GameObject)Instantiate(objPrefab, startPosition, Quaternion.identity);


        newObj.SetActive(true);

        
        Rigidbody _rigidbody = newObj.GetComponent<Rigidbody>();

        if (_rigidbody != null)
        {
            Vector3 forceVector = activatingController.transform.position - startPosition;
            forceVector.y = Mathf.Sin(launchAngle);
            

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(forceVector.normalized * launchPower);
        }
        
    }











    public bool Use(PlayerController player)
    {
        throw new NotImplementedException();
        //activatingController = player;
        //UserInput _input = player.GetComponent<UserInput>();
        //_input.isEnabled = false;


        //menuObject = ObjectPoolerManager.Instance.MerchantMenuPooler.GetPooledObject();
        //MerchantMenu _menu = menuObject.GetComponent<MerchantMenu>();
        //_menu.Initialze(this, producst, followOffset);
        //menuObject.SetActve(true);
    }
    public bool Give(PlayerController player)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled()
    {
        return isMerchantOpen;
    }

    public bool IsUsableOutsideFOV()
    {
        return false;
    }


    InteractableUIController myUI;
    public void InflateUI()
    {
        if (myUI == null)
        {
            myUI = ObjectPoolerManager.Instance.Interactable_UIPooler.GetPooledObject().GetComponent<InteractableUIController>();
        }

        myUI.Activate(transform, uiOffset, "Merchant");
        myUI.gameObject.SetActive(true);
    }
    public void DeflateUI()
    {
        if (myUI == null)
            return;

        myUI.gameObject.SetActive(false);
        myUI = null;
    }
    */
}





public class MenuItem
{
    public GameObject Product;
    public List<CustomTuple2<Attribute, float>> Costs;
    public int Quantity;
}

