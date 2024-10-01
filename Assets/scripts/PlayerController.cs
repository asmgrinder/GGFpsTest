using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using static PlayerController;

public class PlayerController : MonoBehaviour
{
    public float RotSpeed = 60;
    public float RotLerp = 0.8f;
    public float MoveAcc = 5;
    public float JumpAcc = 10;

    public bool InvertY = true;

    public float NextJumpPause = 2;

    public Transform Head;

    public WeaponController[] Weapons;
    public AmmoPanel[] AmmoPanels;

    public RectTransform HealthBar;

    public float DamageFromLaser = 0.1f;
    public float DamageFromAcid = 0.01f;

    [Range(1, 10)] public float ZoomK = 4;

    public bool Frozen = false;
    public float Health => health;

    public static PlayerController Instance => instance;

    enum WeaponType { Blaster = 0, Minigun };

    bool[] weaponsMask;

    Rigidbody rb;

    float rotV = 0;
    float rotH = 0;

    float jumpTimer = 0;
    float startY = 0;
    float health = 100;
    Vector2 healthSize;

    float startFov;
    float rotSpeed;

    static PlayerController instance;

    //const string healthStr = "health";
    //const string weaponIndexStr = "weaponIndex";
    //const string maskStr = "mask";
    //const string ammoStr = "ammo";


    [Serializable]
    public class SaveInfo
    {
        public float Health;
        public int Selected;
        public bool[] WeaponMasks;
        public float[] Ammo;
    }


    // Start is called before the first frame update
    void Start()
    {
        if (null == instance)
        {
            instance = this;
        }
        startFov = Camera.main.fieldOfView;
        rotSpeed = RotSpeed;
        startY = transform.position.y;
        weaponsMask = new bool[Weapons.Length];
        rb = GetComponent<Rigidbody>();
        healthSize = HealthBar.sizeDelta;

        RestoreState();
        updateHealthBar();
        updateAmmoText();
    }

    void Update()
    {
        if (false == Frozen)
        {
            KeyCode[] keys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2 };
            int index = Array.FindIndex(keys, key => Input.GetKey(key));
            selectWeapon(index);

            if (Input.GetMouseButton(0))
            {
                int activeIndex = Array.FindIndex(Weapons, weapon => weapon.gameObject.activeSelf);
                WeaponController wc = (activeIndex >= 0) ? Weapons[activeIndex] : null;
                if (null != wc)
                {
                    wc.Fire();
                }
            }
        }

        bool zoomed = Input.GetMouseButton(1);
        Camera.main.fieldOfView = startFov / (zoomed ? ZoomK : 1);
        rotSpeed = RotSpeed / (zoomed ? ZoomK : 1);

        updateAmmoText();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.angularVelocity = Vector3.zero;
        jumpTimer -= Mathf.Min(jumpTimer, Time.deltaTime);
        
        // process rotation
        float horz = Input.GetAxis("Mouse X");
        float vert = Input.GetAxis("Mouse Y");

        rotH += horz * rotSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.AngleAxis(rotH, Vector3.up);

        rotV += vert * rotSpeed * Time.deltaTime * (InvertY ? -1 : 0);
        rotV = Mathf.Clamp(rotV, -45, 40);
        Head.transform.localRotation = Quaternion.AngleAxis(rotV, Vector3.right);
        
        // forbid movement on end of round
        if (false == Frozen)
        {

            bool onGround = Physics.Raycast(transform.position, -Vector3.up, startY + 0.3f);
            if (onGround)
            {
                Vector3 newVel = Vector3.zero;
                if (Input.GetKey(KeyCode.A)
                    || Input.GetKey(KeyCode.LeftArrow))
                {
                    newVel -= transform.right;
                }
                if (Input.GetKey(KeyCode.D)
                    || Input.GetKey(KeyCode.RightArrow))
                {
                    newVel += transform.right;
                }
                if (Input.GetKey(KeyCode.W)
                    || Input.GetKey(KeyCode.UpArrow))
                {
                    newVel += transform.forward;
                }
                if (Input.GetKey(KeyCode.S)
                    || Input.GetKey(KeyCode.DownArrow))
                {
                    newVel -= transform.forward;
                }
                newVel = MoveAcc * (newVel.x * Vector3.right + newVel.z * Vector3.forward).normalized - rb.velocity;
                if (newVel.sqrMagnitude > 0.01f)
                {
                    newVel.y = 0;
                    rb.AddForce(newVel, ForceMode.Impulse);
                }

                if (0 == jumpTimer
                    && Input.GetKey(KeyCode.Space))
                {
                    rb.AddForce(JumpAcc * Vector3.up, ForceMode.VelocityChange);
                    jumpTimer = NextJumpPause;
                }
            }
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    string getSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "GameData.json");
    }

    public void SaveState()
    {
        string dataFilePath = getSavePath();
        Debug.Log(dataFilePath);
        try
        {
            using (StreamWriter writer = new(dataFilePath))
            {
                SaveInfo si = new()
                {
                    Health = health,
                    Selected = Array.FindIndex(Weapons, weapon => weapon.gameObject.activeSelf),
                    WeaponMasks = new bool[weaponsMask.Length],
                    Ammo = new float[Weapons.Length]
                };
                for (int i = 0; i < weaponsMask.Length; i++)
                {
                    si.WeaponMasks[i] = weaponsMask[i];
                }
                for (int i = 0; i < Weapons.Length; i++)
                {
                    si.Ammo[i] = Weapons[i].Ammo + Weapons[i].Clip;
                }

                string dataToWrite = JsonUtility.ToJson(si);
                writer.Write(dataToWrite);
                Debug.Log("saved: " + dataToWrite);
            }
        }
        catch (Exception)
        {
            Debug.Log("Save failed");
        }

        //for (int i = 0; i < weaponsMask.Length; i++)
        //{
        //    PlayerPrefs.SetInt(maskStr + i.ToString(), weaponsMask[i] ? 1 : 0);
        //    PlayerPrefs.SetFloat(ammoStr + i.ToString(), Weapons[i].Ammo + Weapons[i].Clip);
        //}
        //PlayerPrefs.SetFloat(healthStr, health);
        //int selected = Array.FindIndex(Weapons, weapon => weapon.gameObject.activeSelf);
        //PlayerPrefs.SetInt(weaponIndexStr, selected);
    }

    public void RestoreState()
    {
        string dataFilePath = getSavePath();
        Debug.Log(dataFilePath);
        try
        {
            using (StreamReader reader = new(dataFilePath))
            {
                string dataToLoad = reader.ReadToEnd();

                Debug.Log("loaded: " + dataToLoad);

                //JsonUtility.FromJsonOverwrite(dataToLoad, saveInfo);
                SaveInfo si = JsonUtility.FromJson<SaveInfo>(dataToLoad);

                if (si.WeaponMasks.Length == weaponsMask.Length
                    && si.Ammo.Length == Weapons.Length)
                {
                    health = si.Health;
                    for (int i = 0; i < weaponsMask.Length; i++)
                    {
                        weaponsMask[i] = si.WeaponMasks[i];
                    }

                    for (int i = 0; i < Weapons.Length; i++)
                    {
                        Weapons[i].Ammo = si.Ammo[i];
                    }

                    selectWeapon(si.Selected);
                }
                //for (int i = 0; i < weaponsMask.Length; i++)
                //{
                //    weaponsMask[i] = 0 != PlayerPrefs.GetInt(maskStr + i.ToString(), 0);
                //    Weapons[i].Ammo = PlayerPrefs.GetFloat(ammoStr + i.ToString(), 0);
                //}
                //health = PlayerPrefs.GetFloat(healthStr, 100);
                //int selected = PlayerPrefs.GetInt(weaponIndexStr, -1);
                //if (selected > 0)
                //{
                //    selectWeapon(selected);
                //}
            }
        }
        catch (FileNotFoundException e)
        {
            Debug.Log("Load: " + e.Message);
        }
    }

    void updateAmmoText()
    {
        if (Weapons.Length == AmmoPanels.Length)
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                AmmoPanels[i].Text.text = weaponsMask[i] ? ((int)Weapons[i].Ammo).ToString() : "";    // "#.0"
                AmmoPanels[i].SetClipBar(Weapons[i].Clip / Weapons[i].ClipSize);
            }
        }
    }

    void updateHealthBar()
    {
        HealthBar.sizeDelta += (healthSize.x * health * 0.01f - HealthBar.sizeDelta.x) * Vector2.right;
    }

    public void TakeHit(float Strength)
    {
        health -= Mathf.Min(health, Strength);
        updateHealthBar();
    }

    void handleMediKit(CollectableController CC)
    {
        MediKitController mkc;
        if (health > 0
            && health < 100
            && null != (mkc = CC as MediKitController))
        {
            health = Mathf.Min(100, health + mkc.Amount);
            updateHealthBar();

            CC.Respawn();
        }
    }

    void handleBattery(CollectableController CC)
    {
        BatteryController bc;
        int wi = (int)WeaponType.Blaster;
        if (Weapons.Length > wi
            && Weapons[wi].Ammo + Weapons[wi].Clip < Weapons[wi].MaxAmmo
            && null != (bc = CC as BatteryController))
        {
            Weapons[wi].Ammo = Mathf.Min(Weapons[wi].MaxAmmo - Weapons[wi].Clip, Weapons[wi].Ammo + bc.Amount);

            CC.Respawn();
        }
    }

    void handleBullets(CollectableController CC)
    {
        BulletController bc;
        int wi = (int)WeaponType.Minigun;
        if (Weapons.Length > wi
            && Weapons[wi].Ammo < Weapons[wi].MaxAmmo
            && null != (bc = CC as BulletController))
        {
            Weapons[wi].Ammo = Mathf.Min(Weapons[wi].MaxAmmo - Weapons[wi].Clip, Weapons[wi].Ammo + bc.Amount);

            CC.Respawn();
        }
    }

    void handleBlasterItem(CollectableController CC)
    {
        BlasterItemController bic;
        int wi = (int)WeaponType.Blaster;
        if (Weapons.Length > wi
            && Weapons[wi].Ammo < Weapons[wi].MaxAmmo
            && !weaponsMask[wi]
            && null != (bic = CC as BlasterItemController))
        {
            weaponsMask[wi] = true;
            Weapons[wi].Ammo = bic.Amount;
            selectWeapon(wi);

            CC.Respawn();
        }
    }

    void handleMinigunItem(CollectableController CC)
    {
        MinigunItemController bic;
        int wi = (int)WeaponType.Minigun;
        if (Weapons.Length > wi
            && Weapons[wi].Ammo < Weapons[wi].MaxAmmo
            && !weaponsMask[wi]
            && null != (bic = CC as MinigunItemController))
        {
            weaponsMask[wi] = true;
            Weapons[wi].Ammo = bic.Amount;
            selectWeapon(wi);

            CC.Respawn();
        }
    }


    private void OnTriggerStay(Collider other)
    {
        CollectableController cc;
        if (other.CompareTag("collectable")
            && null != (cc = other.GetComponent<CollectableController>()))
        {
            handleMediKit(cc);
            handleBattery(cc);
            handleBullets(cc);
            handleBlasterItem(cc);
            handleMinigunItem(cc);
        }
        if (other.CompareTag("droid laser"))
        {
            TakeHit(DamageFromLaser);
        }
        if (other.CompareTag("acid"))
        {
            TakeHit(DamageFromAcid);
        }
    }

    void selectWeapon(int Index)
    {
        if (Index >= 0
            && Index < Weapons.Length
            && weaponsMask[Index])
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                Weapons[i].gameObject.SetActive(i == Index);
            }
        }
    }
}
