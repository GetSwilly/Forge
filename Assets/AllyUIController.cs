using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllyUIController : MonoBehaviour {

    [SerializeField]
    GameObject m_UIPrefab;

    [SerializeField]
    Transform contentParent;

    Dictionary<GameObject, AllyUI> uiSet = new Dictionary<GameObject, AllyUI>();


    public void AddAllyUI(GameObject obj)
    {
        if (uiSet.ContainsKey(obj))
        {
            return;
        }

        GameObject panel = GameObject.Instantiate(m_UIPrefab);
        panel.transform.SetParent(contentParent);

        AllyUI ui = panel.GetComponent<AllyUI>();

        IIdentifier identifier = obj.GetComponent<IIdentifier>();
        ui.text.text = identifier != null ? identifier.Name : "NULL";

        Health health = obj.GetComponent<Health>();
        health.OnHealthChange += UpdateHealth;
        health.OnKilled += RemoveAllyUI;
        
        uiSet.Add(obj, ui);

        panel.SetActive(true);
        panel.transform.localScale = Vector3.one;
        UpdateHealth(health);
    }
    public void RemoveAllyUI(Health health)
    {
        RemoveAllyUI(health.gameObject);
    }
    public void RemoveAllyUI(GameObject obj)
    {
        if (!uiSet.ContainsKey(obj))
        {
            return;
        }

        AllyUI ui = uiSet[obj];
        uiSet.Remove(obj);

        Destroy(ui.gameObject);
    }
    public void RemoveAll()
    {
        Dictionary<GameObject, AllyUI>.KeyCollection.Enumerator enumerator = uiSet.Keys.GetEnumerator();

        while (enumerator.MoveNext())
        {
            RemoveAllyUI(enumerator.Current);
        }
    }


    void UpdateHealth(Health health)
    {
        if (health == null || !uiSet.ContainsKey(health.gameObject))
        {
            return;
        }

        AllyUI ui = uiSet[health.gameObject];
        ui.progressBar.SetPercentage(health.HealthPercentage);
    }
}
