﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class backToMenuButton : MonoBehaviour {

    void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(changeOnClick);
    }

    void changeOnClick()
    {
        GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
        gc.startLoadingLevel("Main");
    }
}