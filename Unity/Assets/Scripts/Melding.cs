using UnityEngine;
using System.Collections;
using System;

public class Melding : MonoBehaviour {

	public float width;
	public float height;

	bool show;
	string title;
	string text;


	public GUISkin skin;

	// Use this for initialization
	void Start () {
		//Show("Lorem ipsum", "GUI.skin.label.fontSize = 18;// OutlineGUI.skin.label.normal.textColor = Color.black;GUI.Label (new Rect ((Screen.width - width) / 2 + 1, (Screen.height - height) / 2, width, 30), title);GUI.Label (new Rect ((Screen.width - width) / 2 - 1, (Screen.height - height) / 2, width, 30), title);\n\t\t\tGUI.Label (new Rect ((Screen.width - width) / 2, (Screen.height - height) / 2 + 1, width, 30), title);");
	}


	public void OnGUI() {

		if (show) {
			GUI.depth = -200;

			var bounds = new Rect ((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
			GUI.skin = skin;
			GUI.Box (bounds, "");

			/* Tittel */
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.skin.label.fontSize = 18;

			GUI.Label (new Rect ((Screen.width - width) / 2, (Screen.height - height) / 2, width, 40), title);

			/* Text */
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
			GUI.skin.label.fontSize = 14;
			GUI.Label (new Rect ((Screen.width - width) / 2 + 20, (Screen.height - height) / 2 + 50, width - 40, height - 100), text);

			/* Ok Button */
			var buttonWidth = 100;
			var buttonHeight = 30;
			var okButtonRect = new Rect ((Screen.width - buttonWidth) / 2, (Screen.height + height) / 2 - buttonHeight - 10, buttonWidth, buttonHeight);
			if (GUI.Button (okButtonRect, "Ok")) {
				show = false;
				var mapControl = GameObject.Find ("Tileset map").GetComponent<OnlineMapsControlBase3D> ();
				mapControl.allowUserControl = true;
			}
		}
	}


	public void Show(string tittel, string melding) {
		show = true;
		title = tittel;
		text = melding;

		var mapControl = GameObject.Find ("Tileset map").GetComponent<OnlineMapsControlBase3D> ();
		mapControl.allowUserControl = false;

	}

	[Obsolete ("Bruk Show(tittel, melding)")]
	public void Show(string melding) {
		show = true;
		text = melding;

		var mapControl = GameObject.Find ("Tileset map").GetComponent<OnlineMapsControlBase3D> ();
		mapControl.allowUserControl = false;

	}
}
