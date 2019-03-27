using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSet : MonoBehaviour
{
    public Text level;
    int iLevel;
    // Start is called before the first frame update
    void Start()
    {
        iLevel = 1;
    }

    // Update is called once per frame
    void Update()
    {
        level.text = iLevel.ToString();
    }

    public void levelIncrement()
    {
        iLevel += 1;
    }
}
