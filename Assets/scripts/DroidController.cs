using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroidController : MonoBehaviour
{
    public float Speed = 5;
    public float GuardRadius = 2;
    public Vector3[] Route;
    public Transform[] ArmPoints;

    [Range(0.01f, 0.5f)] public float LaserInertia = 0.125f;

    public MeshRenderer HPBar;

    public float DamageFromLaser = 0.5f;
    public float DamageFromMinigun = 5;

    Vector3[] armPos;
    Vector3[] armScale;

    int RouteIndex = 0;

    float laserTimer;

    float Health = 100;

    // Start is called before the first frame update
    void Start()
    {
        float minDist2 = 0;
        int minIndex = -1;
        for (int i = 0; i < Route.Length; i++)
        {
            float dist2 = (transform.position - Route[i]).sqrMagnitude;
            if (minIndex < 0
                || dist2 < minDist2)
            {
                minIndex = i;
                minDist2 = dist2;
            }
        }
        //Debug.Log(gameObject.name + ": " + minIndex.ToString());
        if (minIndex >= 0)
        {
            RouteIndex = (minIndex + 1) % Route.Length;
        }

        armPos = new Vector3[ArmPoints.Length];
        armScale = new Vector3[ArmPoints.Length];
        for (int i = 0; i < ArmPoints.Length; i++)
        {
            armPos[i] = ArmPoints[i].localPosition;
            armScale[i] = ArmPoints[i].localScale;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        updateHealthBar();
    }

    void updateHealthBar()
    {
        if (null != HPBar.material)
        {
            HPBar.material.SetFloat("_HP", Health / 100f);
        }
    }

    void move()
    {
        // move droid
        Vector3 dir = Route[RouteIndex] - transform.position;
        float dist = Speed * Time.deltaTime;
        if (dist > dir.magnitude)
        {
            transform.position += dir;
            dist -= dir.magnitude;
            // next point
            RouteIndex++;
            RouteIndex %= Route.Length;
            // update direction
            dir = Route[RouteIndex] - transform.position;
        }
        transform.position += dist * dir.normalized;
        transform.LookAt(transform.position + dir.normalized);
    }

    void processFireAtPlayer()
    {
        // handle player
        if (null != PlayerController.Instance
            && PlayerController.Instance.Health > 0)
        {
            Vector3 playerPos = PlayerController.Instance.transform.position;
            Vector3 playerDir = playerPos - transform.position;
            if (playerDir.sqrMagnitude < GuardRadius * GuardRadius)
            {
                transform.LookAt(playerPos);

                for (int i = 0; i < ArmPoints.Length; i++)
                {
                    Vector3 worldPos = ArmPoints[i].parent.TransformPoint(armPos[i]);

                    if (Physics.Raycast(new Ray(worldPos + 0.2f * transform.forward, transform.forward), out RaycastHit hitInfo, 100))
                    {
                        float length = (hitInfo.point + 0.2f * transform.forward - worldPos).magnitude;
                        ArmPoints[i].position = worldPos + 0.5f * length * transform.forward;
                        ArmPoints[i].localScale += (0.5f * length - ArmPoints[i].localScale.y) * Vector3.up;

                        laserTimer = LaserInertia;
                    }
                }
            }
        }
    }

    void turnOffLasers()
    {
        for (int i = 0; i < ArmPoints.Length; i++)
        {
            ArmPoints[i].localPosition = armPos[i];
            ArmPoints[i].localScale = armScale[i];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bullet effect"))
        {
            takeHit(DamageFromMinigun);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("player laser"))
        {
            takeHit(DamageFromLaser);
        }
    }

    void takeHit(float Damage)
    {
        Health -= Mathf.Min(Health, Damage);
        updateHealthBar();
        if (0 == Health)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        move();
        processFireAtPlayer();
        if (laserTimer > 0)
        {
            laserTimer -= Mathf.Min(laserTimer, Time.deltaTime);
            if (0 == laserTimer)
            {
                turnOffLasers();
            }
        }
    }
}
