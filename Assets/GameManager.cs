using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public GameObject panleObject;
    public GameObject maruObject;
    public GameObject batuObject;
    public GameObject[,] panelStatus = new GameObject[3, 3]; //生成するオブジェクトを保持
    public float objectSize_mid = 1.5f; // 中オブジェクトのサイズ
    public float objectSize_big = 2f; // 大オブジェクトのサイズ

    public Text centerText;
    public GameObject centerPanel;

    private Vector3 moveTo;
    private GameObject moveObject; //Imageをタップしてドラッグするためのオブジェクト
    private GameObject tapPanelObject = null; //パネル上のオブジェクトをタップしてドラッグするためのオブジェクト
    private bool isDragObject = false; //ドラッグしているかフラグ
    private GameObject tapImageObject; //タップされたイメージを保持
    private int whichTurn; //どっちのターンか保持(○:1 ,×:-1)
    private AudioSource[] sources; // 音声の配列

    private GameObject coverMaruPanel; //下ウィンドウを擬似的に非活性にするためのパネルオブジェクト
    private GameObject coverBatuPanel; //上ウィンドウを擬似的に非活性にするためのパネルオブジェクト

    private bool beRay = false; //タップされた時のイベントフラグ

    private bool gameSetFlg = false; //ゲームセットしたか保持するフラグ

    // Use this for initialization
    void Start() {
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

        // 音声取得 [0:ゲームBGM][1:開始音][2:終了音][3:配置音]
        sources = gameObject.GetComponents<AudioSource>();
        sources[1].Play(); //開始音再生

        // 非活性用のパネルを取得
        coverMaruPanel = GameObject.Find("CoverMaruPanel");
        coverBatuPanel = GameObject.Find("CoverBatuPanel");

        //中心のテキストパネルを非表示
        centerPanel.SetActive(false);

        //どっちか先行か設定
        whichTurn = 1;
    }

    // Update is called once per frame
    void Update() {

        //ゲームセットフラグが立っていれば処理しない
        if (gameSetFlg) {
            //中心のテキストパネルを表示
            centerPanel.SetActive(true);


            return;
        }

        //○のターンの場合
        if (whichTurn < 0) {
            coverMaruPanel.SetActive(true);
            coverBatuPanel.SetActive(false);

            //×のターンの場合
        } else {
            coverMaruPanel.SetActive(false);
            coverBatuPanel.SetActive(true);
        }


        // 画面がタップされた場合
        if (Input.touchCount > 0) {
            // タッチ情報の取得
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                Debug.Log("touchdown");
                RayCheck();

            }
            // 画面がクリックされた場合
        }else if (Input.GetMouseButtonDown(0)) {
            Debug.Log("buttondown");
            RayCheck();
        }


        if (beRay) {
            //オブジェクトをドラッグ中の場合
            if (isDragObject) {
                MovePoisition();
            }
        }

        if (Input.GetMouseButtonUp(0) && beRay) {
            //オブジェクトをドラッグ中の場合
            if (isDragObject) {
                // 作業用パネルを生成
                GameObject wkPanel = null;
                //ドラッグ中のオブジェクトがパネル上にあるか判定
                wkPanel = JudgeDetachObjectOnPanel();
                //判定の結果、パネル上にあった場合
                if (wkPanel != null) {
                    // 移動先パネル上に置けるか判定
                    if (wkPanel.GetComponent<PanelManager>().JudgePutMaruBatuObuject(moveObject.name)) {
                        // 画面下ウィンドウの画像をタップした場合と、パネル上のオブジェクトをタップした場合で処理を分ける
                        //パネルをタップした時のドラッグ中の場合
                        if (tapPanelObject != null) {
                            // 移動元と移動先が同じパネルじゃない場合
                            if (wkPanel.GetComponent<PanelManager>().GetPanelNumber() != tapPanelObject.GetComponent<PanelManager>().GetPanelNumber()) {
                                //パネル上のオブジェクトを再表示
                                tapPanelObject.GetComponent<PanelManager>().ShowMaruBatuObject();
                                // パネル上に表示されているまるばつオブジェクト配列を更新
                                tapPanelObject.GetComponent<PanelManager>().RemoveOrderArray(); //移動元パネルの配列から削除
                                // パネル下に移動
                                HideMaruBatuObject(tapPanelObject);

                                // 勝敗判定(タップしたオブジェクトをどかした盤面で勝敗がつく可能性があるため)※レビュー指摘
                                JudgeGameStatus();
                                // どかした時点で勝敗が決まった場合
                                if (gameSetFlg) {
                                    // 移動元オブジェクトを徐々に上昇させる。
                                    ShowMaruBatuObjectEX(wkPanel);

                                    // ドラッグ終了処理
                                    AfterEndDragObject();
                                    return;
                                }
                                // レビュー指摘ここまで


                                wkPanel.GetComponent<PanelManager>().AddOrderArray(moveObject.name); //移動先パネルの配列に追加
                            } else {
                                // 移動元のオブジェクトを際表示
                                ShowMaruBatuObject(tapPanelObject);
                            }

                            // 画面下のImageをタップした時のドラッグ中の場合
                        } else if (tapImageObject != null) {
                            //一度使ったImageは非表示にする
                            tapImageObject.SetActive(false);

                            // パネル上に表示されているまるばつオブジェクト配列を更新
                            wkPanel.GetComponent<PanelManager>().AddOrderArray(moveObject.name); //移動先パネルの配列に追加

                        }
                        //パネル上に置く(表示)
                        ShowMaruBatuObject(wkPanel);


                        // 交代
                        whichTurn *= -1;
                        sources[3].Play(); //ターンチェンジ音再生

                    }


                    //パネル上以外で手放した場合
                } else {
                    //処理なし
                }
            }

            //TODO デバック用　パネルに置かれたもの配列を表示
            WKShowAllAddArray();

            // ドラッグ終了処理
            AfterEndDragObject();

            //勝敗判定
            JudgeGameStatus();
        }
    }

    private void RayCheck() {
        //パネルに配置されたオブジェクトがタップされた時の処理
        Ray ray = new Ray();
        RaycastHit hit = new RaycastHit();
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // パネル上のまるばつオブジェクトをタップしたかつ、
        // まるターンの場合は丸オブジェクトをタップまたは、ばつターンの場合は罰オブジェクトをタップした場合。
        if (Physics.Raycast(ray, out hit, 100.0f) &&
            (hit.collider.gameObject.CompareTag("MaruObject") && whichTurn > 0) ||
            (hit.collider.gameObject.CompareTag("BatuObject") && whichTurn < 0)) {

            // どのパネルか取得
            for (int i = 0; i < panelStatus.GetLength(0); i++) {
                for (int j = 0; j < panelStatus.GetLength(1); j++) {
                    // パネル上にあった場合
                    if (panelStatus[i, j].GetComponent<PanelManager>().JudgeOnPanel(hit.collider.gameObject)) {
                        // パネルオブジェクトを一時保持。
                        tapPanelObject = panelStatus[i, j];
                        // パネル上のオブジェクトを一時非表示
                        tapPanelObject.GetComponent<PanelManager>().HideMaruBatuObject();

                        // ○側の場合
                        if (whichTurn > 0) {
                            // タップ時にドラッグ中に表示するオブジェクト生成
                            moveObject = Instantiate(maruObject);

                            // ×側の場合
                        } else {
                            // タップ時にドラッグ中に表示するオブジェクト生成
                            moveObject = Instantiate(batuObject);

                        }
                        // タップされたオブジェクト名によって、ドラッグ中に表示するオブジェクト名を変更
                        moveObject.name = RenameMoveObjectName(hit.collider.gameObject.transform.root.name);
                        // タップされたオブジェクトをもとに大きさ変更
                        if (moveObject.name.Contains("mid")) {
                            moveObject.transform.localScale *= objectSize_mid;
                        } else if (moveObject.name.Contains("big")) {
                            moveObject.transform.localScale *= objectSize_big;
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
                // ○側の場合
                if (whichTurn > 0) {
                    // タップ時にドラッグ中に表示するオブジェクト生成
                    moveObject = Instantiate(maruObject);

                    // ×側の場合
                } else {
                    // タップ時にドラッグ中に表示するオブジェクト生成
                    moveObject = Instantiate(batuObject);
                }
                // ドラッグ中に表示するオブジェクト名を変更
                moveObject.name = raycastResult.gameObject.name;
                // タップされたImageをもとに大きさ変更
                if (moveObject.name.Contains("mid")) {
                    moveObject.transform.localScale *= objectSize_mid;
                } else if (moveObject.name.Contains("big")) {
                    moveObject.transform.localScale *= objectSize_big;
                }

                //ドラッグ中
                isDragObject = true;
                beRay = true;
            }
            break;
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


    //ドラッグ終了時の後処理
    private void AfterEndDragObject() {
        // 移動オブジェクトを破棄
        Destroy(moveObject);
        beRay = false;
        //ドラッグ中解除
        isDragObject = false;

        // 画面下ウィンドウの画像をタップした場合と、パネル上のオブジェクトをタップした場合で処理を分ける
        //パネルをタップした時のドラッグ中の場合
        if (tapPanelObject != null) {
            // パネル上のオブジェクトを再表示
            tapPanelObject.GetComponent<PanelManager>().ShowMaruBatuObject();

            // 画面下のImageをタップした時のドラッグ中の場合
        } else if (tapImageObject != null) {
            //処理なし

        }

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
        if (argStr.Contains("Batu1_1")) { return "Batu_small1"; }
        if (argStr.Contains("Batu1_2")) { return "Batu_small2"; }
        if (argStr.Contains("Batu2_1")) { return "Batu_mid1"; }
        if (argStr.Contains("Batu2_2")) { return "Batu_mid2"; }
        if (argStr.Contains("Batu3_1")) { return "Batu_big1"; }
        if (argStr.Contains("Batu3_2")) { return "Batu_big2"; }
        //  該当しない場合は空文字を返却
        return "";
    }

    // ドラッグしたオブジェクトに一致する、オブジェクトをパネル上に移動
    private void ShowMaruBatuObject(GameObject wkPanel) {

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
            case "Batu_small1":
                wkPanel.GetComponent<PanelManager>().GetBatu1_1().GoUpObject();
                break;
            case "Batu_small2":
                wkPanel.GetComponent<PanelManager>().GetBatu1_2().GoUpObject();
                break;
            case "Batu_mid1":
                wkPanel.GetComponent<PanelManager>().GetBatu2_1().GoUpObject();
                break;
            case "Batu_mid2":
                wkPanel.GetComponent<PanelManager>().GetBatu2_2().GoUpObject();
                break;
            case "Batu_big1":
                wkPanel.GetComponent<PanelManager>().GetBatu3_1().GoUpObject();
                break;
            case "Batu_big2":
                wkPanel.GetComponent<PanelManager>().GetBatu3_2().GoUpObject();
                break;
        }
    }

    //ドラッグ元のオブジェクトをパネル下に移動
    private void HideMaruBatuObject(GameObject wkPanel) {
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
            case "Batu_small1":
                wkPanel.GetComponent<PanelManager>().GetBatu1_1().GoDownObject();
                break;
            case "Batu_small2":
                wkPanel.GetComponent<PanelManager>().GetBatu1_2().GoDownObject();
                break;
            case "Batu_mid1":
                wkPanel.GetComponent<PanelManager>().GetBatu2_1().GoDownObject();
                break;
            case "Batu_mid2":
                wkPanel.GetComponent<PanelManager>().GetBatu2_2().GoDownObject();
                break;
            case "Batu_big1":
                wkPanel.GetComponent<PanelManager>().GetBatu3_1().GoDownObject();
                break;
            case "Batu_big2":
                wkPanel.GetComponent<PanelManager>().GetBatu3_2().GoDownObject();
                break;
        }
    }

    // ゲームの状況を判定(揃ったか判定)
    /**
    * 盤面上のまるばつを判定
    * 盤面の番号は以下
    * ------------
    *  7 | 8 | 9 |
    * ------------
    *  4 | 5 | 6 |
    * ------------
    *  1 | 2 | 3 |
    * ------------
    * ○:正の数 , ×:負の数 ,未配置:0
*/
    private void JudgeGameStatus() {

        int panel1 = panelStatus[0, 0].GetComponent<PanelManager>().GetDisplayedObjectType() * whichTurn;
        int panel2 = panelStatus[0, 1].GetComponent<PanelManager>().GetDisplayedObjectType() * whichTurn;
        int panel3 = panelStatus[0, 2].GetComponent<PanelManager>().GetDisplayedObjectType() * whichTurn;
        int panel4 = panelStatus[1, 0].GetComponent<PanelManager>().GetDisplayedObjectType() * whichTurn;
        int panel5 = panelStatus[1, 1].GetComponent<PanelManager>().GetDisplayedObjectType() * whichTurn;
        int panel6 = panelStatus[1, 2].GetComponent<PanelManager>().GetDisplayedObjectType() * whichTurn;
        int panel7 = panelStatus[2, 0].GetComponent<PanelManager>().GetDisplayedObjectType() * whichTurn;
        int panel8 = panelStatus[2, 1].GetComponent<PanelManager>().GetDisplayedObjectType() * whichTurn;
        int panel9 = panelStatus[2, 2].GetComponent<PanelManager>().GetDisplayedObjectType() * whichTurn;
        int argStatus = whichTurn * -1;//判定順に利用


        // 勝利判定...ターンごとに判定する順番を変える
        // ○ターンだった場合、×が揃ったか先に判定
        // ×ターンだった場合、○が揃ったか先に判定

        // 1,2,3が揃ってる場合
        if (panel1 < 0 && panel2 < 0 && panel3 < 0) { GameSetProc(-1* whichTurn); }
        // 4,5,6が揃ってる場合
        if (panel4 < 0 && panel5 < 0 && panel6 < 0) { GameSetProc(-1 * whichTurn); }
        // 7,8,9が揃ってる場合
        if (panel7 < 0 && panel8 < 0 && panel9 < 0) { GameSetProc(-1 * whichTurn); }
        // 1,4,7が揃ってる場合
        if (panel1 < 0 && panel4 < 0 && panel7 < 0) { GameSetProc(-1 * whichTurn); }
        // 2,5,8が揃ってる場合
        if (panel2 < 0 && panel5 < 0 && panel8 < 0) { GameSetProc(-1 * whichTurn); }
        // 3,6,9が揃ってる場合
        if (panel3 < 0 && panel6 < 0 && panel9 < 0) { GameSetProc(-1 * whichTurn); }
        // 1,5,9が揃ってる場合
        if (panel1 < 0 && panel5 < 0 && panel9 < 0) { GameSetProc(-1 * whichTurn); }
        // 3,5,7が揃ってる場合
        if (panel3 < 0 && panel5 < 0 && panel7 < 0) { GameSetProc(-1 * whichTurn); }


        // ○側の勝利判定
        // 1,2,3が揃ってる場合
        if (panel1 > 0 && panel2 > 0 && panel3 > 0) { GameSetProc(1 * whichTurn); }
        // 4,5,6が揃ってる場合
        if (panel4 > 0 && panel5 > 0 && panel6 > 0) { GameSetProc(1 * whichTurn); }
        // 7,8,9が揃ってる場合
        if (panel7 > 0 && panel8 > 0 && panel9 > 0) { GameSetProc(1 * whichTurn); }
        // 1,4,7が揃ってる場合
        if (panel1 > 0 && panel4 > 0 && panel7 > 0) { GameSetProc(1 * whichTurn); }
        // 2,5,8が揃ってる場合
        if (panel2 > 0 && panel5 > 0 && panel8 > 0) { GameSetProc(1 * whichTurn); }
        // 3,6,9が揃ってる場合
        if (panel3 > 0 && panel6 > 0 && panel9 > 0) { GameSetProc(1 * whichTurn); }
        // 1,5,9が揃ってる場合
        if (panel1 > 0 && panel5 > 0 && panel9 > 0) { GameSetProc(1 * whichTurn); }
        // 3,5,7が揃ってる場合
        if (panel3 > 0 && panel5 > 0 && panel7 > 0) { GameSetProc(1 * whichTurn); }


        // 全部埋まっている場合(引き分け)
        if (panel1 < 0 && panel2 < 0 && panel3 < 0 && panel4 < 0 && panel5 < 0 && panel6 < 0 && panel7 < 0 && panel8 < 0 && panel9 < 0) { GameSetProc(0); }

        if (panel1 > 0 && panel2 > 0 && panel3 > 0 && panel4 > 0 && panel5 > 0 && panel6 > 0 && panel7 > 0 && panel8 > 0 && panel9 > 0) { GameSetProc(0); }

    }

    //ゲームセット処理(ゲームセット時の状態 ○勝ち:1 ×勝ち:-1 引き分け:0)
    private void GameSetProc(int status) {

        switch (status) {
            case 1:
                centerText.text = "まるのかち";
                centerText.color = Color.red;
                break;
            case -1:
                centerText.text = "ばつのかち";
                centerText.color = Color.blue;
                break;
            case 0:
                centerText.text = "ひきわけ";
                break;
        }

        gameSetFlg = true;

        sources[2].Play(); //終了音再生

        //中心のテキストパネルを表示
        centerPanel.SetActive(true);
        centerPanel.GetComponent<CenterPanelScript>().SetGameSetFlg(true);

    }

    private void WKShowAllAddArray() {
        // パネルを取得
        for (int i = 0; i < panelStatus.GetLength(0); i++) {
            for (int j = 0; j < panelStatus.GetLength(1); j++) {
                //　配列を表示
                panelStatus[i, j].GetComponent<PanelManager>().ShowPutOrderArray();
            }
        }
    }


    // 特殊な終わりかたをした場合のオブジェクトの表示
    private void ShowMaruBatuObjectEX(GameObject wkPanel) {
        switch (moveObject.name) {
            case "Maru_small1":
                wkPanel.GetComponent<PanelManager>().GetMaru1_1().GoUpObjectEX();
                break;
            case "Maru_small2":
                wkPanel.GetComponent<PanelManager>().GetMaru1_2().GoUpObjectEX();
                break;
            case "Maru_mid1":
                wkPanel.GetComponent<PanelManager>().GetMaru2_1().GoUpObjectEX();
                break;
            case "Maru_mid2":
                wkPanel.GetComponent<PanelManager>().GetMaru2_2().GoUpObjectEX();
                break;
            case "Maru_big1":
                wkPanel.GetComponent<PanelManager>().GetMaru3_1().GoUpObjectEX();
                break;
            case "Maru_big2":
                wkPanel.GetComponent<PanelManager>().GetMaru3_2().GoUpObjectEX();
                break;
            case "Batu_small1":
                wkPanel.GetComponent<PanelManager>().GetBatu1_1().GoUpObjectEX();
                break;
            case "Batu_small2":
                wkPanel.GetComponent<PanelManager>().GetBatu1_2().GoUpObjectEX();
                break;
            case "Batu_mid1":
                wkPanel.GetComponent<PanelManager>().GetBatu2_1().GoUpObjectEX();
                break;
            case "Batu_mid2":
                wkPanel.GetComponent<PanelManager>().GetBatu2_2().GoUpObjectEX();
                break;
            case "Batu_big1":
                wkPanel.GetComponent<PanelManager>().GetBatu3_1().GoUpObjectEX();
                break;
            case "Batu_big2":
                wkPanel.GetComponent<PanelManager>().GetBatu3_2().GoUpObjectEX();
                break;
        }
    }

}


