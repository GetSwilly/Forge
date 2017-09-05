using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ItemDrop : MonoBehaviour
{

	public enum ItemType { Health, Experience };
	[SerializeField] ItemType myType;

	[SerializeField] int amount;

	public ItemType Type
    {
		get { return myType; }
	}
    public int Amount
    {
        get { return amount; }
    }



    void OnCollisionEnter(Collision coll)
    {

        switch (myType)
        {
            case ItemType.Health:
                Health _health = coll.gameObject.GetComponent<Health>();

                if (_health != null && _health.NeedsHealth)
                {
                    _health.HealthArithmetic(amount, false, transform);

                    GameObject _info = ObjectPoolerManager.Instance.DynamicInfoPooler.GetPooledObject();
                    _info.transform.position = transform.position;

                    DynamicInfoScript _infoScript = _info.GetComponent<DynamicInfoScript>();
                    //_infoScript.infoText.text = Mathf.Abs(amount).ToString();
                    //_infoScript.infoColor = Color.red;

                    _info.SetActive(true);
                    _infoScript.Initialize(amount, Color.red, true);




                    //gameObject.SetActive(false);
                    Destroy(gameObject);
                }

                break;
            case ItemType.Experience:
                PlayerController _controller = coll.gameObject.GetComponent<PlayerController>();

                if (_controller != null && _controller.ShouldCollectExp)
                {
                    _controller.ModifyExp(amount);

                    GameObject _info = ObjectPoolerManager.Instance.DynamicInfoPooler.GetPooledObject();
                    _info.transform.position = transform.position;

                    DynamicInfoScript _infoScript = _info.GetComponent<DynamicInfoScript>();

                    _info.SetActive(true);
                    _infoScript.Initialize(amount, Color.green, true);

                    //gameObject.SetActive(false);
                    Destroy(gameObject);
                }

                break;
        }

    }
}
