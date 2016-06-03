using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InspiserLokalitet : MonoBehaviour {

	bool showTooltip;
	bool showText;
	Lokalitet l;
	Måling m;
	GUIStyle s;
	string labelText;
	public GUISkin mySkin;
	public Texture xBtn;
	public Texture2D guiDark;
	public Texture2D guiLight;
	Manager manager;
	string info;
	
	// Move window
	Vector2 point;
	Vector2 lastMousePosition;
	bool moving;

	
	OnlineMaps api;
	// Use this for initialization
	void Start () {
		
		api = GameObject.Find ("Tileset map").GetComponent<OnlineMaps>();
		
		l = (Lokalitet)gameObject.GetComponent<OnlineMapsMarker3DInstance>().marker.customData;
		//m = (Måling)e.getSenesteMålingGittDato(Manager.currentDate);
		labelText = l.getLokalitetsnavn ();
		showText = true;

	}

	private string VerticalText(string s)
	{
		string st = "";
		foreach (char c in s)
		{
			st += c + "\n";
		}
		return st.ToUpper();
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
		GUI.skin = mySkin;
		GUI.depth = -100;
		if (showTooltip) {
			Dictionary<String, Double> alleEnheter = getCollectedValues();

			mySkin.box.alignment = TextAnchor.UpperCenter;
			mySkin.box.alignment = TextAnchor.MiddleCenter;
			mySkin.box.normal.textColor = Color.white;
			mySkin.box.normal.background = guiDark;
			
			var barRectNoData = new Rect (point.x, Screen.height - point.y, 180, 20);
			var barRect = new Rect (point.x,Screen.height - point.y, 400, 20);
			
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

			if(alleEnheter.Keys.Count > 0){
				
				GUI.Box (barRect, l.getLokalitetsnavn() + " - for alle enheter");
				
				if(GUI.Button(new Rect (point.x + 398, Screen.height - point.y, 20, 20), xBtn)){
					toggleTooltip();
				}
				
				mySkin.box.alignment = TextAnchor.UpperLeft;
				mySkin.box.normal.background = guiLight;
				mySkin.box.normal.textColor = Color.black;
				
				int count = 0;
				foreach (KeyValuePair<String, Double> pair in alleEnheter)
				{
					mySkin.box.alignment = TextAnchor.UpperLeft;
					mySkin.box.padding.left = 4;
					GUI.Box (new Rect (point.x, Screen.height - point.y + 20 + 11*count, 380, 11), pair.Key.ToString());
					mySkin.box.alignment = TextAnchor.UpperCenter;
					mySkin.box.padding.left = 0;

					string s = pair.Key.ToUpper();
					if(s.Contains("SNITT")){
						GUI.Box (new Rect (point.x + 378, Screen.height - point.y + 20 + 11*count, 40, 11), pair.Value.ToString("0.00"));
					}else{
						GUI.Box (new Rect (point.x + 378, Screen.height - point.y + 20 + 11*count, 40, 11), pair.Value.ToString());
					}
					count++;
				}
			}else{
				GUI.Box (barRectNoData, l.getLokalitetsnavn ());
				
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

	public void setValueText(string l, double d, float t){
		if (t != -100) {
			labelText = l + " " + t.ToString () + "°C\n" + d.ToString ("0.000");
		} else {
			labelText = l + "\n" + d.ToString ("0.000");
		}


	}

	private enum MålingBeregning {
		Snitt, Maks, Total
	};

	private Dictionary<String, Double> getCollectedValues(){
		Dictionary<String, Double> collectedDict = new Dictionary<string, double>();

		MålingBeregning beregning = MålingBeregning.Total;

		foreach(string s in Manager.datatyper){

			var dataType = s.ToUpper();


			if (dataType.Contains("SNITT")) {
				beregning = MålingBeregning.Snitt;
			} else if (dataType.Contains("MAKS")) {
				beregning = MålingBeregning.Maks;
			} else if (dataType.Contains("TOTAL")) {
				beregning = MålingBeregning.Total;
			}

			// Beregner Samlet data for alle enheter til lokaliteten
			if (beregning == MålingBeregning.Snitt) {

				double snitt = 0;
				int num = 0;
				foreach (Enhet e in l.getEnheter ()) {
					try{
						snitt += e.getSenesteMålingGittDato (Manager.currentDate).getValueForKey (s);
						num ++;
					}catch(Exception ex){
						
					}

				}
				if(num > 0){
					snitt = snitt/num;
					collectedDict.Add(s, snitt);
				}else{
					collectedDict.Add(s, 0);
				}
			} else if (beregning == MålingBeregning.Maks) {
				
				double max = 0;
				foreach (Enhet e in l.getEnheter ()) {
					try{
						if(e.getSenesteMålingGittDato (Manager.currentDate).getValueForKey (s) > max){
							max = e.getSenesteMålingGittDato (Manager.currentDate).getValueForKey (s);
						}
					}catch(Exception ex){
						
					}
					
				}
				collectedDict.Add(s, max);
			} else {
				double tot = 0;
				foreach (Enhet e in l.getEnheter ()) {
					try{
						double tempVerdi = e.getSenesteMålingGittDato (Manager.currentDate).getValueForKey (s);
						tot += tempVerdi;
					}catch(Exception ex){

					}
				}
				collectedDict.Add(s, tot);
			}
		}
		return collectedDict;
	}
	
	public string getValueText(){
		return labelText;
	}
	
	public void ToggleText(bool b){
		showText = b;
	}

	public Lokalitet getLokalitet(){
		return l;
	}
}
