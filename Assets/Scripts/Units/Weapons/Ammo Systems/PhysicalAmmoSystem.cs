using UnityEngine;
using System;
using System.Collections;

public class PhysicalAmmoSystem : AmmoSystem {

    [SerializeField]
    protected int maxClipAmmo;

    protected int currentClipAmmo;

    [SerializeField]
    bool infiniteAmmo = true;


	public override void UseAmmo(float delta)
    {
        if (CurrentClipAmmo - delta >= 0)
		{
            CurrentClipAmmo -= (int)delta;
			
		}

		ActivateAlert(GetAmmoPercentage());
	}
	public override void AddAmmo(float delta)
    {
		int toAdd = (int)delta;
		int clipNeed = MaxClipAmmo - CurrentClipAmmo;

		if(toAdd >= clipNeed)
        {
			toAdd -= clipNeed;
			CurrentClipAmmo = MaxClipAmmo;

            CurrentAmmo += toAdd;
		}
        else
        {
			CurrentClipAmmo += toAdd;
		}

		ActivateAlert(GetAmmoPercentage());
	}
	public override void MaxOutAmmo()
    {
        CurrentAmmo = MaxAmmo;
		CurrentClipAmmo = MaxClipAmmo;
		
		ActivateAlert(GetAmmoPercentage());
	}




	public override bool CanReload()
    {
		return CurrentAmmo > 0 && CurrentClipAmmo < MaxClipAmmo;
	}
	public override void Reload()
    {
		if(CurrentAmmo <= 0)
			return;

		int neededAmmo = MaxClipAmmo - CurrentClipAmmo;
		
		if(neededAmmo < 0)
        {
			Debug.Log("Need negative ammo");
			return;
		}
		
		int transferAmmo = CurrentAmmo >=  neededAmmo ? neededAmmo : (int)CurrentAmmo;
        CurrentClipAmmo += transferAmmo;

        if (!infiniteAmmo)
        {
            CurrentAmmo -= transferAmmo;
        }

		ActivateAlert(GetAmmoPercentage());
	}


	

	
	public override bool CanFire(float delta)
	{
        return (CurrentClipAmmo - delta) >= 0;
	}

	public override float GetAmmoPercentage()
    {
		return CurrentClipAmmo / (float)MaxClipAmmo;
	}


    #region Accessors

    public int MaxClipAmmo
    {
        get { return maxClipAmmo; }
        private set { maxClipAmmo = Mathf.Clamp(value, 0, value); }
    }
    public int CurrentClipAmmo
    {
        get { return currentClipAmmo; }
        private set { currentClipAmmo = Mathf.Clamp(value, 0, MaxClipAmmo); }
    }

    #endregion

    protected override void OnValidate()
    {
        base.OnValidate();

        MaxClipAmmo = MaxClipAmmo;
    }
}
