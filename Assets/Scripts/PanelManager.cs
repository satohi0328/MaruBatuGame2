using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour {
	private int posX;       //x軸位置
	private int posY;       //y軸位置
	private int panelNumber; //パネルの番号(1~9)
	private GameObject maru1_1, maru1_2; //小○
	private GameObject maru2_1, maru2_2; //中○
	private GameObject maru3_1, maru3_2; //大○
	private GameObject batu1_1, batu1_2; //小×
	private GameObject batu2_1, batu2_2; //中×
	private GameObject batu3_1, batu3_2; //大×

	private GameObject displayedObject = null; //パネル上の一番表に表示されているオブジェクトを保持

    //パブリック変数
	public GameObject maruObject; //○オブジェクト
	public GameObject batuObject; //×オブジェクト

	private ArrayList putOrderArray; //マルバツオブジェクトが置かれた順に配列に保持

	// 初期化処理
	public void Init(int x,int y) {

		// メンバ変数に値を設定
		this.posX = x;
		this.posY = y;
		this.panelNumber = x * 3 + y * 1;

		// パネルの座標設定
		transform.position = new Vector3(y * 4.5f, 0, x * 4.5f);
		// パネルオブジェクト名を変更
		gameObject.name = "Panle_" + panelNumber;

		// パネル直下にマルバツオブジェクト生成
	    maru1_1 = Instantiate(maruObject) as GameObject;
		maru1_1.GetComponent<MaruBatuObject>().Init(posX, posY, 1,1);
		maru1_2 = Instantiate(maruObject) as GameObject;
		maru1_2.GetComponent<MaruBatuObject>().Init(posX, posY, 1,2);
		maru2_1 = Instantiate(maruObject) as GameObject;
		maru2_1.GetComponent<MaruBatuObject>().Init(posX, posY, 2,1);
		maru2_2 = Instantiate(maruObject) as GameObject;
		maru2_2.GetComponent<MaruBatuObject>().Init(posX, posY, 2,2);
		maru3_1 = Instantiate(maruObject) as GameObject;
		maru3_1.GetComponent<MaruBatuObject>().Init(posX, posY, 3,1);
		maru3_2 = Instantiate(maruObject) as GameObject;
		maru3_2.GetComponent<MaruBatuObject>().Init(posX, posY, 3,2);

		batu1_1 = Instantiate(batuObject) as GameObject;
		batu1_1.GetComponent<MaruBatuObject>().Init(posX, posY, -1,1);
		batu1_2 = Instantiate(batuObject) as GameObject;
		batu1_2.GetComponent<MaruBatuObject>().Init(posX, posY, -1,2);
		batu2_1 = Instantiate(batuObject) as GameObject;
		batu2_1.GetComponent<MaruBatuObject>().Init(posX, posY, -2,1);
		batu2_2 = Instantiate(batuObject) as GameObject;
		batu2_2.GetComponent<MaruBatuObject>().Init(posX, posY, -2,2);
		batu3_1 = Instantiate(batuObject) as GameObject;
		batu3_1.GetComponent<MaruBatuObject>().Init(posX, posY, -3,1);
		batu3_2 = Instantiate(batuObject) as GameObject;
		batu3_2.GetComponent<MaruBatuObject>().Init(posX, posY, -3,2);
	}

	// Use this for Initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //パネル上にオブジェクトがあるか判定
	public bool JudgeOnPanel(GameObject argObj) {
        // 引数オブジェクトの位置を取得
		Vector3 argVec = argObj.transform.position;
        
		Bounds b = this.gameObject.GetComponent<Renderer>().bounds;
		// 各角のポジション取得
		Vector3 boundPoint1 = b.min; //正面左下
		Vector3 boundPoint2 = b.max; //奥面右上
		Vector3 boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z); //奥面左上
		Vector3 boundPoint8 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z); //正面右上

        // パネル上に引数オブジェクトが存在する場合
        if ((boundPoint1.x < argVec.x && argVec.x < boundPoint8.x) &&
			(boundPoint1.z < argVec.z && argVec.z < boundPoint6.z)) {
			return true;
        }
		return false;

	}

	// ゲッター
	public MaruBatuObject GetMaru1_1() {return this.maru1_1.GetComponent<MaruBatuObject>();}
	public MaruBatuObject GetMaru1_2() { return this.maru1_2.GetComponent<MaruBatuObject>(); }
	public MaruBatuObject GetMaru2_1() { return this.maru2_1.GetComponent<MaruBatuObject>(); }
	public MaruBatuObject GetMaru2_2() { return this.maru2_2.GetComponent<MaruBatuObject>(); }
	public MaruBatuObject GetMaru3_1() { return this.maru3_1.GetComponent<MaruBatuObject>(); }
	public MaruBatuObject GetMaru3_2() { return this.maru3_2.GetComponent<MaruBatuObject>(); }

    // 表示されているオブジェクトを保持
    public void SetDisplayedObject() {
		this.displayedObject = maru1_1;
    }

    // 設置されたオブジェクト配列の更新(追加)
    public void AddOrderArray(string argStr) {
		if (argStr.Contains("Maru_small1")) { putOrderArray.Add(maru1_1); }
		if (argStr.Contains("Maru_small2")) { putOrderArray.Add(maru1_2); }
		if (argStr.Contains("Maru_mid1")) { putOrderArray.Add(maru2_1); }
		if (argStr.Contains("Maru_mid2")) { putOrderArray.Add(maru2_2); }
		if (argStr.Contains("Maru_big1")) { putOrderArray.Add(maru3_1); }
		if (argStr.Contains("Maru_big2")) { putOrderArray.Add(maru3_2); }
	}
	// 設置されたオブジェクト配列の更新(削除)
	public void RemoveOrderArray(string argStr) {
		putOrderArray.RemoveAt(putOrderArray.Count-1);
	}

	// パネル上にオブジェクトを置けるか判定
	public bool JudgePutMaruBatuObuject() {
        //TODO おける判定

		return true;
    }

	// パネル上のマルバツオブジェクトを隠す
	public void HideMaruBatuObject() {
		maru1_1.SetActive(false);
		maru1_2.SetActive(false);
		maru2_1.SetActive(false);
		maru2_2.SetActive(false);
		maru3_1.SetActive(false);
		maru3_2.SetActive(false);
		batu1_1.SetActive(false);
		batu1_2.SetActive(false);
		batu2_1.SetActive(false);
		batu2_2.SetActive(false);
		batu3_1.SetActive(false);
		batu3_2.SetActive(false);
	}
	// パネル上のマルバツオブジェクトを再表示する
	public void ShowMaruBatuObject() {
		maru1_1.SetActive(true);
		maru1_2.SetActive(true);
		maru2_1.SetActive(true);
		maru2_2.SetActive(true);
		maru3_1.SetActive(true);
		maru3_2.SetActive(true);
		batu1_1.SetActive(true);
		batu1_2.SetActive(true);
		batu2_1.SetActive(true);
		batu2_2.SetActive(true);
		batu3_1.SetActive(true);
		batu3_2.SetActive(true);
	}

}
