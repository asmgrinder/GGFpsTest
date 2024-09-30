using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BulletEffectController : MonoBehaviour
{
    public float DisappearDelay = 0.25f;

    float disappearTimer = 0;

    Action<BulletEffectController> releaseAction;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (disappearTimer > 0)
        {
            disappearTimer -= Mathf.Min(disappearTimer, Time.deltaTime);
            if (0 == disappearTimer)
            {
                releaseAction(this);
            }
        }
    }

    public void Init(Action<BulletEffectController> ReleaseAction)
    {
        releaseAction = ReleaseAction;
        disappearTimer = DisappearDelay;
    }
}
