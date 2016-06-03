using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;

using InfinityCode;
//using UnityEditor;



public class Manager : MonoBehaviour
{

	public static List<Lokalitet> lokaliteter;

	public OnlineMaps onlineMaps;

	public List<DateTime> dates;
	public static DateTime currentDate;
	public static DateTime firstRegisteredDate;

	//GUI
	public Button playPauseButton;
	public Slider slider;
	public InputField searchField;

	public Slider timeSlider;
	public Text timeSliderCurrentDateText;
	public float animationSpeed = 1.0f;

	public Slider animationSpeedSlider;
	public Text animationSpeedSliderText;
	public Text animationSpeedSliderTextTooltip;

	public Camera _camera;

	int defaultMarkerScale = 5;
	float minimumMarkerHeight = 5.0f;
	float maxMarkerHeight = 200.0f;

	GameObject[] lokalitetObjekter;
	GameObject[] enhetObjekter;


	// Data selection
	bool showDataSelection;
	private GUIStyle rowStyle;

	public static List<string> datatyper = new List<string>();
	int valgtDatatype;


	OnlineMapsMarker3D marker;

	public Text valgtDataText;


	bool animating;
	bool browsingFile;
	//FileBrowser

	//skins and textures

	string[] layoutTypes = {"Type 0","Type 1"};
	FileBrowser fb = new FileBrowser();
	string output = "no file";



	// Valg lokalitet eller enhet



	bool visLokalitet;
	bool visEnhet;

	public Button visLokalitetButton;
	public Button visEnhetButton;


	enum MålingBeregning {
		Snitt, Maks, Total
	};


	List<OnlineMapsDrawingElement> enhetDrawingLines = new List<OnlineMapsDrawingElement> ();

	// Inspisering og FileBrowser
	public GUISkin[] skins;
	public GUISkin inspiserSkin;
	public Texture2D file,folder,back,drive;
	
	// Regneark Meny
	bool showRegneArkMenu;
	public GUISkin regnearkMenuSkin;

	public Texture2D normalButtonTex;
	public Texture2D pressedButtonTex;
	public Texture2D hoverButtonTex;
	public Texture2D xButtonTex;


	// Use this for initialization
	void Start ()
	{
		lokalitetObjekter = new GameObject[0];
		enhetObjekter = new GameObject[0];
		currentDate = new DateTime (1, 1, 1);
		lokaliteter = new List<Lokalitet> ();
		ToggleVisLokaliteter ();
		ToggleVisEnheter ();
		animationSpeed = 1.0f;

		animationSpeedSlider = GameObject.Find ("AnimationSpeedSlider").GetComponent<Slider> ();
		animationSpeedSliderText = GameObject.Find("AnimationSpeedSliderText").GetComponent<Text>();
		animationSpeedSliderTextTooltip = GameObject.Find ("AnimationSpeedTooltipText").GetComponent<Text> ();
		timeSlider =  GameObject.Find ("TimeSlider").GetComponent<Slider> ();

		//FileReader
		fb.guiSkin = skins[0]; //set the starting skin
		fb.fileTexture = file; 
		fb.directoryTexture = folder;
		fb.driveTexture = drive;
		fb.showSearch = true;
		fb.searchRecursively = true;

		//Populate(Application.dataPath + "/Resources/06.01.2016-Generell-Info.xls");
		//Populate(Application.dataPath + "/Resources/06.01.2016-Lusetellinger-1712.xls");



	}

	void OnGUI(){


		lokalitetObjekter = GameObject.FindGameObjectsWithTag("Lokalitet");
		enhetObjekter = GameObject.FindGameObjectsWithTag("Enhet");
		//Debug.Log (enhetObjekter.Length);
		
		GUI.depth = 100;
		GUI.skin = inspiserSkin;
		if(lokalitetObjekter.Length > 0){


			foreach(GameObject lok in lokalitetObjekter)
			{
				Lokalitet lokalitet = lok.GetComponent<InspiserLokalitet>().getLokalitet();
				var pos = (Vector3)lok.gameObject.transform.position;
				var screenPos = Camera.main.WorldToScreenPoint (pos);

				string lokInformasjon = lok.GetComponent<InspiserLokalitet>().getValueText();

				inspiserSkin.label.normal.textColor = Color.black;
				inspiserSkin.label.alignment = TextAnchor.MiddleCenter;
				GUI.Label (new Rect (screenPos.x - 151, Screen.height - screenPos.y - 15, 300, 40), lokInformasjon);
				GUI.Label (new Rect (screenPos.x - 149, Screen.height - screenPos.y - 15, 300, 40), lokInformasjon);
				GUI.Label (new Rect (screenPos.x - 150, Screen.height - screenPos.y + 1 - 15, 300, 40), lokInformasjon);
				GUI.Label (new Rect (screenPos.x - 150, Screen.height - screenPos.y - 1 - 15, 300, 40), lokInformasjon);
				
				inspiserSkin.label.normal.textColor = Color.white;
				GUI.Label (new Rect (screenPos.x - 150, Screen.height - screenPos.y - 15, 300, 40), lokInformasjon);

			}

			foreach(GameObject enh in enhetObjekter)
			{
				if(onlineMaps._zoom > 9){
					InspiserEnhet inspiser = enh.GetComponent<InspiserEnhet>();
					Enhet enhet = inspiser.getEnhet();
					var pos = (Vector3)enh.gameObject.transform.position;
					var screenPos = Camera.main.WorldToScreenPoint (pos);

					string enhInformasjon = enh.GetComponent<InspiserEnhet>().getValueText();

					inspiserSkin.label.normal.textColor = Color.black;
					inspiserSkin.label.alignment = TextAnchor.MiddleCenter;
					GUI.Label (new Rect (screenPos.x - 51, Screen.height - screenPos.y - 15, 100, 40), enhInformasjon);
					GUI.Label (new Rect (screenPos.x - 49, Screen.height - screenPos.y - 15, 100, 40), enhInformasjon);
					GUI.Label (new Rect (screenPos.x - 50, Screen.height - screenPos.y + 1 - 15, 100, 40), enhInformasjon);
					GUI.Label (new Rect (screenPos.x - 50, Screen.height - screenPos.y - 1 - 15, 100, 40), enhInformasjon);
					
					inspiserSkin.label.normal.textColor = Color.white;
					GUI.Label (new Rect (screenPos.x - 50, Screen.height - screenPos.y - 15, 100, 40), enhInformasjon);
				}
			}
		}
		
		GUI.depth = 0;


		GUI.skin = regnearkMenuSkin;
		GUI.skin.button.alignment = TextAnchor.MiddleCenter;

		if (GUI.Button(new Rect(5,50,30,20), "-")) onlineMaps.zoom--;

		for (int i = 5; i < 21; i++) {

			if (onlineMaps.zoom == i) {
				GUI.skin.button.normal.background = hoverButtonTex;
			} else {
				GUI.skin.button.normal.background = normalButtonTex;
			}

			if (GUI.Button (new Rect (5, 20 + 10 * i, 30, 10), "")) {
				onlineMaps.zoom = i;
			}
		}
		GUI.skin.button.normal.background = normalButtonTex;


		if (GUI.Button(new Rect(5, 230, 30, 20),"+")) onlineMaps.zoom++;

		GUI.skin.button.alignment = TextAnchor.MiddleLeft;


		if (showRegneArkMenu) {
			GUI.skin = regnearkMenuSkin;
			var boxRect = new Rect (0, 0, 140, 84);
			if (!boxRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) && Input.GetMouseButtonDown(0)) {
				ToggleRegneArkMenu ();
			}
			if (GUI.Button(new Rect(0,28,140,30), "Generel info")) {
				ToggleRegneArkMenu ();
			}
			GUI.enabled = false;
			if (GUI.Button(new Rect(0,56,140,30), "Data info")) {
				toggleFileBrowser ();
				ToggleRegneArkMenu ();
			}
			GUI.enabled = true;
		}

		if(browsingFile){
		GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			GUILayout.Space(10);
			skins[0].label.normal.textColor = Color.black;
			skins[0].button.normal.textColor = Color.black;
			skins[0].button.alignment = TextAnchor.MiddleLeft;
			//GUILayout.Label("Selected File: "+output);
			GUILayout.EndHorizontal();
			//draw and display output
			if(fb.draw()){ //true is returned when a file has been selected
				string filePath = "";
				//the output file is a member if the FileInfo class, if cancel was selected the value is null
				output = (fb.outputFile==null)?"cancel hit":fb.outputFile.ToString();


				//Debug.Log (output);
				if(output != "cancel hit"){
					filePath = fb.outputFile.ToString();

					//Hvis fila eksisterer og er i .xls-format
					if(File.Exists(filePath) && filePath.Trim().EndsWith(".xlsx")){
						//Debug.Log(filePath);
						toggleFileBrowser();
						//Her skal vi kalle på Excel-metoden til arne. Vi sender med fb.outputFile.ToString() som argument.
						Populate(filePath);
					}
				}else{
					toggleFileBrowser();
				}
			}
		}





		var dataSelectionRect = new Rect (0, 30, 500, datatyper.Count * 19 + 4);
		GUI.skin = regnearkMenuSkin;
		if (showDataSelection) {
			
			var buttonHeight = 20;

			//var color = rowStyle.normal.textColor;
			//var hoverColor = rowStyle.hover.textColor;
			for (int i = 0; i < datatyper.Count; i++) {

				if (valgtDatatype == i) {
					GUI.skin.button.normal.background = pressedButtonTex;
				} else {
					GUI.skin.button.normal.background = normalButtonTex;
				}
				if (GUI.Button (new Rect (0, i * buttonHeight + 28, 500, buttonHeight), datatyper[i])) {
					valgtDatatype = i;
					dataTypeChanged ();
				}
			}
			GUI.skin.button.normal.background = normalButtonTex;
		}

		if (showDataSelection && Input.GetMouseButton(0) && !(dataSelectionRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) || new Rect(150, 0,100,30).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))) {
			toggleDataSelection ();

		} 

		if (animationSpeedSliderTextTooltip != null && animationSpeedSliderTextTooltip.text == "Dager per sekund") {
			animationSpeedSliderTextTooltip.transform.position = new Vector3 (Input.mousePosition.x+60, Input.mousePosition.y+20, 0);
		}





	}

	//	private void OnChangeZoom(){
	//		Debug.Log (onlineMaps._zoom);
	//		foreach (OnlineMapsMarker3D m in OnlineMapsControlBase3D.instance.markers3D) {
	//			//m.scale = (onlineMaps._zoom * 2);
	//		}
	//	}

	public void dataTypeChanged(){
		if (datatyper.Count > 0) {
			valgtDataText.text = datatyper [valgtDatatype];
		}
		oppdaterMarkers ();
	}
	void Populate (string filePath)
	{
		/*
		lokaliteter = new ArrayList ();

		lokaliteter.Add (new Lokalitet ("12394", "Ørnøya", new Vector2(63.759167f, 8.449133f)));
		lokaliteter.Add (new Lokalitet ("31959", "Rataren", new Vector2(63.782383f, 8.526367f)));
		*/
		var excelReader = new ExcelReader ();

		lokaliteter = excelReader.readFile (filePath, lokaliteter);

		OnlineMapsControlBase3D control = onlineMaps.GetComponent<OnlineMapsControlBase3D> ();
		control.RemoveAllMarker3D ();

		onlineMaps.RemoveAllDrawingElements ();
		enhetDrawingLines.Clear ();

		for (int i = 0; i < lokaliteter.Count; i++) {
			Lokalitet l = lokaliteter [i] as Lokalitet;


			GameObject mapObject = (GameObject)Resources.Load ("markerPrefab", typeof(GameObject));
			//GameObject mapObject = Instantiate(Resources.Load("markerPrefab", typeof(GameObject))) as GameObject;
			//mapObject.name = lokaliteter[i].getLokalitetsnavn();


			Vector2 position = l.getCoordinates ();
			marker = control.AddMarker3D (position, mapObject);

			marker.position = position;
			marker.label = l.getLokalitetsnavn ();
			marker.scale = defaultMarkerScale;
			marker.customData = l;

			
			control = onlineMaps.GetComponent<OnlineMapsControlBase3D> ();

			l.setMarker (marker);
			//control.AddMarker3D (marker);

			List<Enhet> enheter = l.getEnheter();


			float radius = 0.1f;

			//var circlePoints = new List<Vector2> ();

			for(int j=0; j<l.getEnheter().Count; j++){
				

				Enhet e = enheter[j] as Enhet;
				
				GameObject mapObjectChild = (GameObject)Resources.Load ("markerEnhetPrefab", typeof(GameObject));
				//GameObject mapObjectChild = Instantiate(Resources.Load("markerEnhetPrefab", typeof(GameObject))) as GameObject;
				//mapObjectChild.name = enheter[j].getEnhetsId();


				var angle = j * Mathf.PI * 2 / enheter.Count;
				position = l.getCoordinates ();


				var pos = new Vector2 (position.x + Mathf.Cos (angle) * radius, position.y + Mathf.Sin (angle) * radius * 0.5f);

				marker = control.AddMarker3D (pos, mapObjectChild);

				marker.label = l.getLokalitetsnavn () + ": " + e.getEnhetsId().Replace(" ", "");
				marker.scale = defaultMarkerScale;
				marker.customData = e;
			
				e.setMarker (marker);

				//Destroy(mapObjectChild);

				//circlePoints.Add (pos);

				var linePoints = new List<Vector2> ();
				linePoints.Add (l.getCoordinates ());
				linePoints.Add (pos);

				OnlineMapsDrawingElement line = new OnlineMapsDrawingLine (linePoints, Color.black, 0.3f);			
				//onlineMaps.AddDrawingElement (line);

				enhetDrawingLines.Add (line);

			}

			//OnlineMapsDrawingElement circle = new OnlineMapsDrawingPoly (circlePoints);
			//onlineMaps.AddDrawingElement (circle);

			if (visEnhet) {
				foreach (var line in enhetDrawingLines) {
					onlineMaps.AddDrawingElement (line);
				}
			}


			//Destroy(mapObject);
		}
		UpdateSliderDates ();
		oppdaterMarkers ();
		dataTypeChanged ();

	}

	void UpdateSliderDates() {
		currentDate = firstDate ();


		setTimeSliderMaxValue ();
		setTimeSliderCurrentDateText ();
	}

	// Update is called once per frame
	void Update ()
	{

	}

	private bool startAnimation ()
	{
		// Prøv å start animasjon av data
		animating = true;
		InvokeRepeating ("incrementDay", 1/animationSpeed, 1/animationSpeed);

		return true;
	}

	private bool stopAnimation ()
	{
		// Stop animasjon av data
		animating = false;
		CancelInvoke ("incrementDay");

		return true;
	}

	public void startPauseAnimation ()
	{
		Text t = playPauseButton.GetComponentInChildren<Text> ();
		if (animating) {
			if (stopAnimation ()) {
				// Bytt bilde på playPauseButton?
				t.text = "►";

			}
		} else {
			if (startAnimation ()) {
				// Bytt bilde på playPauseButton?
				t.text = "❙❙";
				if (timeSlider.value == timeSlider.maxValue) {
					timeSlider.value = timeSlider.minValue;
				}

			}
		}
	}

	public void onAnimationSpeedChange(){
		animationSpeed = animationSpeedSlider.value;
		animationSpeedSliderText.text = String.Format ("{0:0.0}", animationSpeed);
		if (animating) {
			CancelInvoke ("incrementDay");
			InvokeRepeating ("incrementDay", 1/animationSpeed, 1/animationSpeed);
		}
	}

	public void incrementDay(){
		timeSlider.value++;
	}

	public void toggleFileBrowser()
	{
		OnlineMapsControlBase3D control = onlineMaps.GetComponent<OnlineMapsControlBase3D> ();
		if (!browsingFile) {
			control.allowZoom = false;
			browsingFile = true;
		} else {
			control.allowZoom = true;
			browsingFile = false;
		}
	}

	public void searchValueChanged ()
	{


		if (Input.GetKey (KeyCode.Backspace) || searchField.text.Equals ("")) {
			return;
		}

		string query = searchField.text.Substring (0, searchField.caretPosition);



	


		Lokalitet l = null;
		for (int i = 0; i < lokaliteter.Count; i++) {
			if (((Lokalitet)lokaliteter [i]).getLokalitetsnavn ().ToUpper ().StartsWith (query.ToUpper ())) {
				l = (Lokalitet)lokaliteter [i];
				break;
			}
		}

		if (l != null) {
			searchField.text = l.getLokalitetsnavn ();
			searchField.caretPosition = query.Length;
			searchField.selectionAnchorPosition = searchField.caretPosition;
			searchField.selectionFocusPosition = searchField.text.Length;
		}
	}

	public void searchEnd ()
	{
		if (Input.GetKey (KeyCode.Return)) {
			search ();
		}
	}

	public void search ()
	{
		string query = searchField.text;

		if (!query.Equals ("")) {
			Lokalitet l = null;
			for (int i = 0; i < lokaliteter.Count; i++) {
				if (((Lokalitet)lokaliteter [i]).getLokalitetsnavn ().ToUpper ().StartsWith (query.ToUpper ())) {
					l = (Lokalitet)lokaliteter [i];
					break;
				}
			}

			if (l != null) {
				onlineMaps.SetPosition (l.getCoordinates ().x, l.getCoordinates ().y);
			}
		}
	}
		
	public DateTime firstDate ()
	{
		DateTime earliestDateSoFar;
		DateTime temp;
		earliestDateSoFar = new DateTime (9000, 1, 1);
		foreach (Lokalitet l in lokaliteter) {
			temp = l.firstDate ();
			if (earliestDateSoFar.CompareTo (temp) > 0) {
				earliestDateSoFar = temp;
			}
		}
		return earliestDateSoFar;

	}

	public DateTime lastDate ()
	{
		DateTime latestDateSoFar;
		DateTime temp;
		latestDateSoFar = new DateTime (1000, 1, 1);
		foreach (Lokalitet l in lokaliteter) {
			temp = l.lastDate ();
			if (latestDateSoFar.CompareTo (temp) < 0) {
				latestDateSoFar = temp;
			}
		}
		return latestDateSoFar;
	}

	public int totalDates(){

		DateTime firstD = new DateTime();
		DateTime lastD = new DateTime();
		firstD = firstDate ();
		lastD = lastDate ();


		if (!firstD.Equals(new DateTime(9000,1,1)) && !lastD.Equals(new DateTime(1000,1,1))){
			return (int)(lastD - firstD).TotalDays;
		}
		return 0;
	}

	public void setTimeSliderMaxValue(){
		timeSlider.maxValue = totalDates ();
	}

	public void setTimeSliderCurrentDateText (){
		GameObject temp = GameObject.Find ("CurrentDateText");
		if (temp != null) {
			timeSliderCurrentDateText = temp.GetComponent<Text>();
			timeSliderCurrentDateText.text = currentDate.ToString ("yyyy-MM-dd");//change to currentDate() when it's implemented
		}
	}

	public void oppdaterMarkers(){

		MålingBeregning beregning = MålingBeregning.Total;

		if (datatyper.Count > 0) {
			var dataType = datatyper [valgtDatatype].ToUpper();

			if (dataType.Contains("SNITT")) {
				beregning = MålingBeregning.Snitt;
			} else if (dataType.Contains("MAKS")) {
				beregning = MålingBeregning.Maks;
			} else if (dataType.Contains("TOTAL")) {
				beregning = MålingBeregning.Total;
			}
		}

		// Beregner høydeskalering
		float høydeSkalering = minimumMarkerHeight;
		if (beregning == MålingBeregning.Total) {

			//Lagrer alle verdier for en type måling i en liste
			List<float> alleVerdierForValgtDatatype = new List<float> ();
			foreach (Lokalitet l in lokaliteter) {
				float tot = 0;
				foreach (Enhet e in l.getEnheter ()) {
					try {
						float tempVerdi = (float)e.getSenesteMålingGittDato (currentDate).getValueForKey (datatyper[valgtDatatype]);
						tot += tempVerdi;
					} catch (Exception ex){
					}

				}
				alleVerdierForValgtDatatype.Add (tot);
			}
			//Går igjennom listen og finner høyeste verdi, og regner ut høydeskalering basert på den
			høydeSkalering = beregnHøydeSkalering (valgtDatatype, alleVerdierForValgtDatatype);


		} else {

			//Lagrer alle verdier for en type måling i en liste
			List<float> alleVerdierForValgtDatatype = new List<float> ();

			foreach (Lokalitet l in lokaliteter) {
				foreach (Enhet e in l.getEnheter ()) {
					try {
						float tempVerdi = (float)e.getSenesteMålingGittDato (currentDate).getValueForKey (datatyper[valgtDatatype]);
						alleVerdierForValgtDatatype.Add (tempVerdi);
					} catch (Exception ex){
					}

				}
			}
			//Går igjennom listen og finner høyeste verdi, og regner ut høydeskalering basert på den
			høydeSkalering = beregnHøydeSkalering (valgtDatatype, alleVerdierForValgtDatatype);
		}



		//Går igjennom alle markers og legger til skalering
		foreach (Lokalitet l in lokaliteter) {

			l.getMarker ().instance.transform.localScale = new Vector3 ((float)defaultMarkerScale, minimumMarkerHeight, (float)defaultMarkerScale);
			float d = 0;
			foreach (Enhet e in l.getEnheter ()) {

				float enhetMåling = -1;

				//d = minimumMarkerHeight;
				e.getMarker ().instance.transform.localScale = new Vector3 ((float)defaultMarkerScale, minimumMarkerHeight, (float)defaultMarkerScale);
				try {
					float de = (float)e.getSenesteMålingGittDato (currentDate).getValueForKey (datatyper[valgtDatatype]);
					enhetMåling = (float)(de * høydeSkalering);

					switch (beregning) {
					case MålingBeregning.Snitt:
					case MålingBeregning.Total:
						d += de;
						break;
					case MålingBeregning.Maks:
						d = de > d ? de : d;
						break;
					}


					e.getMarker().instance.GetComponent<InspiserEnhet>().setValueText(de);


				} catch (Exception ex) {

				}



				skalerMarker (e.getMarker (), enhetMåling);
			}
			//skalerer lokaliteter (gir egentlig ikke mening før data er samlet på lokalitet)

			switch (beregning) {
			case MålingBeregning.Snitt:
				d /= l.getEnheter().Count;
				break;
			case MålingBeregning.Total:
				break;
			case MålingBeregning.Maks:
				break;
			}



			l.getMarker ().instance.GetComponent<InspiserLokalitet> ().setValueText (l.getLokalitetsnavn(), d, l.getSenesteTemperaturGittDato(currentDate));
			skalerMarker (l.getMarker (), d * høydeSkalering);
		}


	}

	public float beregnHøydeSkalering(int valgtDatatype, List<float> alleVerdierForValgtDattype){
		//Vet ikke hvorfor datatype skal ha noe å si, men kan bli bruk for senere
		float maxVerdiSåLangt = 0;
		float høydeSkalering = 0;


			foreach (float f in alleVerdierForValgtDattype) {
				if (f > maxVerdiSåLangt) {
					maxVerdiSåLangt = f;
				}
			}


		if (maxVerdiSåLangt > 0) {
			høydeSkalering = maxMarkerHeight / maxVerdiSåLangt;
		} else {
			høydeSkalering = minimumMarkerHeight;
		}
//		Debug.Log ("Valgt datatype: " + valgtDatatype + "\nMaxVerdi: " + maxVerdiSåLangt + "\tHøydeskalering: " + høydeSkalering);
		return høydeSkalering;

	}

	public void skalerMarker(OnlineMapsMarker3D marker, float d){
		if (d == null) {
			marker.instance.GetComponent<Renderer> ().material.color = new Color (1f, 1f, 1f);
			marker.instance.transform.localScale = new Vector3 ((float)defaultMarkerScale, minimumMarkerHeight, (float)defaultMarkerScale);
			return;
		}
		if (d < minimumMarkerHeight) {
			marker.instance.GetComponent<Renderer> ().material.color = new Color (1f, 1f, 1f);
			marker.instance.transform.localScale = new Vector3 ((float)defaultMarkerScale, minimumMarkerHeight, (float)defaultMarkerScale);
		} else {
			marker.instance.GetComponent<Renderer> ().material.color = new Color ((((d / maxMarkerHeight)/2f)+0.5f), (maxMarkerHeight - d) / maxMarkerHeight, 0f);
			marker.instance.transform.localScale = new Vector3 ((float)defaultMarkerScale, d, (float)defaultMarkerScale);
		}

	}

	public void onDateChanged(){
		currentDate = (firstDate ().AddDays (timeSlider.value));
		setTimeSliderCurrentDateText ();
		Måling målingen;
		oppdaterMarkers ();
		if (timeSlider.value == timeSlider.maxValue) {
			startPauseAnimation ();
		}

	}

	public void toggleDataSelection() {
		OnlineMapsControlBase3D control = onlineMaps.GetComponent<OnlineMapsControlBase3D> ();
		if (!showDataSelection) {
			control.allowZoom = false;
			showDataSelection = true;
		} else {
			control.allowZoom = true;
			showDataSelection = false;
		}

	}


	/// Valg om lokaliter eller enheter skal vises

	public void ToggleVisLokaliteter() {
		visLokalitet = !visLokalitet;


		if (visLokalitet) {
			var cb = visLokalitetButton.colors;

			var pressedColor = cb.pressedColor;
			cb.normalColor = pressedColor;
			cb.highlightedColor = pressedColor;

			visLokalitetButton.colors = cb;
		} else {
			visLokalitetButton.colors = ColorBlock.defaultColorBlock;
		}

		OnlineMapsControlBase3D control = onlineMaps.GetComponent<OnlineMapsControlBase3D> ();

		foreach (var l in lokaliteter) {
			l.getMarker ().instance.SetActive (visLokalitet);
			l.getMarker ().instance.GetComponent<MeshRenderer> ().enabled = visLokalitet;
		}
	}

	public void ToggleVisEnheter() {
		
		visEnhet = !visEnhet;

		if (visEnhet) {
			var cb = visEnhetButton.colors;

			var pressedColor = cb.pressedColor;
			cb.normalColor = pressedColor;
			cb.highlightedColor = pressedColor;

			visEnhetButton.colors = cb;
		} else {
			visEnhetButton.colors = ColorBlock.defaultColorBlock;
		}
			

		foreach (var l in lokaliteter) {
			foreach (var e in l.getEnheter()) {
				e.getMarker ().instance.SetActive (visEnhet);

				e.getMarker ().instance.GetComponent<MeshRenderer> ().enabled = visEnhet;
				e.getMarker ().instance.GetComponent<InspiserEnhet> ().ToggleText (visEnhet);
			}
		}

		if (visEnhet) {
			foreach (var drawing in enhetDrawingLines) {
				onlineMaps.AddDrawingElement (drawing);
			}
		} else {
			onlineMaps.RemoveAllDrawingElements ();
		}

	}


	public void ToggleRegneArkMenu() {
		showRegneArkMenu = !showRegneArkMenu;

	}
}