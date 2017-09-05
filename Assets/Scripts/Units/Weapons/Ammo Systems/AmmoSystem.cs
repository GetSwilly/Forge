using UnityEngine;
using System;
using System.Collections;

public abstract class AmmoSystem : MonoBehaviour {

    [SerializeField]
    float maxAmmo;

	float currentAmmo;


	
	public delegate void AlertAmmoChange(float _percent);
	public event AlertAmmoChange OnAmmoChanged;
	
	public virtual void OnEnable () {
		MaxOutAmmo();
	}
	
	public abstract void UseAmmo(float delta);

	public abstract void AddAmmo(float delta);

	public abstract void MaxOutAmmo();



	public abstract bool CanReload();

	public abstract void Reload();




    public abstract bool CanFire(float delta);

	public abstract float GetAmmoPercentage();

	protected void ActivateAlert(float _percent)
    {
		if(OnAmmoChanged != null)
			OnAmmoChanged(_percent);
	}

	#region Accessors
	
	public float MaxAmmo
    {
		get { return maxAmmo; }
		private set { maxAmmo = Mathf.Clamp(value, 0, value); } 
	}
    protected float CurrentAmmo
    {
        get { return currentAmmo; }
        set { currentAmmo = Mathf.Clamp(value, 0, MaxAmmo); }
    }
    /*
	public float AmmoPerFire { 
		get { return ammoPerFire; }
		set { ammoPerFire = value; }
	}*/

	#endregion


   protected virtual void OnValidate()
    {
        MaxAmmo = MaxAmmo;
    }
}
