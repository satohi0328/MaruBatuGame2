using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class CenterPanelScript : MonoBehaviour {
    private bool gameSetFlg = false; //ゲームセットフラグ
    public float speed = 0.1f; //拡大スピード

    public GameObject gobackTitleButton; //タイトルへボタン

    // Use this for initialization
    void Start() {
        this.gameObject.transform.localScale = new Vector3(0, 0, 0);
        gobackTitleButton.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        if (gameSetFlg) {
            //スケールが１になるまで
            if (this.gameObject.transform.localScale.x < 1) {
                this.gameObject.transform.localScale += new Vector3(speed, speed, speed);

            } else {
                //表示完了後、タイトルへボタンを表示
                gobackTitleButton.SetActive(true);
            }
        }
    }


    public void OnClickGoBackTitleButton() {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();

        SceneManager.LoadScene("MenuScene");
    }


    public void SetGameSetFlg(bool flg) {
        this.gameSetFlg = flg;
    }
}
