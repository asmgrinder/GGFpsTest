using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (null != PlayerController.Instance)
        {
            transform.LookAt(PlayerController.Instance.transform.position);
        }
    }
}
