using UnityEngine;
using System;
using System.Collections;

public class EnergyAmmoSystem : AmmoSystem {

    [SerializeField]
    protected float passiveReloadRate;
	
	

	
	void Update ()
    {
		if(CurrentAmmo < MaxAmmo)
        {
			float delta = passiveReloadRate * Time.deltaTime;
			AddAmmo(delta);
		}
	}



    public override void UseAmmo(float delta)
    {
        if (CurrentAmmo - (delta * Time.deltaTime) >= 0)
        {
            CurrentAmmo -= delta * Time.deltaTime;
        }

        ActivateAlert(GetAmmoPercentage());
    }
    public override void AddAmmo(float delta)
    {
        CurrentAmmo += delta;

        ActivateAlert(GetAmmoPercentage());
    }
	public override void MaxOutAmmo()
    {
		AddAmmo(MaxAmmo);
	}


	public override bool CanReload()
    {
		return false;
	}
	public override void Reload()
    {
		float delta = 0;//activeReloadRate * Time.deltaTime;
		AddAmmo(delta);
	}
	

	
	public override bool CanFire(float delta)
    {
        return (CurrentAmmo - (delta * Time.deltaTime)) >= 0;
	}
	public override float GetAmmoPercentage()
    {
		return CurrentAmmo / MaxAmmo;
	}
}
