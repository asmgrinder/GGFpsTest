//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MinigunController : WeaponController
{
    public float FireRate = 0.05f;
    public GameObject bulletEffectPrefab;
    public Transform RayCastBase;

    float nextShotIn = 0;

    ObjectPool<GameObject> bulletEffectPool;

    // Start is called before the first frame update
    void Start()
    {
        int preloadCount = 400;
        int maxCount = 2 * preloadCount;
        bulletEffectPool = new ObjectPool<GameObject>(
                            () => { return Instantiate(bulletEffectPrefab); },
                            be => { be.SetActive(true); },
                            be => { be.SetActive(false); },
                            be => { Destroy(be); },
                            false,
                            preloadCount,
                            maxCount);
        if (null != bulletEffectPool)
        {
            GameObject[] instances = new GameObject[preloadCount];
            for (int i = 0; i < instances.Length; i++)
            {
                instances[i] = bulletEffectPool.Get();
            }
            foreach (GameObject instance in instances)
            {
                bulletEffectPool.Release(instance);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        onUpdate();
        if (nextShotIn > 0)
        {
            nextShotIn -= Time.deltaTime;
        }
    }

    public override void Fire()
    {
        onUpdate();
        if (nextShotIn <= 0
            && clip > 0)
        {   // shoot another bullet
            clip -= Mathf.Min(clip, 1);
            nextShotIn += FireRate;
            fireBullet();
        }
    }

    void releaseBulletEffect(BulletEffectController bulletEffect)
    {
        if (null != bulletEffectPool)
        {
            bulletEffectPool?.Release(bulletEffect.gameObject);
        }
    }

    void fireBullet()
    {
        if (Physics.Raycast(new Ray(RayCastBase.position + 0.05f * transform.forward, transform.forward), out RaycastHit hitInfo))
        {
            //Debug.Log("placing bullet mark");
            GameObject gameObj = bulletEffectPool.Get();
            BulletEffectController bec;
            Rigidbody rb;
            if (null != gameObj
                && null != (bec = gameObj.GetComponent<BulletEffectController>())
                && null != (rb = gameObj.GetComponent<Rigidbody>()))
            {
                bec.Init(releaseBulletEffect);
                DroidController dc = hitInfo.collider.GetComponent<DroidController>();
                if (null != dc)
                {
                    rb.velocity = dc.Speed * 2 * transform.forward;
                    rb.position = hitInfo.point + 0.1f * transform.forward;
                }
                else
                {
                    rb.position = hitInfo.point;
                }
            }
        }
    }
}
