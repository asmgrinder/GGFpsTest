using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public float Ammo;
    public float MaxAmmo;
    public float ClipSize = 12;
    public float ReloadTime = 0.5f;

    public float Clip => clip;

    protected float clip = 0;
    float reloadTimer = 0;

    public virtual void Fire()
    {

    }

    public void onUpdate()
    {
        if (reloadTimer > 0)
        {
            reloadTimer -= Mathf.Min(reloadTimer, Time.deltaTime);
            if (0 == reloadTimer)
            {
                float amount = Mathf.Min(Ammo, ClipSize);
                Ammo -= amount;
                clip += amount;
            }
        }
        if (0 == clip
            && Ammo > 0
            && 0 == reloadTimer)
        {
            reloadTimer = ReloadTime;
        }
    }
}
