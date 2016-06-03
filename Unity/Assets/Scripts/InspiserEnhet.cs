using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InspiserEnhet : MonoBehaviour {
	
	bool showTooltip;
	bool showText;
	Enhet e;
	Måling m;
	GUIStyle s;
	string labelText;
	public GUISkin mySkin;
	public Texture xBtn;
	public Texture2D guiDark;
	public Texture2D guiLight;
	Manager manager;
	string info;

	static int guiDepth = 1;

	// Move window
	Vector2 point;
	Vector2 lastMousePosition;
	bool moving;
	OnlineMaps api;
	// Use this for initialization
	void Start () {

		api = GameObject.Find ("Tileset map").GetComponent<OnlineMaps>();
		e = (Enhet)gameObject.GetComponent<OnlineMapsMarker3DInstance>().marker.customData;
		m = (Måling)e.getSenesteMålingGittDato(Manager.currentDate);
		labelText = e.getEnhetsId ();
		showText = true;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnMouseOver(){
		if (Input.GetMouseButtonDown (0)) {
			toggleTooltip ();
		}
	}
	
	void OnGUI(){
		var pointText = Camera.main.WorldToScreenPoint (transform.position);

		guiDepth = -200;
		GUI.depth = guiDepth;
		GUI.skin = mySkin;
		if (showTooltip) {
			m = (Måling)e.getSenesteMålingGittDato(Manager.currentDate);

			mySkin.box.alignment = TextAnchor.MiddleCenter;
			mySkin.box.normal.textColor = Color.white;
			mySkin.box.normal.background = guiDark;

			var barRectNoData = new Rect (point.x, Screen.height - point.y, 180, 20);
			var barRect = new Rect (point.x,Screen.height - point.y, 400, 20);

			Debug.Log (barRect);
			if (barRect.yMin < 0) {
				point.y = Screen.height;
				barRect = new Rect (point.x + Screen.width / 20, point.y, 380, 20);
			}

			var mp = Input.mousePosition;
							
			if (barRect.Contains (new Vector2 (mp.x, Screen.height - mp.y)) && Input.GetMouseButton (0)) {
				moving = true;	
			}
			if (moving && !Input.GetMouseButton (0)) {
				moving = false;
			}
			if (moving) {
			
				if (lastMousePosition != Vector2.zero) {
					var dmp = new Vector2 (mp.x - lastMousePosition.x, mp.y - lastMousePosition.y);

					point = new Vector2(point.x + dmp.x, point.y  + dmp.y);
				}
				lastMousePosition = mp;
			} else {
					lastMousePosition = Vector2.zero;
			}


			if(m != null){
				Dictionary<String, Double> data = m.getKeyValuePairs();

				GUI.Box (barRect, e.getEnhetsId ());

				if(GUI.Button(new Rect (point.x + 398, Screen.height - point.y, 20, 20), xBtn)){
					toggleTooltip();
				}

				mySkin.box.alignment = TextAnchor.UpperLeft;
				mySkin.box.normal.background = guiLight;
				mySkin.box.normal.textColor = Color.black;

				int count = 0;
				foreach (KeyValuePair<String, Double> pair in data)
				{
					mySkin.box.alignment = TextAnchor.UpperLeft;
					mySkin.box.padding.left = 4;
					GUI.Box (new Rect (point.x, Screen.height - point.y + 20 + 11*count, 380, 11), pair.Key.ToString());
					mySkin.box.alignment = TextAnchor.UpperCenter;
					mySkin.box.padding.left = 0;
					GUI.Box (new Rect (point.x + 378, Screen.height - point.y + 20 + 11*count, 40, 11), pair.Value.ToString());
					count++;
				}
			}else{
				GUI.Box (barRectNoData, e.getEnhetsId ());

				if(GUI.Button(new Rect (point.x + 180, Screen.height - point.y , 20, 20), xBtn)){
					toggleTooltip();
				}

				GUI.Box (new Rect (point.x, Screen.height - point.y + 20, 200, 100), "Ingen data tilgjengelig før denne datoen.");
			}
		}
	}
	
	
	public void toggleTooltip(){
		point = Camera.main.WorldToScreenPoint (transform.position);
		point.y += 50;
		point.x += 10;
		if(showTooltip){
			showTooltip = false;
		}else{
			showTooltip = true;
		}
	}

	public bool Tooltip(){
		return showTooltip;
	}

	public bool isMoving(){
		return moving;
	}

	public void Moving(bool b){
		moving = b;
	}

	public Enhet getEnhet(){
		return e;
	}

	public Vector2 getLastMousePosition(){
		return lastMousePosition;
	}

	public void setLastMousePosition(Vector2 v){
		lastMousePosition = v;
	}


	public void setValueText(double d){
		labelText = e.getEnhetsId() + "\n" + d.ToString("0.000");
	}

	public string getValueText(){
		return labelText;
	}

	public void ToggleText(bool b){
		showText = b;
	}
}
