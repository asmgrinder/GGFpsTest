using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediKitController : CollectableController
{
    //[Range(0, 100)] public float HealingStrength = 15;

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
