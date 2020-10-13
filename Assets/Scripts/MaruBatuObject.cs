using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaruBatuObject : MonoBehaviour {

	private int posX;       //x軸位置
	private int posY;       //y軸位置
	private int panelNumber; //3*3マスのどのマスか(1~9)
	private int objType; //○×のどれか(大○:3,中○:2,小○:1,×:-3,×:-2,×:-1);
	private int dx; //名前重複防止用(大1,大2みたいな)


	// 初期化処理
	public void Init(int x,int y, int argObjType,int dx) {
        // メンバ変数に値を設定
		this.posX = x;
		this.posY = y;
		this.objType = argObjType;
		this.panelNumber = x * 3 + y * 1 + 1;
		this.dx = dx;

		// オブジェクトの位置を調整
		transform.position = new Vector3(0,0,0);
		// 座標設定 パネルの直下に移動
		transform.position = new Vector3(y * 4.5f, -10, x * 4.5f);
		// オブジェクトの大きさを変更
		transform.localScale *=  (0.5f + Mathf.Abs(argObjType)*0.5f);

		// オブジェクト名を変更 (Maru(大きさ)_(どのマス))
		// ○の場合
		if (objType > 0) {
			this.gameObject.name = "Maru" + Mathf.Abs(argObjType) + "_" + dx + "_" + panelNumber; // オブジェクト名を変更

            //×の場合
		} else {
			this.gameObject.name = "Batu" + Mathf.Abs(argObjType) + "_" + dx + "_" + panelNumber; // オブジェクト名を変更
		}
	}

    public void GoUpObject() {
        // y軸のみ0にする
        switch (Mathf.Abs(this.objType)) {
			case 1: //小
				transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
				break;
			case 2: //中
				transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
				break;
			case 3: //大
				transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
				break;
		}


    }

	public void GoDownObject() {
		// y軸のみ-10にする
		transform.position = new Vector3(transform.position.x, -10, transform.position.z);
	}

    public int GetObjectType() {
		return this.objType;
    }

	public void GoUpObjectEX() {
		// y軸のみ0にする
		switch (Mathf.Abs(this.objType)) {
			case 1: //小
				transform.position = new Vector3(transform.position.x + 1, 3f, transform.position.z + 1);
				break;
			case 2: //中
				transform.position = new Vector3(transform.position.x + 1.5f, 4.5f, transform.position.z + 1.5f);
				break;
			case 3: //大
				transform.position = new Vector3(transform.position.x + 1.5f, 5f, transform.position.z + 1.5f);
				break;
		}


	}
}
