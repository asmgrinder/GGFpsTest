using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlasterController : WeaponController
{
    public float AmmoCost = 2;
    public Transform LaserThread;
    public float FireTime = 0.5f;

    Vector3 startPos;
    Vector3 startScale;

    float fireTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (null != LaserThread)
        {
            startPos = LaserThread.transform.localPosition;
            startScale = LaserThread.transform.localScale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        onUpdate();
        if (fireTimer > 0)
        {
            fireTimer -= Mathf.Min(fireTimer, Time.deltaTime);
            if (0 == fireTimer)
            {
                LaserThread.transform.localPosition = startPos;
                LaserThread.transform.localScale = startScale;
            }
        }
    }

    public override void Fire()
    {
        if (null != LaserThread
            && clip > 0)    // Ammo > 0
        {
            Vector3 worldPos = LaserThread.parent.TransformPoint(startPos);
            Vector3 targetPoint = worldPos + transform.forward * 100;
            if (Physics.Raycast(new Ray(worldPos + 0.2f * transform.forward, transform.forward), out RaycastHit hitInfo, 100))
            {
                targetPoint = hitInfo.point + 0.2f * transform.forward;
            }
            LaserThread.position = 0.5f * (worldPos + targetPoint);
            Vector3 sc = LaserThread.localScale;
            sc.y = 0.5f * (targetPoint - worldPos).magnitude;
            LaserThread.localScale = sc;

            clip -= Mathf.Min(clip, AmmoCost * Time.deltaTime);

            fireTimer = FireTime;
        }
    }
}
