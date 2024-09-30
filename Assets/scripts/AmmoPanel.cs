using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmmoPanel : MonoBehaviour
{
    public TMP_Text Text;
    public RectTransform ClipBar;

    Vector2 startSize;

    public void SetClipBar(float Perc)
    {
        ClipBar.sizeDelta += (Perc * startSize.y - ClipBar.sizeDelta.y) * Vector2.up;
    }

    // Start is called before the first frame update
    void Awake()
    {
        startSize = ClipBar.sizeDelta;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
