using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float LoadSceneDelay = 3;
    public TMP_Text Message;

    public static GameManager Instance => instance;

    bool levelEnded = false;

    static GameManager instance;

    // Start is called before the first frame update
    void Start()
    {
        if (null == instance)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (false == levelEnded)
        {
            if (null != PlayerController.Instance
                && 0 == PlayerController.Instance.Health)
            {
                levelEnded = true;
                // reload level (SceneManager.sceneCountInBuildSettings)
                int index = SceneManager.GetActiveScene().buildIndex;
                StartCoroutine(showTextAndLoadScene("Defeat", index));
            }
            DroidController[] droids = FindObjectsOfType<DroidController>();
            if (null == droids
                || 0 == droids.Length)
            {
                levelEnded = true;
                int index = (1 + SceneManager.GetActiveScene().buildIndex) % SceneManager.sceneCountInBuildSettings;
                PlayerController.Instance.SaveState();
                StartCoroutine(showTextAndLoadScene("Victory", index));
            }
        }
    }

    IEnumerator showTextAndLoadScene(string Msg, int SceneIndex)
    {
        if (null != PlayerController.Instance)
        {
            PlayerController.Instance.Frozen = true;
        }
        Message.text = Msg;
        Message.gameObject.SetActive(true);
        yield return new WaitForSeconds(LoadSceneDelay);
        SceneManager.LoadScene(SceneIndex);
    }
}
