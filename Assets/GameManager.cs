using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
    public GameObject panleObject;
    public GameObject maruObject;
    public GameObject[,] panelStatus = new GameObject[3, 3]; //生成するオブジェクトを保持

    private Vector3 moveTo;
    private GameObject moveObject; //Imageをタップしてドラッグするためのオブジェクト
    private GameObject tapPanelObject = null; //パネル上のオブジェクトをタップしてドラッグするためのオブジェクト
    private bool isDragObject = false; //ドラッグしているかフラグ
    private GameObject tapImageObject; //タップされたイメージを保持

    private bool beRay = false; //タップされた時のイベントフラグ
    private Camera camera;
    // Use this for initialization
    void Start() {
        camera = Camera.main;
        // 3×3のパネルを生成
        for (int i = 0; i < panelStatus.GetLength(0); i++) {
            for (int j = 0; j < panelStatus.GetLength(1); j++) {
                // パネル生成
                GameObject wkPanelObj = Instantiate(panleObject) as GameObject;

                panelStatus[i, j] = wkPanelObj;
                // マスごとに初期化
                panelStatus[i, j].GetComponent<PanelManager>().Init(i, j);

            }
        }
    }

    // Update is called once per frame
    void Update() {

        // 画面がタップされた場合
        if (Input.GetMouseButtonDown(0)) {
            RayCheck();
        }

        if (beRay) {
            //オブジェクトをドラッグ中の場合
            if (isDragObject) {
                MovePoisition();
            }
        }

        if (Input.GetMouseButtonUp(0) && beRay) {
            // 画面下ウィンドウの画像をタップした場合と、パネル上のオブジェクトをタップした場合で処理を分ける

            //パネルをタップした時のドラッグ中の場合
            if (tapPanelObject != null) {
                //パネル上のオブジェクトを再表示
                tapPanelObject.GetComponent<PanelManager>().ShowMaruBatuObject();

                //TODO 置けるか判定

                // パネル下に移動
                HideMaruBatuObject(tapPanelObject);
                // パネル上に表示されているまるばつオブジェクト配列を更新
                tapPanelObject.GetComponent<PanelManager>().AddOrderArray(moveObject.name);
                tapPanelObject.GetComponent<PanelManager>().RemoveOrderArray(moveObject.name);

                // 画面下のImageをタップした時のドラッグ中の場合
            }else if(tapImageObject != null) {

            }
  


            //オブジェクトをドラッグ中の場合
            if (isDragObject) {
                // 作業用パネルを生成
                GameObject wkPanel = null;
                //パネル上にあるか判定
                wkPanel = JudgeDetachObjectOnPanel();
                //判定の結果、パネル上にあった場合
                if (wkPanel != null) {
                    //TODOパネル上に置けるか判定

                    //パネル上に置く(表示)
                    ShowMaruBatuObject(wkPanel);
                    // パネル上に表示されているまるばつオブジェクト配列を更新
                    tapPanelObject.GetComponent<PanelManager>().AddOrderArray(moveObject.name);

                }
            }

            // ドラッグ終了処理
            AfterEndDragObject();
        }
    }

    private void RayCheck() {
        //パネルに配置されたオブジェクトがタップされた時の処理
        Ray ray = new Ray();
        RaycastHit hit = new RaycastHit();
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // パネル上のまるばつオブジェクトをタップした場合
        if (Physics.Raycast(ray, out hit, 100.0f) && hit.collider.gameObject.CompareTag("MaruObject")) {
            // どのパネルか取得
            for (int i = 0; i < panelStatus.GetLength(0); i++) {
                for (int j = 0; j < panelStatus.GetLength(1); j++) {
                    // パネル上にあった場合
                    if (panelStatus[i, j].GetComponent<PanelManager>().JudgeOnPanel(hit.collider.gameObject)) {
                        // パネルオブジェクトを一時保持。
                        tapPanelObject = panelStatus[i, j];
                        // パネル上のオブジェクトを一時非表示
                        tapPanelObject.GetComponent<PanelManager>().HideMaruBatuObject();
                        // タップ時にドラッグ中に表示するオブジェクト生成
                        moveObject = Instantiate(maruObject);
                        // タップされたオブジェクト名によって、ドラッグ中に表示するオブジェクト名を変更
                        moveObject.name = RenameMoveObjectName(hit.collider.gameObject.transform.root.name);                        // タップされたオブジェクトをもとに大きさ変更
                        if (hit.collider.gameObject.transform.root.name.Contains("Maru2") || hit.collider.gameObject.transform.root.name.Contains("Batu2")) {
                            moveObject.transform.localScale *= 2;
                        } else if (hit.collider.gameObject.transform.root.name.Contains("Maru3") || hit.collider.gameObject.transform.root.name.Contains("Batu3")) {
                            moveObject.transform.localScale *= 3;
                        }
                        //ドラッグ中
                        isDragObject = true;
                        beRay = true;
                    }
                }
            }
        } else {
            beRay = false;
        }

        // 画面下ウィンドウのImageがタップされた時の処理
        // タップされたUIを検出
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, result);
        foreach (RaycastResult raycastResult in result) {
            // タグがObjectImage(したパネルの画像)の場合
            if (raycastResult.gameObject.CompareTag("ObjectImage")) {
                // タップされたImageオブジェクトを保持(パネルにおけた場合、非表示にするため)
                tapImageObject = raycastResult.gameObject;
                // タップ時にオブジェクト生成
                moveObject = Instantiate(maruObject);
                // ドラッグ中に表示するオブジェクト名を変更
                moveObject.name = raycastResult.gameObject.name;
                // タップされたImageをもとに大きさ変更
                if (moveObject.name.Contains("mid")) {
                    moveObject.transform.localScale *= 2;
                } else if (moveObject.name.Contains("big")) {
                    moveObject.transform.localScale *= 3;
                }

                //ドラッグ中
                isDragObject = true;
                beRay = true;
            }
        }
    }

    //オブジェクトのドラッグ移動
    private void MovePoisition() {
        // 入力ポジション取得
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 30f;
        // ワールド座標に変換
        moveTo = Camera.main.ScreenToWorldPoint(mousePos);
        // y軸は3に固定
        moveTo.y = 3;
        // 移動
        moveObject.transform.position = moveTo;

    }

    //オブジェクトを離した際に、パネル上にあるか判定。あればパネルオブジェクトを返却
    private GameObject JudgeDetachObjectOnPanel() {
        // パネルを取得
        for (int i = 0; i < panelStatus.GetLength(0); i++) {
            for (int j = 0; j < panelStatus.GetLength(1); j++) {
                // パネル上にあった場合
                if (panelStatus[i, j].GetComponent<PanelManager>().JudgeOnPanel(moveObject)) {
                    // パネルを設定
                    return panelStatus[i, j];
                }
            }
        }
        return null;
    }

    // ドラッグしたオブジェクトに一致する、オブジェクトをパネル上に移動
    private void ShowMaruBatuObject(GameObject wkPanel) {
        Debug.Log(moveObject.name);
        switch (moveObject.name) {
            case "Maru_small1":
                wkPanel.GetComponent<PanelManager>().GetMaru1_1().GoUpObject();
                break;
            case "Maru_small2":
                wkPanel.GetComponent<PanelManager>().GetMaru1_2().GoUpObject();
                break;
            case "Maru_mid1":
                wkPanel.GetComponent<PanelManager>().GetMaru2_1().GoUpObject();
                break;
            case "Maru_mid2":
                wkPanel.GetComponent<PanelManager>().GetMaru2_2().GoUpObject();
                break;
            case "Maru_big1":
                wkPanel.GetComponent<PanelManager>().GetMaru3_1().GoUpObject();
                break;
            case "Maru_big2":
                wkPanel.GetComponent<PanelManager>().GetMaru3_2().GoUpObject();
                break;
        }
    }

    //ドラッグ元のオブジェクトをパネル下に移動
    private void HideMaruBatuObject(GameObject wkPanel) {
        Debug.Log(moveObject.name);
        switch (moveObject.name) {
            case "Maru_small1":
                wkPanel.GetComponent<PanelManager>().GetMaru1_1().GoDownObject();
                break;
            case "Maru_small2":
                wkPanel.GetComponent<PanelManager>().GetMaru1_2().GoDownObject();
                break;
            case "Maru_mid1":
                wkPanel.GetComponent<PanelManager>().GetMaru2_1().GoDownObject();
                break;
            case "Maru_mid2":
                wkPanel.GetComponent<PanelManager>().GetMaru2_2().GoDownObject();
                break;
            case "Maru_big1":
                wkPanel.GetComponent<PanelManager>().GetMaru3_1().GoDownObject();
                break;
            case "Maru_big2":
                wkPanel.GetComponent<PanelManager>().GetMaru3_2().GoDownObject();
                break;
        }
    }

    //ドラッグ終了時の後処理
    private void AfterEndDragObject() {
        // 移動オブジェクトを破棄
        Destroy(moveObject);
        beRay = false;
        //ドラッグ中解除
        isDragObject = false;

        //オブジェクトを初期化
        tapImageObject = null;
        tapPanelObject = null;
    }

    // 引数を基に、文字列(オブジェクト名)を変更
    // パネルタップ時に生成されるオブジェクト名を変更するための処理
    private string RenameMoveObjectName(string argStr) {
        if (argStr.Contains("Maru1_1")) { return "Maru_small1"; }
        if (argStr.Contains("Maru1_2")) { return "Maru_small2"; }
        if (argStr.Contains("Maru2_1")) { return "Maru_mid1"; }
        if (argStr.Contains("Maru2_2")) { return "Maru_mid2"; }
        if (argStr.Contains("Maru3_1")) { return "Maru_big1"; }
        if (argStr.Contains("Maru3_2")) { return "Maru_big2"; }
        //  該当しない場合は空文字を返却
        return "";
    }
}
