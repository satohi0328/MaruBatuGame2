﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartButtonScript : MonoBehaviour {

	private bool onclickbutton = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (onclickbutton) {
			SceneManager.LoadScene("GameScene");

		}


	}


    public void OnClickedButton() {
		onclickbutton = true;
	}
}
