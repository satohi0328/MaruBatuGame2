using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScript : MonoBehaviour {

    public float titleDownSpeed; // タイトルが降りてくるスピード

    private GameObject startButton;
    private float buttonPosY;

    // Use this for initialization
    void Start() {
        var t = GameObject.Find("Canvas").GetComponent<RectTransform>();
        this.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, t.sizeDelta.y /2  + 100, 0);
        startButton = GameObject.Find("GameStartButton");

        // タイトルの最終ポジションの設定。　スタートボタンのちょっと上
        buttonPosY = startButton.GetComponent<RectTransform>().anchoredPosition.y + startButton.GetComponent<RectTransform>().rect.height + 100;

    }

    // Update is called once per frame
    void Update() {
        if (this.GetComponent<RectTransform>().anchoredPosition.y > buttonPosY) {
            this.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -titleDownSpeed);

        }


    }
}
