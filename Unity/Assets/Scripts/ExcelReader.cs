// Hentet fra: https://github.com/ExcelDataReader/ExcelDataReader
// den 08/01-16 kl. 15:00


using UnityEngine;
using System.Collections;
using System; 
using System.IO; 
using System.Data;
using Excel;
using System.Collections.Generic;




public class ExcelReader {

	enum filTyper : int {LEInfo, Data, Temp};

	public int finnFilType(string filePath){
		FileStream stream = null;
		IExcelDataReader excelReader = null;
		try {
			stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

			excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
			excelReader.Read();
			if(excelReader.Read()){
				for (int i = 0; i < excelReader.FieldCount; i++) {
					string h = excelReader.GetString(i);
					if(h.Contains("Breddegrad") || h.Contains("Lengdegrad")){
						return (int)filTyper.LEInfo;
					}
					if (h.Contains("lusetelling")){
						return (int)filTyper.Data;
					}
					if (h.Contains("Temperatur")){
						return (int)filTyper.Temp;
					}
				}
			}

		}catch (Exception ex){
			Debug.Log (ex);
		} finally{
			try{
				excelReader.Close();
				stream.Close();
			} catch (Exception ex2){
				Debug.Log (ex2);
			}

		}
		return -1;
	}

	public List<Lokalitet> readFile(string filePath, List<Lokalitet> lokaliteter){
		int filType = finnFilType (filePath);

		List<Lokalitet> l = new List<Lokalitet> ();
		switch (filType)
		{
		case (int)filTyper.LEInfo:
			l = readLEInfo (filePath);
			GameObject.Find ("Manager").GetComponent<Melding>().Show ("Lokalitets- og enhets-informasjon innlest", filePath + " lest inn");
			break;
		case (int)filTyper.Data:
			GameObject.Find ("Manager").GetComponent<Melding>().Show ("Lusetellinger innlest", filePath + " lest inn");
			l = readData (filePath, lokaliteter);
			break;
		case (int)filTyper.Temp:
			GameObject.Find ("Manager").GetComponent<Melding>().Show ("Temperaturmålinger innlest", filePath + " lest inn");
			l = readTemperaturer (filePath, lokaliteter);
			break;
		case -1:
			GameObject.Find ("Manager").GetComponent<Melding>().Show ("Feil i innlesing", filePath + " kunne ikke leses inn");
			break;
		default:
			Debug.Log("readFile(string filePath) - Something went wrong");
			break;
		}

		return l;
	}
	public List<Lokalitet> readLEInfo( string filePath)
	{

		Dictionary<string, Lokalitet> lokDict = new Dictionary<string, Lokalitet> ();
		Dictionary<string, int> headers = new Dictionary<string, int> ();

		try {
			FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
			//1. Reading from a binary Excel file ('97-2003 format; *.xls)
			IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);


			excelReader.Read();
			if (excelReader.Read()) {
				for (int i = 0; i < excelReader.FieldCount; i++) {

					string h = excelReader.GetString(i);

					//Debug.Log(h);

					if (h != null) {
						headers.Add(h, i);

					}
				}
			}


			while (excelReader.Read())
			{
				try {


					Lokalitet lok = null;

					// Finn riktig lokalitet
					int index = -1;
					if (headers.TryGetValue("Lokalitet", out index)) {
						string lokNavn = excelReader.GetString(index);

						if (lokNavn.ToUpper().Equals("FLERE")) continue;

						if (!lokDict.TryGetValue(lokNavn, out lok)) {

							lok = new Lokalitet();
							lok.setLokalitetsNavn(lokNavn);

							lokDict.Add(lokNavn, lok);

						}
					}

					index = -1;
					if (headers.TryGetValue("Lokalitet", out index)) {
						string loknavn = excelReader.GetString(index);

						lok.setLokalitetsNavn(loknavn);
					}



					Enhet enhet;
					// Finn riktig enhet

					index = -1;
					if (headers.TryGetValue("Enhet", out index)) {
						string enhetId = excelReader.GetString(index);

						enhet = lok.getEnhetById(enhetId);
						if (enhet == null) {
							enhet = lok.leggTilEnhet(enhetId);
						}
					}

					int bredIndex = -1;
					int lengIndex = -1;
					if (headers.TryGetValue("Lengdegrad", out lengIndex) && headers.TryGetValue("Breddegrad", out bredIndex)) {
						try {
							float bredde = float.Parse(excelReader.GetString(bredIndex).Replace (",", "."));
							float lengde = float.Parse(excelReader.GetString(lengIndex).Replace (",", "."));

							lok.setCoordinates(lengde, bredde);
						} catch (Exception e) {

						}
					}


				} catch (Exception e) {
					//GameObject.Find ("Manager").GetComponent<Melding>().Show (e.ToString ());
					Debug.Log(e);
				}
			}

			//6. Free resources (IExcelDataReader is IDisposable)
			excelReader.Close();
			stream.Close();
		} catch (Exception e) {
			//GameObject.Find ("Manager").GetComponent<Melding>().Show (e.ToString ());
			Debug.Log(e);
		}

		return new List<Lokalitet>(lokDict.Values);
	}

	public List<Lokalitet> readData( string filePath, List<Lokalitet> lokaliteter)
	{
		Dictionary<string, Lokalitet> lokDict = new Dictionary<string, Lokalitet> ();
		List<string> headers = new List<string> ();
		Manager.datatyper = new List<string> ();


		if (lokaliteter != null) {
			foreach (var l in lokaliteter) {
				lokDict.Add (l.getLokalitetsnavn (), l);
			}
		}



		try {
			FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

			//1. Reading from a binary Excel file ('97-2003 format; *.xls)
			IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);


			int lokNavnIndex = -1, datoIndex = -1, enhetIndex = -1, antLusTellIndex = -1;


			excelReader.Read();
			if (excelReader.Read()) {
				for (int i = 0; i < excelReader.FieldCount; i++) {

					string h = excelReader.GetString(i);

					//Debug.Log(h);

					if (h != null) {
						headers.Add(h);
						if (h.Equals("Lokalitet")) {
							lokNavnIndex = i;
						} else if (h.Equals("Enhet")){
							enhetIndex = i;
						} else if (h.Equals("Siste dato for lusetelling i perioden")) {
							datoIndex = i;
						} else if (h.Equals("Antall lusetellinger i perioden")) {
							antLusTellIndex = i;
						} else if (!h.ToUpper().Contains("DATO")) {
							Manager.datatyper.Add(h);
						}
					}
				}
			}

			if (lokNavnIndex == -1 || enhetIndex == -1 || datoIndex == -1 || antLusTellIndex == -1) {
				Debug.Log ("Lokalitet, Enhet, Dato eller Antall Lusetellinger er ikke med");
				return new List<Lokalitet>();
			}


			while (excelReader.Read())
			{
				try {

					int antLusTell = excelReader.GetInt32(antLusTellIndex);

					if (antLusTell <= 0) {
						continue;
					}


					Lokalitet lok = null;
					string lokNavn;

					// Finn riktig lokalitet
					lokNavn = excelReader.GetString(lokNavnIndex);
					if (lokNavn.ToUpper().Equals("FLERE")) continue;

					if (!lokDict.TryGetValue(lokNavn, out lok)) {

						continue;

					}


					Enhet enhet;
					string enhetId;

					// Finn riktig enhet

					enhetId = excelReader.GetString(enhetIndex);

					enhet = lok.getEnhetById(enhetId);

					if (enhet == null) {
						enhet = lok.leggTilEnhet(enhetId);
					}






					DateTime dato;
					dato = DateTime.Parse(excelReader.GetString(datoIndex));
					Måling m = new Måling(dato);

					for (int i = 0; i < excelReader.FieldCount; i++) {
						try {

							if (!headers[i].ToUpper().Contains("DATO")) {
								double data = double.Parse(excelReader.GetString(i).Replace(",", "."));


								m.AddData(headers[i], data);

								//.Log("Måling lagt til: " + headers[i] + ", " + data);

							}

						} catch (Exception e) {
							//Debug.Log ("Måling ikke lagt til: " + e);
						}

					}
					enhet.leggTilMåling(m);
				} catch (Exception e) {
					//GameObject.Find ("Manager").GetComponent<Melding>().Show (e.ToString ());
				}
			}

			//6. Free resources (IExcelDataReader is IDisposable)
			excelReader.Close();
			stream.Close();
		} catch (Exception e) {
			//GameObject.Find ("Manager").GetComponent<Melding>().Show (e.ToString ());
		}

		return new List<Lokalitet>(lokDict.Values);
	}

	public List<Lokalitet> readTemperaturer( string filePath, List<Lokalitet> lokaliteter)
	{

		Dictionary<string, Lokalitet> lokDict = new Dictionary<string, Lokalitet> ();
		List<string> headers = new List<string> ();

		if (lokaliteter != null) {
			foreach (var l in lokaliteter) {
				lokDict.Add (l.getLokalitetsnavn (), l);

			}
		} 

		try {
			FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

			//1. Reading from a binary Excel file ('97-2003 format; *.xls)
			IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

			int lokNavnIndex = -1, datoIndex = -1, tempIndex = -1;

			excelReader.Read();
			if (excelReader.Read()) {
				for (int i = 0; i < excelReader.FieldCount; i++) {

					string h = excelReader.GetString(i);

					//Debug.Log(h);

					if (h != null) {
						headers.Add(h);
						if (h.Equals("Lokalitet")) {
							lokNavnIndex = i;
						} else if (h.Equals("Utg?ende Dato") || h.Equals ("Utgående Dato")) {
							datoIndex = i;
						} else if (h.Contains("Temperatur")) {
							tempIndex= i;
						}
					}
				}
			}

			if (lokNavnIndex == -1 || datoIndex == -1 || tempIndex == -1) {
				Debug.Log ("Lokalitet, Dato eller Temperatur er ikke med");
				return new List<Lokalitet>();
			}

			while (excelReader.Read())
			{
				try {

					string tempString = excelReader.GetString(tempIndex);

					if (tempString.Equals("")) {
						continue;
					}

					Lokalitet lok = null;
					string lokNavn;

					// Finn riktig lokalitet
					lokNavn = excelReader.GetString(lokNavnIndex);
					if (lokNavn.ToUpper().Equals("FLERE")) continue;
					if (!lokDict.TryGetValue(lokNavn, out lok)) {

						continue;

					}

					float temperatur = float.Parse(tempString.Replace (",", "."));
					DateTime dato;
					dato = excelReader.GetDateTime(datoIndex);
					lok.leggTilTemperaturMåling(dato, temperatur);
					//Debug.Log(lok.getLokalitetsnavn() + " lagt til: " + dato + " " + temperatur);

				} catch (Exception e) {
					//GameObject.Find ("Manager").GetComponent<Melding>().Show (e.ToString ());
				}

			}

			//6. Free resources (IExcelDataReader is IDisposable)
			excelReader.Close();
			stream.Close();
		} catch (Exception e) {
			//GameObject.Find ("Manager").GetComponent<Melding>().Show (e.ToString ());
		}

		return new List<Lokalitet>(lokDict.Values);
	}
}
