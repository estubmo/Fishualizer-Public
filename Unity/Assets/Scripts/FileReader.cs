using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FileReader : MonoBehaviour {

	List<Lokalitet> lokaliteter = new List<Lokalitet>();

	void Awake() {
//		List<Dictionary<string,object>> data = CSVReader.Read ("Lusetellinger");
//
//		Debug.Log ("Antall linjer er: " + data.Count);
//		for (int i=0; i< data.Count; i++) {
//			string lokId = "";
//			string lokNavn = "";
//			string enhet = "";
//			string dato = "";
//			List<Data> datas = new List<Data>();
//			List<Måling> målinger = new List<Måling>();
//
//			foreach (string key in data[i].Keys) {
//				object val = data[i][key];
//				//Data d = new Data(key, val.ToString());
//				//Debug.Log(key + "  " + val);
//				if(key.Equals("Offisiell lokalitets-ID")){
//					lokId = val.ToString();
//				}else if(key.Equals("Lokalitet")){
//					lokNavn = val.ToString();
//				}else if(key.Equals("Enhet")){
//					enhet = val.ToString();
//				}else if(key.Equals("Uke")){
//					dato = val.ToString();
//				}else{
//					Data d = new Data(key, val.ToString());
//					datas.Add(d);
//				}
//			}
//
//			//Pakkerier
//			if(finnLokalitet(lokId) == null && lokId != null){
//				Lokalitet lok = new Lokalitet();
//				Måling mål = new Måling(dato, datas);
//				lok.setLokalitetsId(lokId);
//				lok.setLokalitetsNavn(lokNavn);
//				lok.leggTilEnhet(enhet);
//				lokaliteter.Add(lok);
//
//				//if(finnEnhet(enhet) == null){
//					Enhet e = new Enhet(enhet, målinger);
//				//}
//				/*else{
//					Enhet e = finnEnhet(enhet);
//				}*/
//
//
//				Debug.Log("Lokalitet " + lokId + ", " + lokNavn + " lagt til. Antall lokaliteter er " + lokaliteter.Count + ". Ant data " + datas.Count);
//			}else{
//				Lokalitet lok = finnLokalitet(lokId);
//				lok.leggTilEnhet(enhet);
//			}
//		}
//		lesLokaliteter ();
	}

	private Lokalitet finnLokalitet(string val){
		if (lokaliteter.Count > 0) {
			for (int i=0; i<lokaliteter.Count; i++) {
				if (lokaliteter [i].getLokalitetsId ().Equals(val)) {
					return lokaliteter[i];
				}
			}
		}
		return null;
	}

	private Enhet finnEnhet(List<Enhet> enheter, string enhetsId){
		if (enheter.Count > 0) {
			for (int i=0; i<enheter.Count; i++) {
				if (enheter[i].getEnhetsId().Equals(enhetsId)) {
					return enheter[i];
				}
			}
		}
		return null;
	}

	private void lesLokaliteter(){
		for(int i=0; i<lokaliteter.Count; i++){
			string lokNavn = lokaliteter[i].getLokalitetsnavn();
			int numEnheter = lokaliteter[i].getEnheter().Count;

			Debug.Log("Lokalitet " + lokNavn + " har " + numEnheter + " enheter.");
		}
	}

	//Neste gang: gjør ferdig innpakkingen.
}
