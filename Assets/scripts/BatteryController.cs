using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryController : CollectableController
{
    // Start is called before the first frame update
    void Start()
    {
        initialize();
    }

    // Update is called once per frame
    void Update()
    {
        onUpdate();
    }
}
